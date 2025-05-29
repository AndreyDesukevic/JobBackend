using JobMonitor.Domain.Interfaces.Entities;

namespace JobMonitor.Domain.Interfaces.Services;

public interface IHhTokenService
{
    Task<HhToken?> GetByUserIdAsync(int userId, CancellationToken cancellationToken);
    Task AddAsync(HhToken token, CancellationToken cancellationToken);
    Task UpdateAsync(HhToken token, CancellationToken cancellationToken);
}
