namespace JobMonitor.Domain.Interfaces.Entities
{
    public class AppToken
    {
        public int Id { get; }
        public int UserId { get; }
        public string AccessToken { get; }
        public DateTime ExpiresAt { get; }
        public DateTime CreatedAt { get; }

        public AppToken(int id, int userId, string accessToken, DateTime expiresAt, DateTime createdAt)
        {
            Id = id;
            UserId = userId;
            AccessToken = accessToken;
            ExpiresAt = expiresAt;
            CreatedAt = createdAt;
        }
    }
}
