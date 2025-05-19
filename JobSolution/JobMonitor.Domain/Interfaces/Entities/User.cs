namespace JobMonitor.Domain.Interfaces.Entities;

public class User
{
    public int Id { get; }
    public string HhId { get; }
    public string Email { get; }
    public string? FirstName { get; }
    public string? LastName { get; }
    public string? Phone { get; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; }

    public User(
        int id,
        string hhId,
        string email,
        string? firstName,
        string? lastName,
        string? phone,
        DateTime createdAt,
        DateTime updatedAt)
    {
        Id = id;
        HhId = hhId;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        Phone = phone;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}