using JobMonitor.Domain.Interfaces.Entities;
using JobMonitor.Infrastructure.Database.Entities;

namespace JobMonitor.Infrastructure.Database.Mappers;

public static class UserMapper
{
    public static User ToDomain(this UserEntity entity) => new(
        entity.Id,
        entity.HhId,
        entity.Email,
        entity.FirstName,
        entity.LastName,
        entity.Phone,
        entity.CreatedAt,
        entity.UpdatedAt
    );

    public static UserEntity ToEntity(this User user) => new()
    {
        Id = user.Id,
        HhId = user.HhId,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Phone = user.Phone,
        CreatedAt = user.CreatedAt,
        UpdatedAt = user.UpdatedAt
    };
}
