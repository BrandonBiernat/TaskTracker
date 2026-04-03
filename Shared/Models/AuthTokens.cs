namespace Shared.Models;

public class AuthTokens {
    public Token AccessToken { get; }
    public string RefreshToken { get; } = string.Empty;

    public AuthTokens(
        Token accesstoken, 
        string refreshToken) {
        AccessToken = accesstoken;
        RefreshToken = refreshToken;
    }
}
