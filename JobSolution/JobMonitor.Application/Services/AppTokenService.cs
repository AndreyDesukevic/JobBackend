using JobMonitor.Domain.Interfaces.Entities;
using JobMonitor.Domain.Interfaces.Repositories;
using JobMonitor.Domain.Interfaces.Services;

namespace JobMonitor.Application.Services;

public class AppTokenService : IAppTokenService
{
    private readonly IAppTokenRepository _appTokenRepository;

    public AppTokenService(IAppTokenRepository appTokenRepository)
    {
        _appTokenRepository = appTokenRepository;
    }

    public async Task AddAsync(AppToken token)
    {
        await _appTokenRepository.AddAsync(token);
    }
}
