import { Handle, Position, type NodeProps } from 'reactflow';
import { cn } from '@/modules/shared/lib/utils';

export interface TaskNodeData {
  title: string;
  description?: string | null;
  assigneeType: string;
  assigneeEmail?: string | null;
  dueAtOffsetDays?: number | null;
  conditionConfig?: string | null;
  parentTaskId?: string | null;
}

export function TaskNode({ data, selected }: NodeProps<TaskNodeData>) {
  return (
    <div
      className={cn(
        'min-w-[180px] max-w-[220px] rounded-lg border-l-4 border-l-blue-500 bg-card shadow-sm',
        selected && 'ring-2 ring-blue-400 ring-offset-1'
      )}
    >
      <Handle
        type="target"
        position={Position.Top}
        className="!h-3 !w-3 !border-2 !border-blue-400 !bg-white"
      />

      <div className="px-3 py-2">
        <span className="mb-1 block text-[10px] font-medium uppercase tracking-wider text-blue-500">
          Task
        </span>
        <p className="truncate text-sm font-medium text-foreground">
          {data.title || 'Untitled'}
        </p>
        {data.assigneeEmail && (
          <p className="mt-0.5 truncate text-xs text-muted-foreground">
            {data.assigneeEmail}
          </p>
        )}
        {data.dueAtOffsetDays != null && (
          <span className="mt-1 inline-block rounded-full bg-blue-50 px-1.5 py-0.5 text-[10px] text-blue-600">
            +{data.dueAtOffsetDays}d
          </span>
        )}
      </div>

      <Handle
        type="source"
        position={Position.Bottom}
        className="!h-3 !w-3 !border-2 !border-blue-400 !bg-white"
      />
    </div>
  );
}
