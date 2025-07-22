using JobBackend.Domain.Interfaces.Entities;

namespace JobBackend.Domain.Interfaces.Services;

public interface IHhTokenService
{
    Task<HhToken?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task AddAsync(HhToken token, CancellationToken cancellationToken);
    Task UpdateAsync(HhToken token, CancellationToken cancellationToken);
}
