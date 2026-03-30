// MyTasksPage.tsx
// Stub page for the My Tasks section.
//
// This is a placeholder rendered at /tasks until the My Tasks Dashboard feature
// (Feature 13) builds the real page. The AuthenticatedLayout shell is fully
// functional around this stub — navigation, sidebar, and top bar all work.

export function MyTasksPage() {
  return (
    <div className="flex flex-col gap-2">
      <h1 className="text-2xl font-semibold text-foreground">My Tasks</h1>
      <p className="text-sm text-muted-foreground">
        Task dashboard coming in a future feature.
      </p>
    </div>
  );
}
