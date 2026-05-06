import type { NotificationNodeData } from '../nodes/NotificationNode';

interface NotificationPropertiesProps {
  nodeData: NotificationNodeData;
  onChange: (field: string, value: string | null) => void;
}

export function NotificationProperties({ nodeData, onChange }: NotificationPropertiesProps) {
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
          placeholder="Notification title"
          className="rounded-lg border border-border bg-background px-3 py-1.5 text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring/50"
        />
      </div>

      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-medium text-foreground">
          Message <span className="text-destructive">*</span>
        </label>
        <textarea
          value={nodeData.description ?? ''}
          onChange={(e) => onChange('description', e.target.value || null)}
          placeholder="Notification message sent to assignees"
          rows={4}
          className="resize-none rounded-lg border border-border bg-background px-3 py-1.5 text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring/50"
        />
      </div>
    </div>
  );
}
