using JobMonitor.Domain.Interfaces.Entities;
using JobMonitor.Infrastructure.Database.Entities;

namespace JobMonitor.Infrastructure.Database.Mappers;

public static class HhTokenMapper
{
    public static HhToken ToDomain(this HhTokenEntity entity) => new(
        entity.UserId,
        entity.HhAccessToken,
        entity.HhRefreshToken,
        entity.HhExpiresAt,
        entity.CreatedAt,
        entity.UpdatedAt
    );

    public static HhTokenEntity ToEntity(this HhToken token) => new()
    {
        UserId = token.UserId,
        HhAccessToken = token.HhAccessToken,
        HhRefreshToken = token.HhRefreshToken,
        HhExpiresAt = token.HhExpiresAt,
        CreatedAt = token.CreatedAt,
        UpdatedAt = token.UpdatedAt
    };
}
