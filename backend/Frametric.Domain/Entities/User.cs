using Frametric.Domain.Enums;

namespace Frametric.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Username { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public UserRole Role { get; private set; }
    public bool CanManageCatalog { get; private set; }
    public bool CanAddUsers { get; private set; }
    public bool CanDeleteUsers { get; private set; }
    public bool CanPromoteToAdmin { get; private set; }
    public bool SuperAdminNotificationSent { get; private set; }
    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiry { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public ICollection<MovieLike> MovieLikes { get; private set; } = new List<MovieLike>();
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
    public ICollection<ImportHistory> ImportHistories { get; private set; } = new List<ImportHistory>();

    private User() { } // For EF Core

    public User(Guid id, string username, string email, string passwordHash)
    {
        Id = id;
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        Role = UserRole.User;
        CreatedAt = DateTime.UtcNow;
    }

    public void PromoteToAdmin()
    {
        Role = UserRole.Admin;
    }

    public void DemoteToUser()
    {
        Role = UserRole.User;
        // Strip permissions on demotion
        CanManageCatalog = false;
        CanAddUsers = false;
        CanDeleteUsers = false;
        CanPromoteToAdmin = false;
    }

    public void UpdatePermissions(bool canManageCatalog, bool canAddUsers, bool canDeleteUsers, bool canPromoteToAdmin)
    {
        CanManageCatalog = canManageCatalog;
        CanAddUsers = canAddUsers;
        CanDeleteUsers = canDeleteUsers;
        CanPromoteToAdmin = canPromoteToAdmin;
    }

    public void SetSuperAdminNotificationSent(bool sent)
    {
        SuperAdminNotificationSent = sent;
    }

    public void UpdateRole(UserRole newRole)
    {
        Role = newRole;
    }

    public void SetPasswordResetToken(string token, DateTime expiry)
    {
        PasswordResetToken = token;
        PasswordResetTokenExpiry = expiry;
    }

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        PasswordResetToken = null;
        PasswordResetTokenExpiry = null;
    }
}

