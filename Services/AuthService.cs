using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
    IConfiguration configuration) : IAuthService
{
    public async Task<IOperationResult<Token>> LoginAsync(string email, string password) {
        IOperationResult<User?> result = await 
            userRepository
            .GetByEmailAsync(email);

        if (!result.HasSuccessStatus || result.Payload is null) {
            return OperationResult<Token>.Failure("Invalid credentials");
        }

        User user = result.Payload;

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) {
            return OperationResult<Token>.Failure("Invalid credentials");
        }

        Token token = GenerateJwt(user);

        return OperationResult<Token>.Success(token);
    }

    public async Task<IOperationResult> RegisterAsync(string email, string password, string firstName, string lastName)
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
}