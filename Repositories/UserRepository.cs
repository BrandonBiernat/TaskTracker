using Shared;
using Shared.Interfaces.DataManagement.DataAccessor;
using Shared.Interfaces.Repositories;
using Shared.Interfaces.ReturnResults;
using Shared.Models;
using Shared.ReturnResult;

namespace Repositories;

public class UserRepository(IDataAccessor dataAccessor) : IUserRepository {
    public async Task<IOperationResult> CreateAsync(User user) =>
        await dataAccessor.ExecuteAsync(
            function: "create_user",
            parameters: new {
                p_uid = user.UID.Value,
                p_email = user.Email,
                p_password_hash = user.PasswordHash,
                p_first_name = user.FirstName,
                p_last_name = user.LastName
            });

    public async Task<IOperationResult> UpdateAsync(User user) =>
        await dataAccessor.ExecuteAsync(
            function: "update_user",
            parameters: new {
                p_uid = user.UID.Value,
                p_password_hash = user.PasswordHash,
                p_first_name = user.FirstName,
                p_last_name = user.LastName
            });

    public async Task<IOperationResult> Delete(IEnumerable<UserUID> uids) =>
        await dataAccessor.ExecuteAsync("delete_users",
              new { uid_list = uids.Select(u => u.Value).ToArray() });
    public async Task<IOperationResult> Delete(UserUID uid) => await Delete([uid]);

    public async Task<IOperationResult<IEnumerable<User>>> GetAllAsync() => 
        await dataAccessor.QueryAsync<User>("get_all_users");

    public async Task<IOperationResult<IEnumerable<User>>> GetByEmailAsync(IEnumerable<string> emails) =>
        await dataAccessor.QueryAsync<User>(
              function: "get_users_by_email",
              parameters: new { email_list = emails.ToArray() });
    public async Task<IOperationResult<User?>> GetByEmailAsync(string email) {
        IOperationResult<IEnumerable<User>> result = await GetByEmailAsync([email]);
        return !result.HasSuccessStatus
            ? OperationResult<User?>.Failure(result.Message)
            : OperationResult<User?>.Success(result.Payload?.FirstOrDefault());
    }
        
    public async Task<IOperationResult<IEnumerable<User>>> GetByUidAsync(IEnumerable<UserUID> uids) =>
        await dataAccessor.QueryAsync<User>(
            function: "get_users_by_uid",
            parameters: new { uid_list = uids.Select(uid => uid.Value).ToArray() });
    public async Task<IOperationResult<User?>> GetByUidAsync(UserUID uid) {
        IOperationResult<IEnumerable<User>> result = await GetByUidAsync([uid]);
        return !result.HasSuccessStatus
              ? OperationResult<User?>.Failure(result.Message)
              : OperationResult<User?>.Success(result.Payload?.FirstOrDefault());
    }
}