using JobBackend.Domain.Interfaces.Entities;
using JobBackend.Domain.Interfaces.Repositories;
using JobBackend.Infrastructure.Database.Mappers;
using Microsoft.EntityFrameworkCore;

namespace JobBackend.Infrastructure.Database.Repositories;

public class AppTokenRepository : IAppTokenRepository
{
    private readonly ApplicationDbContext _dbContext;

    public AppTokenRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<AppToken>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var entities = await _dbContext.AppTokens
            .Where(t => t.UserId == userId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return entities.Select(e => e.ToDomain());
    }

    public async Task<AppToken?> GetLatestByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.AppTokens
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.ExpiresAt)
            .FirstOrDefaultAsync(cancellationToken);

        return entity?.ToDomain();
    }

    public async Task AddAsync(AppToken token, CancellationToken cancellationToken = default)
    {
        var entity = token.ToEntity();
        _dbContext.AppTokens.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var tokens = await _dbContext.AppTokens
            .Where(t => t.UserId == userId)
            .ToListAsync(cancellationToken);

        _dbContext.AppTokens.RemoveRange(tokens);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(AppToken token, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.AppTokens
             .FirstOrDefaultAsync(t => t.UserId == token.UserId, cancellationToken);

        if (existing == null)
        {
            throw new InvalidOperationException("AppToken not found for update");
        }

        existing.AccessToken = token.AccessToken;
        existing.ExpiresAt = token.ExpiresAt;
        existing.UpdatedAt = token.UpdatedAt;

        _dbContext.AppTokens.Update(existing);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
