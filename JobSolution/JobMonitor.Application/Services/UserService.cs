using JobMonitor.Domain.Interfaces.Entities;
using JobMonitor.Domain.Interfaces.Repositories;
using JobMonitor.Domain.Interfaces.Services;

namespace JobMonitor.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User> GetByHhIdAsync(string hhId)
    {
        return await _userRepository.GetByHhIdAsync(hhId);
    }

    public async Task CreateUserAsync(User user)
    {
        await _userRepository.AddAsync(user);
    }

    public async Task UpdateUserAsync(User user)
    {
        await _userRepository.UpdateAsync(user);
    }
}
