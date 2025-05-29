namespace JobMonitor.Domain.Interfaces.Entities;

public class User
{
    public int Id { get; set; }
    public string HhId { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public HhToken? HhToken { get; set; }
    public List<AppToken> AppTokens { get; set; } = new();
}