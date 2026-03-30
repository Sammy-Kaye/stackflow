// WorkflowsPage.tsx
// Stub page for the Workflows section.
//
// This is a placeholder rendered at /workflows until the Workflow CRUD feature
// (Feature 8) builds the real page. The AuthenticatedLayout shell is fully
// functional around this stub — navigation, sidebar, and top bar all work.

export function WorkflowsPage() {
  return (
    <div className="flex flex-col gap-2">
      <h1 className="text-2xl font-semibold text-foreground">Workflows</h1>
      <p className="text-sm text-muted-foreground">
        Workflow management coming in a future feature.
      </p>
    </div>
  );
}
