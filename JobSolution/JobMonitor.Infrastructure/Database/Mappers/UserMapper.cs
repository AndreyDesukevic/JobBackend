using JobMonitor.Domain.Interfaces.Entities;
using JobMonitor.Infrastructure.Database.Entities;

namespace JobMonitor.Infrastructure.Database.Mappers;

public static class UserMapper
{
    public static User ToDomain(this UserEntity entity)
    {
        return new User
        {
            Id = entity.Id,
            HhId = entity.HhId,
            Email = entity.Email,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            Phone = entity.Phone,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            HhToken = entity.HhToken?.ToDomain(),
            AppTokens = entity.AppTokens.Select(a => a.ToDomain()).ToList()
        };
    }

    public static UserEntity ToEntity(this User domain)
    {
        return new UserEntity
        {
            Id = domain.Id,
            HhId = domain.HhId,
            Email = domain.Email,
            FirstName = domain.FirstName,
            LastName = domain.LastName,
            Phone = domain.Phone,
            CreatedAt = domain.CreatedAt,
            UpdatedAt = domain.UpdatedAt,
            HhToken = domain.HhToken?.ToEntity(),
            AppTokens = domain.AppTokens.Select(a => a.ToEntity()).ToList()
        };
    }
}
