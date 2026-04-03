using Shared;
using Shared.Interfaces.DataManagement.DataAccessor;
using Shared.Interfaces.Repositories;
using Shared.Interfaces.ReturnResults;
using Shared.Models;
using Shared.ReturnResult;

namespace Repositories;

public class RefreshTokenRepository(IDataAccessor dataAccessor) : IRefreshTokenRepository
{
    public async Task<IOperationResult> CreateAsync(RefreshToken refreshToken) =>
        await dataAccessor.ExecuteAsync(
            function: "create_refresh_token",
            parameters: new {
                p_uid = refreshToken.UID,
                p_user_uid = refreshToken.UserUID.Value,
                p_token_hash = refreshToken.TokenHash,
                p_expires_at = refreshToken.ExpiresAt
            });
            
    public async Task<IOperationResult> DeleteAsync(RefreshTokenUID uid) =>
        await dataAccessor.ExecuteAsync(
            function: "delete_refresh_token",
            parameters: new { p_uid = uid });

    public async Task<IOperationResult> DeleteByUserUIDAsync(UserUID uid) =>
        await dataAccessor.ExecuteAsync(
            function: "delete_refresh_tokens_by_user",
            parameters: new { p_user_uid = uid });

    public async Task<IOperationResult<RefreshToken>> GetByTokenHashAsync(string tokenHash) {
        IOperationResult<IEnumerable<RefreshToken>> result = await
            dataAccessor
            .QueryAsync<RefreshToken>(
                function: "get_refresh_token",
                parameters: new { p_token_hash = tokenHash });

        if (!result.HasSuccessStatus) {
            return OperationResult<RefreshToken>.Failure(result.Message);
        }

        RefreshToken? token = result.Payload?.FirstOrDefault();

        return token is not null
            ? OperationResult<RefreshToken>.Success(token)
            : OperationResult<RefreshToken>.Failure("Refresh token not found");
    }
}
