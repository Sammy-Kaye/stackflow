import type { ConditionNodeData } from '../nodes/ConditionNode';

interface ConditionPropertiesProps {
  nodeData: ConditionNodeData;
  onChange: (field: string, value: string | null) => void;
}

export function ConditionProperties({ nodeData, onChange }: ConditionPropertiesProps) {
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
          placeholder="Condition description"
          className="rounded-lg border border-border bg-background px-3 py-1.5 text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring/50"
        />
      </div>

      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-medium text-foreground">Yes branch label</label>
        <input
          type="text"
          value={nodeData.yesLabel ?? 'Yes'}
          onChange={(e) => onChange('yesLabel', e.target.value)}
          placeholder="Yes"
          className="rounded-lg border border-border bg-background px-3 py-1.5 text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring/50"
        />
      </div>

      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-medium text-foreground">No branch label</label>
        <input
          type="text"
          value={nodeData.noLabel ?? 'No'}
          onChange={(e) => onChange('noLabel', e.target.value)}
          placeholder="No"
          className="rounded-lg border border-border bg-background px-3 py-1.5 text-sm text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring/50"
        />
      </div>

      <div className="flex flex-col gap-1.5">
        <label className="text-xs font-medium text-foreground">Condition config</label>
        <textarea
          value={nodeData.conditionConfig ?? ''}
          onChange={(e) => onChange('conditionConfig', e.target.value || null)}
          placeholder='{"field":"status","operator":"eq","value":"approved"}'
          rows={4}
          className="resize-none rounded-lg border border-border bg-background px-3 py-1.5 font-mono text-xs text-foreground placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring/50"
        />
        <p className="text-[11px] text-muted-foreground">
          JSON condition expression evaluated at runtime.
        </p>
      </div>
    </div>
  );
}
