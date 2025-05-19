using JobMonitor.Domain.Interfaces.Entities;

namespace JobMonitor.Infrastructure.Database.Entities;

public class AppTokenEntity 
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string AccessToken { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public UserEntity User { get; set; } = null!;
}
