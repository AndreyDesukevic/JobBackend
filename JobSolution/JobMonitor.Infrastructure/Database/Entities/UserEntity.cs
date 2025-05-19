namespace JobMonitor.Infrastructure.Database.Entities;

public class UserEntity 
{
    public int Id { get; set; }
    public string HhId { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public HhTokenEntity? HhToken { get; set; }
    public ICollection<AppTokenEntity> AppTokens { get; set; } = new List<AppTokenEntity>();
}
