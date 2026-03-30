// ActiveWorkflowsPage.tsx
// Stub page for the Active Workflows board.
//
// This is a placeholder rendered at /active until the Active Workflows Board feature
// (Feature 15) builds the real page. The AuthenticatedLayout shell is fully
// functional around this stub — navigation, sidebar, and top bar all work.

export function ActiveWorkflowsPage() {
  return (
    <div className="flex flex-col gap-2">
      <h1 className="text-2xl font-semibold text-foreground">Active Workflows</h1>
      <p className="text-sm text-muted-foreground">
        Active workflow board coming in a future feature.
      </p>
    </div>
  );
}
