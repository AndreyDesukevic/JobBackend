using JobMonitor.Domain.Interfaces.Entities;

namespace JobMonitor.Domain.Interfaces.Services;

public interface IUserService
{
    Task<User> GetByHhIdAsync(string hhId);
    Task CreateUserAsync(User user);
    Task UpdateUserAsync(User user);
}
