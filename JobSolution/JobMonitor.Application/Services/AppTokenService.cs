using JobMonitor.Domain.Interfaces.Entities;
using JobMonitor.Domain.Interfaces.Repositories;
using JobMonitor.Domain.Interfaces.Services;

namespace JobMonitor.Application.Services;

public class AppTokenService : IAppTokenService
{
    private readonly IAppTokenRepository _tokenRepository;

    public AppTokenService(IAppTokenRepository tokenRepository)
    {
        _tokenRepository = tokenRepository;
    }

    public Task AddAsync(AppToken token, CancellationToken cancellationToken)
    {
        return _tokenRepository.AddAsync(token, cancellationToken);
    }

    public Task<IEnumerable<AppToken>> GetByUserIdAsync(int userId, CancellationToken cancellationToken)
    {
        return _tokenRepository.GetByUserIdAsync(userId, cancellationToken);
    }

    public Task<AppToken?> GetByAccessTokenAsync(string accessToken, CancellationToken cancellationToken)
    {
        return _tokenRepository.GetByAccessTokenAsync(accessToken, cancellationToken);
    }

    public Task RemoveAsync(AppToken token, CancellationToken cancellationToken)
    {
        return _tokenRepository.RemoveAsync(token, cancellationToken);
    }
}
