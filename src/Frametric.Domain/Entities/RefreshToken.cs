namespace Frametric.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    // Navigation property
    public User User { get; private set; } = null!;

    private RefreshToken() { } // For EF Core

    public RefreshToken(Guid id, Guid userId, string token, DateTime expiresAt)
    {
        Id = id;
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
    }

    public void Revoke()
    {
        RevokedAt = DateTime.UtcNow;
    }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => RevokedAt == null && !IsExpired;
}
