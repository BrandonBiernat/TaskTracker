using Shared.Interfaces.ReturnResults;
using Shared.Models;

namespace Shared.Interfaces.Repositories;

public interface IUserRepository
{
    Task<IOperationResult<IEnumerable<User>>> GetAllAsync();
    Task<IOperationResult<IEnumerable<User>>> GetByUidAsync(IEnumerable<UserUID> uids);
    Task<IOperationResult<User?>> GetByUidAsync(UserUID uid);
    Task<IOperationResult<IEnumerable<User>>> GetByEmailAsync(IEnumerable<string> emails);
    Task<IOperationResult<User?>> GetByEmailAsync(string email);

    Task<IOperationResult> CreateAsync(User user);
    Task<IOperationResult> UpdateAsync(User user);

    Task<IOperationResult> Delete(IEnumerable<UserUID> uids);
    Task<IOperationResult> Delete(UserUID uid);
}