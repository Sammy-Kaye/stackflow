namespace StackFlow.Domain.Constants;

// Fixed Guids used across the application for seeded/system entities.
// These values are stable — changing them would require a migration to update seeded rows.
public static class WellKnownIds
{
    // The demo workspace pre-populated with sample workflows for Phase 1.
    public static readonly Guid DemoWorkspaceId = new Guid("00000000-0000-0000-0000-000000000001");

    // The global workspace used to house seeded workflow templates available to all workspaces.
    public static readonly Guid GlobalWorkspaceId = new Guid("00000000-0000-0000-0000-000000000002");
}
