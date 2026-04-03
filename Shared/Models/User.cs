namespace Shared.Models;

public class User {
    public UserUID UID { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }

    private User() { }
    public User(
        string email,
        string passwordHash,
        string firstName,
        string lastName) {
        UID = UserUID.New();
        Email = email;
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
        CreatedDate = DateTime.UtcNow;
    }

    public User Clone() =>
        MemberwiseClone() as User ??
        throw new InvalidOperationException("Unable to clone user");
}
