using Shared.Interfaces.ReturnResults;

namespace Shared.Interfaces.Services;

public interface IAuthService {
    Task<IOperationResult> RegisterAsync(
        string email, 
        string password, 
        string firstName, 
        string lastName);
    Task<IOperationResult<Token>> LoginAsync(string email, string password);
}