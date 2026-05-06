import { Handle, Position, type NodeProps } from 'reactflow';
import { cn } from '@/modules/shared/lib/utils';
import type { TaskNodeData } from './TaskNode';

export function ApprovalNode({ data, selected }: NodeProps<TaskNodeData>) {
  return (
    <div
      className={cn(
        'min-w-[180px] max-w-[220px] rounded-lg border-l-4 border-l-violet-500 bg-card shadow-sm',
        selected && 'ring-2 ring-violet-400 ring-offset-1'
      )}
    >
      <Handle
        type="target"
        position={Position.Top}
        className="!h-3 !w-3 !border-2 !border-violet-400 !bg-white"
      />

      <div className="px-3 py-2">
        <span className="mb-1 block text-[10px] font-medium uppercase tracking-wider text-violet-500">
          Approval
        </span>
        <p className="truncate text-sm font-medium text-foreground">
          {data.title || 'Untitled'}
        </p>
        <p className="mt-0.5 text-xs text-muted-foreground">Requires approval</p>
        {data.assigneeEmail && (
          <p className="mt-0.5 truncate text-xs text-muted-foreground">
            {data.assigneeEmail}
          </p>
        )}
      </div>

      <Handle
        type="source"
        position={Position.Bottom}
        className="!h-3 !w-3 !border-2 !border-violet-400 !bg-white"
      />
    </div>
  );
}
