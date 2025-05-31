using JobMonitor.Domain.Interfaces.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobMonitor.Infrastructure.Database.Entities;

[Table("app_tokens", Schema = "admin")]
public class AppTokenEntity 
{
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("access_token")]
    public string AccessToken { get; set; } = null!;

    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }


    public UserEntity User { get; set; } = null!;
}
