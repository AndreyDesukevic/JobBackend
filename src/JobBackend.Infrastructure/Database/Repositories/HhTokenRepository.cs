using JobBackend.Domain.Interfaces.Entities;
using JobBackend.Domain.Interfaces.Repositories;
using JobBackend.Infrastructure.Database.Mappers;
using Microsoft.EntityFrameworkCore;

namespace JobBackend.Infrastructure.Database.Repositories;

public class HhTokenRepository : IHhTokenRepository
{
    private readonly ApplicationDbContext _dbContext;

    public HhTokenRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HhToken?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.HhTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.UserId == userId, cancellationToken);

        return entity?.ToDomain();
    }

    public async Task AddAsync(HhToken token, CancellationToken cancellationToken = default)
    {
        var entity = token.ToEntity();
        _dbContext.HhTokens.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(HhToken token, CancellationToken cancellationToken = default)
    {
        var entity = token.ToEntity();
        _dbContext.HhTokens.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
