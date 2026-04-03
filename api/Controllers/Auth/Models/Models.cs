namespace api.Controllers.Auth.Models;

public class Register_RequestModel {
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

public class Login_RequestModel {
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class Refresh_RequestModel {
    public string RefreshToken { get; set; } = string.Empty;
}