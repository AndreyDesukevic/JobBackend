using JobMonitor.Domain.Interfaces.Entities;

namespace JobMonitor.Domain.Interfaces.Repositories;

public interface IAppTokenRepository
{
    Task<AppToken?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AppToken>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task AddAsync(AppToken token, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
