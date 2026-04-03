using Microsoft.Extensions.Configuration;
using NSubstitute;
using Shared;
using Shared.Interfaces.Repositories;
using Shared.Interfaces.ReturnResults;
using Shared.Models;
using Shared.ReturnResult;
using Services;

namespace Tests;

public class AuthServiceTests
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IConfiguration _configuration;
    private readonly AuthService _authService;

    private static User CreateTestUser(string email = "test@example.com", string password = "Password123!")
    {
        return new User(
            email: email,
            passwordHash: BCrypt.Net.BCrypt.HashPassword(password),
            firstName: "John",
            lastName: "Doe");
    }

    public AuthServiceTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "this-is-a-test-key-that-is-at-least-32-bytes-long",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience"
            })
            .Build();

        _authService = new AuthService(
            _userRepository,
            _refreshTokenRepository,
            _configuration);
    }

    // --- Register ---

    [Fact]
    public async Task RegisterAsync_WithNewEmail_ReturnsSuccess()
    {
        _userRepository.GetByEmailAsync(Arg.Any<string>())
            .Returns(OperationResult<User?>.Success(null));
        _userRepository.CreateAsync(Arg.Any<User>())
            .Returns(OperationResult.Success());

        IOperationResult result = await _authService.RegisterAsync(
            "new@example.com", "Password123!", "Jane", "Doe");

        Assert.True(result.HasSuccessStatus);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ReturnsFailure()
    {
        User existing = CreateTestUser();
        _userRepository.GetByEmailAsync("test@example.com")
            .Returns(OperationResult<User?>.Success(existing));

        IOperationResult result = await _authService.RegisterAsync(
            "test@example.com", "Password123!", "John", "Doe");

        Assert.False(result.HasSuccessStatus);
        Assert.Equal("A user with this email already exists.", result.Message);
    }

    [Fact]
    public async Task RegisterAsync_HashesPassword()
    {
        _userRepository.GetByEmailAsync(Arg.Any<string>())
            .Returns(OperationResult<User?>.Success(null));
        _userRepository.CreateAsync(Arg.Any<User>())
            .Returns(OperationResult.Success());

        await _authService.RegisterAsync(
            "new@example.com", "Password123!", "Jane", "Doe");

        await _userRepository.Received(1).CreateAsync(
            Arg.Is<User>(u => u.PasswordHash != "Password123!"
                && BCrypt.Net.BCrypt.Verify("Password123!", u.PasswordHash)));
    }

    // --- Login ---

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsTokens()
    {
        User user = CreateTestUser("test@example.com", "Password123!");
        _userRepository.GetByEmailAsync("test@example.com")
            .Returns(OperationResult<User?>.Success(user));
        _refreshTokenRepository.CreateAsync(Arg.Any<RefreshToken>())
            .Returns(OperationResult.Success());

        IOperationResult<AuthTokens> result = await _authService.LoginAsync(
            "test@example.com", "Password123!");

        Assert.True(result.HasSuccessStatus);
        Assert.NotNull(result.Payload);
        Assert.False(string.IsNullOrEmpty(result.Payload!.AccessToken.Value));
        Assert.False(string.IsNullOrEmpty(result.Payload.RefreshToken));
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ReturnsFailure()
    {
        User user = CreateTestUser("test@example.com", "Password123!");
        _userRepository.GetByEmailAsync("test@example.com")
            .Returns(OperationResult<User?>.Success(user));

        IOperationResult<AuthTokens> result = await _authService.LoginAsync(
            "test@example.com", "WrongPassword!");

        Assert.False(result.HasSuccessStatus);
        Assert.Equal("Invalid credentials.", result.Message);
    }

    [Fact]
    public async Task LoginAsync_WithNonexistentEmail_ReturnsFailure()
    {
        _userRepository.GetByEmailAsync("nobody@example.com")
            .Returns(OperationResult<User?>.Success(null));

        IOperationResult<AuthTokens> result = await _authService.LoginAsync(
            "nobody@example.com", "Password123!");

        Assert.False(result.HasSuccessStatus);
        Assert.Equal("Invalid credentials.", result.Message);
    }

    [Fact]
    public async Task LoginAsync_SameErrorForWrongEmailAndWrongPassword()
    {
        User user = CreateTestUser("test@example.com", "Password123!");
        _userRepository.GetByEmailAsync("test@example.com")
            .Returns(OperationResult<User?>.Success(user));
        _userRepository.GetByEmailAsync("wrong@example.com")
            .Returns(OperationResult<User?>.Success(null));

        IOperationResult<AuthTokens> wrongEmail = await _authService.LoginAsync(
            "wrong@example.com", "Password123!");
        IOperationResult<AuthTokens> wrongPassword = await _authService.LoginAsync(
            "test@example.com", "WrongPassword!");

        Assert.Equal(wrongEmail.Message, wrongPassword.Message);
    }

    [Fact]
    public async Task LoginAsync_StoresRefreshTokenHash()
    {
        User user = CreateTestUser("test@example.com", "Password123!");
        _userRepository.GetByEmailAsync("test@example.com")
            .Returns(OperationResult<User?>.Success(user));
        _refreshTokenRepository.CreateAsync(Arg.Any<RefreshToken>())
            .Returns(OperationResult.Success());

        await _authService.LoginAsync("test@example.com", "Password123!");

        await _refreshTokenRepository.Received(1).CreateAsync(
            Arg.Is<RefreshToken>(rt =>
                !string.IsNullOrEmpty(rt.TokenHash)
                && rt.UserUID == user.UID
                && rt.ExpiresAt > DateTime.UtcNow));
    }

    // --- Refresh ---

    [Fact]
    public async Task RefreshAsync_WithValidToken_ReturnsNewTokens()
    {
        User user = CreateTestUser();
        string rawToken = Convert.ToBase64String(new byte[64]);
        string tokenHash = HashToken(rawToken);

        RefreshToken storedToken = new(
            userUID: user.UID,
            tokenHash: tokenHash,
            expiresAt: DateTime.UtcNow.AddDays(7));

        _refreshTokenRepository.GetByTokenHashAsync(tokenHash)
            .Returns(OperationResult<RefreshToken>.Success(storedToken));
        _refreshTokenRepository.DeleteAsync(storedToken.UID)
            .Returns(OperationResult.Success());
        _userRepository.GetByUidAsync(user.UID)
            .Returns(OperationResult<User?>.Success(user));
        _refreshTokenRepository.CreateAsync(Arg.Any<RefreshToken>())
            .Returns(OperationResult.Success());

        IOperationResult<AuthTokens> result = await _authService.RefreshAsync(rawToken);

        Assert.True(result.HasSuccessStatus);
        Assert.NotNull(result.Payload);
    }

    [Fact]
    public async Task RefreshAsync_DeletesOldToken()
    {
        User user = CreateTestUser();
        string rawToken = Convert.ToBase64String(new byte[64]);
        string tokenHash = HashToken(rawToken);

        RefreshToken storedToken = new(
            userUID: user.UID,
            tokenHash: tokenHash,
            expiresAt: DateTime.UtcNow.AddDays(7));

        _refreshTokenRepository.GetByTokenHashAsync(tokenHash)
            .Returns(OperationResult<RefreshToken>.Success(storedToken));
        _refreshTokenRepository.DeleteAsync(storedToken.UID)
            .Returns(OperationResult.Success());
        _userRepository.GetByUidAsync(user.UID)
            .Returns(OperationResult<User?>.Success(user));
        _refreshTokenRepository.CreateAsync(Arg.Any<RefreshToken>())
            .Returns(OperationResult.Success());

        await _authService.RefreshAsync(rawToken);

        await _refreshTokenRepository.Received(1).DeleteAsync(storedToken.UID);
    }

    [Fact]
    public async Task RefreshAsync_WithExpiredToken_ReturnsFailure()
    {
        User user = CreateTestUser();
        string rawToken = Convert.ToBase64String(new byte[64]);
        string tokenHash = HashToken(rawToken);

        RefreshToken storedToken = new(
            userUID: user.UID,
            tokenHash: tokenHash,
            expiresAt: DateTime.UtcNow.AddDays(-1));

        _refreshTokenRepository.GetByTokenHashAsync(tokenHash)
            .Returns(OperationResult<RefreshToken>.Success(storedToken));
        _refreshTokenRepository.DeleteAsync(storedToken.UID)
            .Returns(OperationResult.Success());

        IOperationResult<AuthTokens> result = await _authService.RefreshAsync(rawToken);

        Assert.False(result.HasSuccessStatus);
        Assert.Equal("Refresh token expired.", result.Message);
    }

    [Fact]
    public async Task RefreshAsync_WithExpiredToken_StillDeletesOldToken()
    {
        User user = CreateTestUser();
        string rawToken = Convert.ToBase64String(new byte[64]);
        string tokenHash = HashToken(rawToken);

        RefreshToken storedToken = new(
            userUID: user.UID,
            tokenHash: tokenHash,
            expiresAt: DateTime.UtcNow.AddDays(-1));

        _refreshTokenRepository.GetByTokenHashAsync(tokenHash)
            .Returns(OperationResult<RefreshToken>.Success(storedToken));
        _refreshTokenRepository.DeleteAsync(storedToken.UID)
            .Returns(OperationResult.Success());

        await _authService.RefreshAsync(rawToken);

        await _refreshTokenRepository.Received(1).DeleteAsync(storedToken.UID);
    }

    [Fact]
    public async Task RefreshAsync_WithInvalidToken_ReturnsFailure()
    {
        _refreshTokenRepository.GetByTokenHashAsync(Arg.Any<string>())
            .Returns(OperationResult<RefreshToken>.Failure("Refresh token not found"));

        IOperationResult<AuthTokens> result = await _authService.RefreshAsync("bogus-token");

        Assert.False(result.HasSuccessStatus);
    }

    // --- Logout ---

    [Fact]
    public async Task LogoutAsync_DeletesAllUserRefreshTokens()
    {
        UserUID userUid = UserUID.New();
        _refreshTokenRepository.DeleteByUserUIDAsync(userUid)
            .Returns(OperationResult.Success());

        IOperationResult result = await _authService.LogoutAsync(userUid);

        Assert.True(result.HasSuccessStatus);
        await _refreshTokenRepository.Received(1).DeleteByUserUIDAsync(userUid);
    }

    // --- Helper ---

    private static string HashToken(string token)
    {
        byte[] bytes = System.Security.Cryptography.SHA256.HashData(
            System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}
