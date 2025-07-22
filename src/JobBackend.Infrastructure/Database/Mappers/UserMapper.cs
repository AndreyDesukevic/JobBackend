using JobBackend.Domain.Interfaces.Entities;
using JobBackend.Infrastructure.Database.Entities;

namespace JobBackend.Infrastructure.Database.Mappers;

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
            UpdatedAt = entity.UpdatedAt
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
            UpdatedAt = domain.UpdatedAt
        };
    }
}
