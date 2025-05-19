using JobMonitor.Domain.Interfaces.Entities;

namespace JobMonitor.Domain.Interfaces.Services;

public interface IAppTokenService
{
    Task AddAsync(AppToken token);
}
