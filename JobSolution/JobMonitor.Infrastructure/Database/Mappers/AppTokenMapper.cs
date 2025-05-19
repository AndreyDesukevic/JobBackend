using JobMonitor.Domain.Interfaces.Entities;
using JobMonitor.Infrastructure.Database.Entities;

namespace JobMonitor.Infrastructure.Database.Mappers;

public static class AppTokenMapper
{
    public static AppToken ToDomain(this AppTokenEntity entity) => new(
        entity.Id,
        entity.UserId,
        entity.AccessToken,
        entity.ExpiresAt,
        entity.CreatedAt
    );

    public static AppTokenEntity ToEntity(this AppToken domain) => new()
    {
        Id = domain.Id,
        UserId = domain.UserId,
        AccessToken = domain.AccessToken,
        ExpiresAt = domain.ExpiresAt,
        CreatedAt = domain.CreatedAt
    };
}
