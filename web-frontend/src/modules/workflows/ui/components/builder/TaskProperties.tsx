import type { TaskNodeData } from '../nodes/TaskNode';

interface TaskPropertiesProps {
  nodeData: TaskNodeData;
  onChange: (field: string, value: string | number | null) => void;
}

export function TaskProperties({ nodeData, onChange }: TaskPropertiesProps) {
  return (
    <div className="flex flex-col gap-4">
      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-medium text-foreground">
          Title <span className="text-destructive">*</span>
        </label>
        <input
          type="text"
          value={nodeData.title}
          onChange={(e) => onChange('title', e.target.value)}
          placeholder="Enter task title"
          className="rounded-lg border border-border bg-background px-3 py-1.5 text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring/50"
        />
      </div>

      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-medium text-foreground">Description</label>
        <textarea
          value={nodeData.description ?? ''}
          onChange={(e) => onChange('description', e.target.value)}
          placeholder="Optional description"
          rows={3}
          className="resize-none rounded-lg border border-border bg-background px-3 py-1.5 text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring/50"
        />
      </div>

      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-medium text-foreground">Assignee type</label>
        <select
          value={nodeData.assigneeType}
          onChange={(e) => onChange('assigneeType', e.target.value)}
          className="rounded-lg border border-border bg-background px-3 py-1.5 text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-ring/50"
        >
          <option value="Internal">Internal</option>
          <option value="External">External</option>
        </select>
      </div>

      {nodeData.assigneeType === 'Internal' && (
        <div className="flex flex-col gap-1.5">
          <label className="text-xs font-medium text-foreground">Assigned to email</label>
          <input
            type="email"
            value={nodeData.assigneeEmail ?? ''}
            onChange={(e) => onChange('assigneeEmail', e.target.value || null)}
            placeholder="team@example.com"
            className="rounded-lg border border-border bg-background px-3 py-1.5 text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring/50"
          />
        </div>
      )}

      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-medium text-foreground">Due offset (days)</label>
        <input
          type="number"
          min={0}
          value={nodeData.dueAtOffsetDays ?? ''}
          onChange={(e) =>
            onChange('dueAtOffsetDays', e.target.value === '' ? null : parseInt(e.target.value, 10))
          }
          placeholder="e.g. 3"
          className="rounded-lg border border-border bg-background px-3 py-1.5 text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring/50"
        />
        <p className="text-[11px] text-muted-foreground">
          Days after workflow starts that this task is due.
        </p>
      </div>
    </div>
  );
}
