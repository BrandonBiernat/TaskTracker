using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Tests;

public class AuthIntegrationTests : IClassFixture<IntegrationTestFixture>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public AuthIntegrationTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Client;
    }

    private static StringContent JsonBody(object data) =>
        new(JsonSerializer.Serialize(data, JsonOptions),
            System.Text.Encoding.UTF8, "application/json");

    private async Task<(string AccessToken, string RefreshToken)> RegisterAndLogin(
        string email = "integration@test.com",
        string password = "Password123!")
    {
        await _client.PostAsync("/api/auth/register", JsonBody(new
        {
            Email = email,
            Password = password,
            FirstName = "Test",
            LastName = "User"
        }));

        HttpResponseMessage loginResponse = await _client.PostAsync("/api/auth/login",
            JsonBody(new { Email = email, Password = password }));

        JsonDocument json = await JsonDocument.ParseAsync(
            await loginResponse.Content.ReadAsStreamAsync());

        return (
            json.RootElement.GetProperty("access_token").GetString()!,
            json.RootElement.GetProperty("refresh_token").GetString()!
        );
    }

    // --- Register ---

    [Fact]
    public async Task Register_WithValidData_Returns201()
    {
        HttpResponseMessage response = await _client.PostAsync("/api/auth/register",
            JsonBody(new
            {
                Email = "register-test@test.com",
                Password = "Password123!",
                FirstName = "John",
                LastName = "Doe"
            }));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_Returns400()
    {
        string email = "duplicate@test.com";

        await _client.PostAsync("/api/auth/register", JsonBody(new
        {
            Email = email,
            Password = "Password123!",
            FirstName = "First",
            LastName = "User"
        }));

        HttpResponseMessage response = await _client.PostAsync("/api/auth/register",
            JsonBody(new
            {
                Email = email,
                Password = "Password456!",
                FirstName = "Second",
                LastName = "User"
            }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_WithMissingFields_Returns400()
    {
        HttpResponseMessage response = await _client.PostAsync("/api/auth/register",
            JsonBody(new { Email = "", Password = "" }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // --- Login ---

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokens()
    {
        string email = "login-test@test.com";
        string password = "Password123!";

        await _client.PostAsync("/api/auth/register", JsonBody(new
        {
            Email = email,
            Password = password,
            FirstName = "Login",
            LastName = "Test"
        }));

        HttpResponseMessage response = await _client.PostAsync("/api/auth/login",
            JsonBody(new { Email = email, Password = password }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        JsonDocument json = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync());
        Assert.True(json.RootElement.TryGetProperty("access_token", out _));
        Assert.True(json.RootElement.TryGetProperty("refresh_token", out _));
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        string email = "wrong-pw@test.com";

        await _client.PostAsync("/api/auth/register", JsonBody(new
        {
            Email = email,
            Password = "Password123!",
            FirstName = "Wrong",
            LastName = "Password"
        }));

        HttpResponseMessage response = await _client.PostAsync("/api/auth/login",
            JsonBody(new { Email = email, Password = "WrongPassword!" }));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithNonexistentEmail_Returns401()
    {
        HttpResponseMessage response = await _client.PostAsync("/api/auth/login",
            JsonBody(new { Email = "nobody@test.com", Password = "Password123!" }));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // --- Refresh ---

    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNewTokens()
    {
        (_, string refreshToken) = await RegisterAndLogin("refresh-test@test.com");

        HttpResponseMessage response = await _client.PostAsync("/api/auth/refresh",
            JsonBody(new { RefreshToken = refreshToken }));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        JsonDocument json = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync());
        Assert.True(json.RootElement.TryGetProperty("access_token", out _));
        Assert.True(json.RootElement.TryGetProperty("refresh_token", out _));
    }

    [Fact]
    public async Task Refresh_WithUsedToken_Fails()
    {
        (_, string refreshToken) = await RegisterAndLogin("refresh-used@test.com");

        // Use the refresh token once
        await _client.PostAsync("/api/auth/refresh",
            JsonBody(new { RefreshToken = refreshToken }));

        // Try to use the same token again — should fail (rotation)
        HttpResponseMessage response = await _client.PostAsync("/api/auth/refresh",
            JsonBody(new { RefreshToken = refreshToken }));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithInvalidToken_Returns401()
    {
        HttpResponseMessage response = await _client.PostAsync("/api/auth/refresh",
            JsonBody(new { RefreshToken = "completely-bogus-token" }));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // --- Protected Endpoints ---

    [Fact]
    public async Task ProtectedEndpoint_WithValidToken_Returns200()
    {
        (string accessToken, _) = await RegisterAndLogin("protected-test@test.com");

        HttpRequestMessage request = new(HttpMethod.Post, "/api/auth/logout");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        HttpResponseMessage response = await _client.SendAsync(request);

        // Logout should succeed with a valid token
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_Returns401()
    {
        HttpResponseMessage response = await _client.PostAsync("/api/auth/logout", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithGarbageToken_Returns401()
    {
        HttpRequestMessage request = new(HttpMethod.Post, "/api/auth/logout");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "not-a-real-jwt");

        HttpResponseMessage response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // --- Logout ---

    [Fact]
    public async Task Logout_InvalidatesRefreshTokens()
    {
        (string accessToken, string refreshToken) = await RegisterAndLogin("logout-test@test.com");

        // Logout
        HttpRequestMessage logoutRequest = new(HttpMethod.Post, "/api/auth/logout");
        logoutRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        await _client.SendAsync(logoutRequest);

        // Try to refresh — should fail
        HttpResponseMessage response = await _client.PostAsync("/api/auth/refresh",
            JsonBody(new { RefreshToken = refreshToken }));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
