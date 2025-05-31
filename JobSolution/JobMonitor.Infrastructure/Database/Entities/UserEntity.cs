using System.ComponentModel.DataAnnotations.Schema;

namespace JobMonitor.Infrastructure.Database.Entities;

[Table("users", Schema = "admin")]
public class UserEntity 
{
    [Column("id")]
    public int Id { get; set; }

    [Column("hh_id")]
    public string HhId { get; set; } = null!;

    [Column("email")]
    public string? Email { get; set; }

    [Column("first_name")]
    public required string FirstName { get; set; }

    [Column("last_name")]
    public required string LastName { get; set; }

    [Column("phone")]
    public string? Phone { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }


    public HhTokenEntity? HhToken { get; set; }
    public ICollection<AppTokenEntity> AppTokens { get; set; } = new List<AppTokenEntity>();
}
