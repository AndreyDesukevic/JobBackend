using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobBackend.Infrastructure.Database.Entities;

[Table("active_jobs", Schema = "hangfire")]
public class ActiveJobEntity
{
    [Key]
    [Column("search_id")]
    public string SearchId { get; set; } = null!;

    [Column("job_id")]
    public string JobId { get; set; } = null!;
}
