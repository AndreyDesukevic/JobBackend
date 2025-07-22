using JobBackend.Domain.Interfaces.Entities;
using JobBackend.Infrastructure.Database.Entities;

namespace JobBackend.Infrastructure.Database.Mappers;

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
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
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
            CreatedAt = domain.CreatedAt,
            UpdatedAt = domain.UpdatedAt
        };
    }
}
