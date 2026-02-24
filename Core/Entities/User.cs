namespace Core.Entities;

public class User
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = "User";
    public bool IsActive { get; set; } = true;

    // 🔐 Auth
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }

    // (اختياري – للمستقبل)
    public string? ExternalProvider { get; set; }   // Google, LinkedIn
    public string? ExternalProviderId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<Scan> Scans { get; set; } = new List<Scan>();

    // NEW: 1:1 Profile
    public Profile? Profile { get; set; }
}