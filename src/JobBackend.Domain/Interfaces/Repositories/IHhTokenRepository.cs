using JobBackend.Domain.Interfaces.Entities;

namespace JobBackend.Domain.Interfaces.Repositories;

public interface IHhTokenRepository
{
    Task<HhToken?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task AddAsync(HhToken token, CancellationToken cancellationToken = default);
    Task UpdateAsync(HhToken token, CancellationToken cancellationToken = default);
}