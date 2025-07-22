using JobBackend.Domain.Interfaces.Entities;

namespace JobBackend.Domain.Interfaces.Repositories;

public interface IAppTokenRepository
{
    Task<IEnumerable<AppToken>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task AddAsync(AppToken token, CancellationToken cancellationToken = default);
    Task DeleteByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<AppToken?> GetLatestByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task UpdateAsync(AppToken appToken, CancellationToken cancellationToken = default);
}