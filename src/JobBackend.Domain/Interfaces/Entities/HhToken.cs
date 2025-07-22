namespace JobBackend.Domain.Interfaces.Entities;

public class HhToken
{
    public int UserId { get; set; }
    public string HhAccessToken { get; set; } = null!;
    public string HhRefreshToken { get; set; } = null!;
    public DateTime HhExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}