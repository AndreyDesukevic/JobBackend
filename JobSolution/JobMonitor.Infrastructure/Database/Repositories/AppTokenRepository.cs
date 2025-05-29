using JobMonitor.Domain.Interfaces.Entities;
using JobMonitor.Domain.Interfaces.Repositories;
using JobMonitor.Infrastructure.Database.Mappers;
using Microsoft.EntityFrameworkCore;

namespace JobMonitor.Infrastructure.Database.Repositories;

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
}
