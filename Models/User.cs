// The line below declares the namespace for the file.
// Namespaces are used in C# to organize code into groups and to prevent name conflicts.
// "AuthSystem.Models" means that all classes in this file belong to the "Models" namespace 
// inside the "AuthSystem" project or group.
namespace AuthSystem.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    // Refresh token fields
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}