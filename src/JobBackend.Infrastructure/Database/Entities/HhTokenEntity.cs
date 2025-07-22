using System.ComponentModel.DataAnnotations.Schema;

namespace JobBackend.Infrastructure.Database.Entities;

[Table("hh_tokens", Schema = "admin")]
public class HhTokenEntity 
{
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("hh_access_token")]
    public string HhAccessToken { get; set; } = null!;

    [Column("hh_refresh_token")]
    public string HhRefreshToken { get; set; } = null!;

    [Column("hh_expires_at")]
    public DateTime HhExpiresAt { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }


    public UserEntity User { get; set; } = null!;
}
