using JobMonitor.Domain.Interfaces.Entities;
using JobMonitor.Domain.Interfaces.Repositories;
using JobMonitor.Infrastructure.Database.Mappers;
using Microsoft.EntityFrameworkCore;

namespace JobMonitor.Infrastructure.Database.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _db;

    public UserRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Users.FindAsync(new object[] { id }, cancellationToken);
        return entity?.ToDomain();
    }

    public async Task<User?> GetByHhIdAsync(string hhId, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.HhId == hhId, cancellationToken);
        return entity?.ToDomain();
    }

    public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _db.Users.AsNoTracking().ToListAsync(cancellationToken);
        return entities.Select(e => e.ToDomain());
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        var entity = user.ToEntity();
        _db.Users.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        var entity = user.ToEntity();
        _db.Users.Update(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Users.FindAsync(new object[] { id }, cancellationToken);
        if (entity != null)
        {
            _db.Users.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
