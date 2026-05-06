// WorkflowBuilderPage.tsx
// Stub page for the workflow template builder canvas.
//
// This placeholder is rendered at both:
//   /workflows/new        — create a new workflow template
//   /workflows/:id/edit   — edit an existing workflow template
//
// The real drag-and-drop canvas (React Flow) is built in Feature 9.
// The Edit and New workflow buttons on WorkflowsListPage navigate here.

export function WorkflowBuilderPage() {
  return (
    <div className="flex flex-col gap-2">
      <h1 className="text-2xl font-semibold text-foreground">Workflow Builder</h1>
      <p className="text-sm text-muted-foreground">
        Drag-and-drop workflow builder coming in Feature 9.
      </p>
    </div>
  );
}
