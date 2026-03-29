using StackFlow.Domain.Enums;

namespace StackFlow.Domain.Models;

// A user who is a member of a workspace.
// Phase 2 adds password hash, OTP, Google OAuth, and refresh token fields to this entity.
public class User
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation property — EF uses this; User itself has no EF dependency.
    public Workspace Workspace { get; set; } = null!;
}
