using JobMonitor.Domain.Interfaces.Entities;
using JobMonitor.Domain.Interfaces.Repositories;
using JobMonitor.Infrastructure.Database.Mappers;
using Microsoft.EntityFrameworkCore;

namespace JobMonitor.Infrastructure.Database.Repositories;

public class HhTokenRepository : IHhTokenRepository
{
    private readonly ApplicationDbContext _db;

    public HhTokenRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<HhToken?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var entity = await _db.HhTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.UserId == userId, cancellationToken);

        return entity?.ToDomain();
    }

    public async Task AddOrUpdateAsync(HhToken token, CancellationToken cancellationToken = default)
    {
        var existing = await _db.HhTokens
            .FirstOrDefaultAsync(t => t.UserId == token.UserId, cancellationToken);

        if (existing is null)
        {
            _db.HhTokens.Add(token.ToEntity());
        }
        else
        {
            existing.HhAccessToken = token.HhAccessToken;
            existing.HhRefreshToken = token.HhRefreshToken;
            existing.HhExpiresAt = token.HhExpiresAt;
            existing.UpdatedAt = token.UpdatedAt;
            existing.CreatedAt = token.CreatedAt;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int userId, CancellationToken cancellationToken = default)
    {
        var entity = await _db.HhTokens.FirstOrDefaultAsync(t => t.UserId == userId, cancellationToken);
        if (entity is not null)
        {
            _db.HhTokens.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}
