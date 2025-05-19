namespace JobMonitor.Domain.Interfaces.Entities;

public class HhToken
{
    public int UserId { get; }
    public string HhAccessToken { get; }
    public string HhRefreshToken { get; }
    public DateTime HhExpiresAt { get; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; }

    public HhToken(
        int userId,
        string hhAccessToken,
        string hhRefreshToken,
        DateTime hhExpiresAt,
        DateTime createdAt,
        DateTime updatedAt)
    {
        UserId = userId;
        HhAccessToken = hhAccessToken;
        HhRefreshToken = hhRefreshToken;
        HhExpiresAt = hhExpiresAt;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}
