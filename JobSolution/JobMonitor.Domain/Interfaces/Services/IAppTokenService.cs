using JobMonitor.Domain.Interfaces.Entities;

namespace JobMonitor.Domain.Interfaces.Services;

public interface IAppTokenService
{
    Task AddAsync(AppToken token, CancellationToken cancellationToken);
    Task<IEnumerable<AppToken>> GetByUserIdAsync(int userId, CancellationToken cancellationToken);
    Task<AppToken?> GetByAccessTokenAsync(string accessToken, CancellationToken cancellationToken);
    Task RemoveAsync(AppToken token, CancellationToken cancellationToken);
}
