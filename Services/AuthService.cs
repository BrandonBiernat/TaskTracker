using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Shared;
using Shared.Interfaces.Repositories;
using Shared.Interfaces.ReturnResults;
using Shared.Interfaces.Services;
using Shared.Models;
using Shared.ReturnResult;

namespace Services;

public class AuthService(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IConfiguration configuration) : IAuthService
{
    public async Task<IOperationResult<AuthTokens>> LoginAsync(
        string email, 
        string password) {
        IOperationResult<User?> result = await 
            userRepository
            .GetByEmailAsync(email);
        if (!result.HasSuccessStatus || result.Payload is null) {
            return OperationResult<AuthTokens>.Failure("Invalid credentials.");
        }

        User user = result.Payload;
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) {
            return OperationResult<AuthTokens>.Failure("Invalid credentials.");
        }

        AuthTokens tokens = await GenerateTokens(user);
        return OperationResult<AuthTokens>.Success(tokens);
    }

    public async Task<IOperationResult> LogoutAsync(UserUID userUid) =>
        await refreshTokenRepository.DeleteByUserUIDAsync(userUid);

    public async Task<IOperationResult<AuthTokens>> RefreshAsync(string refreshToken) {
        string tokenHash = HashToken(refreshToken);

        IOperationResult<RefreshToken> result = await
            refreshTokenRepository
            .GetByTokenHashAsync(tokenHash);
        if (!result.HasSuccessStatus || result.Payload is null)
            return OperationResult<AuthTokens>.Failure("Invalid refresh token.");

        RefreshToken? storedToken = result.Payload;

        await refreshTokenRepository.DeleteAsync(storedToken!.UID);
        if(storedToken.ExpiresAt < DateTime.UtcNow)
            return OperationResult<AuthTokens>.Failure("Refresh token expired.");

        IOperationResult<User?> userResult = await
            userRepository
            .GetByUidAsync(storedToken.UserUID);
        if (!userResult.HasSuccessStatus || userResult.Payload is null) 
            return OperationResult<AuthTokens>.Failure("User not found.");

        AuthTokens tokens = await GenerateTokens(userResult.Payload);
        return OperationResult<AuthTokens>.Success(tokens);
    }

    public async Task<IOperationResult> RegisterAsync(
        string email, 
        string password, 
        string firstName, 
        string lastName)
    {
        IOperationResult<User?> existingUser = await
            userRepository
            .GetByEmailAsync(email);
        if (existingUser.HasSuccessStatus && existingUser.Payload is not null) {
            return OperationResult.Failure("A user with this email already exists.");
        }

        User newUser = new(
            email: email,
            passwordHash: BCrypt.Net.BCrypt.HashPassword(password),
            firstName: firstName,
            lastName: lastName);

        return await userRepository.CreateAsync(newUser);
    }

    private async Task<AuthTokens> GenerateTokens(User user)
    {
        Token accessToken = GenerateJwt(user);

        string rawRefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        string refreshTokenHash = HashToken(rawRefreshToken);

        RefreshToken refreshToken = new(
            userUID: user.UID,
            tokenHash: refreshTokenHash,
            expiresAt: DateTime.UtcNow.AddDays(7));

        await refreshTokenRepository.CreateAsync(refreshToken);

        return new AuthTokens(accessToken, rawRefreshToken);
    }

    private Token GenerateJwt(User user)
    {
        SymmetricSecurityKey key = new (
            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));

        Claim[] claims = [
            new Claim(ClaimTypes.NameIdentifier, user.UID.Value.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName)
        ];

        JwtSecurityToken token = new (
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        string jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return new Token(jwt);
    }

    private static string HashToken(string token)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}