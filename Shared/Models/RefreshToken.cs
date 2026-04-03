namespace Shared.Models;

public class RefreshToken {
    public RefreshTokenUID UID { get; }
    public UserUID UserUID { get; }
    public string TokenHash { get; } = string.Empty;
    public DateTime ExpiresAt { get; }
    public DateTime CreatedAt { get; }

    private RefreshToken() { }
    public RefreshToken(
        UserUID userUID,
        string tokenHash,
        DateTime expiresAt) {
        UID = RefreshTokenUID.New();
        UserUID = userUID;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
    }
}