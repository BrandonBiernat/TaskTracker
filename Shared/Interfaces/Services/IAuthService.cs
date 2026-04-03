using Shared.Interfaces.ReturnResults;
using Shared.Models;

namespace Shared.Interfaces.Services;

public interface IAuthService {
    Task<IOperationResult> RegisterAsync(
        string email, 
        string password, 
        string firstName, 
        string lastName);
    Task<IOperationResult<AuthTokens>> LoginAsync(string email, string password);
    Task<IOperationResult<AuthTokens>> RefreshAsync(string refreshToken);                                                                   
    Task<IOperationResult> LogoutAsync(UserUID userUid);
}