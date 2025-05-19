using JobMonitor.Domain.Interfaces.Entities;

namespace JobMonitor.Domain.Interfaces.Repositories;

public interface IHhTokenRepository
{
    Task<HhToken?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task AddOrUpdateAsync(HhToken token, CancellationToken cancellationToken = default);
    Task DeleteAsync(int userId, CancellationToken cancellationToken = default);
}
