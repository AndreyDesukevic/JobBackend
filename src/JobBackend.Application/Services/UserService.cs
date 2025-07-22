using JobBackend.Domain.Interfaces.Entities;
using JobBackend.Domain.Interfaces.Repositories;
using JobBackend.Domain.Interfaces.Services;

namespace JobBackend.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _repository.GetByIdAsync(id, cancellationToken);
    }

    public Task<User?> GetByHhIdAsync(string hhId, CancellationToken cancellationToken = default)
    {
        return _repository.GetByHhIdAsync(hhId, cancellationToken);
    }

    public Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return _repository.GetAllAsync(cancellationToken);
    }

    public Task<int> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        return _repository.AddAsync(user, cancellationToken);
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        return _repository.UpdateAsync(user, cancellationToken);
    }

    public Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        return _repository.DeleteAsync(id, cancellationToken);
    }
}
