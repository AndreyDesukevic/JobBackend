using JobBackend.Domain.Interfaces.Entities;
using JobBackend.Infrastructure.Database.Entities;

namespace JobBackend.Infrastructure.Database.Mappers;

public static class HhTokenMapper
{
    public static HhToken ToDomain(this HhTokenEntity entity)
    {
        return new HhToken
        {
            UserId = entity.UserId,
            HhAccessToken = entity.HhAccessToken,
            HhRefreshToken = entity.HhRefreshToken,
            HhExpiresAt = entity.HhExpiresAt,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public static HhTokenEntity ToEntity(this HhToken domain)
    {
        return new HhTokenEntity
        {
            UserId = domain.UserId,
            HhAccessToken = domain.HhAccessToken,
            HhRefreshToken = domain.HhRefreshToken,
            HhExpiresAt = domain.HhExpiresAt,
            CreatedAt = domain.CreatedAt,
            UpdatedAt = domain.UpdatedAt
        };
    }
}