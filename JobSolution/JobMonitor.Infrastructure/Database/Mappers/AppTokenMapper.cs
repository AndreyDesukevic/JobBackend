using JobMonitor.Domain.Interfaces.Entities;
using JobMonitor.Infrastructure.Database.Entities;

namespace JobMonitor.Infrastructure.Database.Mappers;

public static class AppTokenMapper
{
    public static AppToken ToDomain(this AppTokenEntity entity)
    {
        return new AppToken
        {
            Id = entity.Id,
            UserId = entity.UserId,
            AccessToken = entity.AccessToken,
            ExpiresAt = entity.ExpiresAt,
            CreatedAt = entity.CreatedAt
        };
    }

    public static AppTokenEntity ToEntity(this AppToken domain)
    {
        return new AppTokenEntity
        {
            Id = domain.Id,
            UserId = domain.UserId,
            AccessToken = domain.AccessToken,
            ExpiresAt = domain.ExpiresAt,
            CreatedAt = domain.CreatedAt
        };
    }
}
