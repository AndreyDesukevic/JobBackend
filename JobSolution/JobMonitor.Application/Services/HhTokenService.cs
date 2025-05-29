using JobMonitor.Domain.Interfaces.Entities;
using JobMonitor.Domain.Interfaces.Repositories;
using JobMonitor.Domain.Interfaces.Services;

namespace JobMonitor.Application.Services;

public class HhTokenService : IHhTokenService
{
    private readonly IHhTokenRepository _tokenRepository;

    public HhTokenService(IHhTokenRepository tokenRepository)
    {
        _tokenRepository = tokenRepository;
    }

    public Task<HhToken?> GetByUserIdAsync(int userId, CancellationToken cancellationToken)
    {
        return _tokenRepository.GetByUserIdAsync(userId, cancellationToken);
    }

    public Task AddAsync(HhToken token, CancellationToken cancellationToken)
    {
        return _tokenRepository.AddAsync(token, cancellationToken);
    }

    public Task UpdateAsync(HhToken token, CancellationToken cancellationToken)
    {
        return _tokenRepository.UpdateAsync(token, cancellationToken);
    }
}
