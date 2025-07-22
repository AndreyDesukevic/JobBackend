using JobBackend.Domain.Interfaces.Entities;

namespace JobBackend.Domain.Interfaces.Services;

public interface IAppTokenService
{
    Task AddAsync(AppToken token, CancellationToken cancellationToken);
    Task<IEnumerable<AppToken>> GetByUserIdAsync(int userId, CancellationToken cancellationToken);
    Task<AppToken?> GetLatestByUserIdAsync(int userId, CancellationToken cancellationToken);
    Task<AppToken?> GetByAccessTokenAsync(string accessToken, CancellationToken cancellationToken);
    Task RemoveAsync(AppToken token, CancellationToken cancellationToken);
    Task UpdateAsync(AppToken token, CancellationToken cancellationToken);
}
