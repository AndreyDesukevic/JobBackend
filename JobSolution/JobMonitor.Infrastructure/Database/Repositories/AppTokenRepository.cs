using JobMonitor.Domain.Interfaces.Entities;
using JobMonitor.Domain.Interfaces.Repositories;
using JobMonitor.Infrastructure.Database.Mappers;
using Microsoft.EntityFrameworkCore;

namespace JobMonitor.Infrastructure.Database.Repositories;

public class AppTokenRepository : IAppTokenRepository
{
    private readonly ApplicationDbContext _db;

    public AppTokenRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<AppToken?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.AppTokens.FindAsync(new object[] { id }, cancellationToken);
        return entity?.ToDomain();
    }

    public async Task<IEnumerable<AppToken>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var tokens = await _db.AppTokens
            .Where(t => t.UserId == userId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return tokens.Select(t => t.ToDomain());
    }

    public async Task AddAsync(AppToken token, CancellationToken cancellationToken = default)
    {
        var entity = token.ToEntity();
        _db.AppTokens.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.AppTokens.FindAsync(new object[] { id }, cancellationToken);
        if (entity is not null)
        {
            _db.AppTokens.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
