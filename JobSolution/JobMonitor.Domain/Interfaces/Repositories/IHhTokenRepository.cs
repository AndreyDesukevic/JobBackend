using JobMonitor.Domain.Interfaces.Entities;

namespace JobMonitor.Domain.Interfaces.Repositories;

public interface IHhTokenRepository
{
    Task<HhToken?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task AddAsync(HhToken token, CancellationToken cancellationToken = default);
    Task UpdateAsync(HhToken token, CancellationToken cancellationToken = default);
}