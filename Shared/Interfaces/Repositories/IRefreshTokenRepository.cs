using Shared.Interfaces.ReturnResults;
using Shared.Models;

namespace Shared.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task<IOperationResult> CreateAsync(RefreshToken refreshToken);
    Task<IOperationResult> DeleteAsync(RefreshTokenUID uid);
    Task<IOperationResult> DeleteByUserUIDAsync(UserUID uid);
    Task<IOperationResult<RefreshToken>> GetByTokenHashAsync(string tokenHash);
}
