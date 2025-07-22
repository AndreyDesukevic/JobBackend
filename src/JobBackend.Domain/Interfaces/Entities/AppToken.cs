namespace JobBackend.Domain.Interfaces.Entities;

public class AppToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string AccessToken { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}