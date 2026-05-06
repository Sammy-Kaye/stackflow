import { Handle, Position, type NodeProps } from 'reactflow';
import { cn } from '@/modules/shared/lib/utils';

export interface NotificationNodeData {
  title: string;
  description?: string | null;
}

export function NotificationNode({ data, selected }: NodeProps<NotificationNodeData>) {
  return (
    <div
      className={cn(
        'min-w-[180px] max-w-[220px] rounded-lg border-l-4 border-l-teal-500 bg-card shadow-sm',
        selected && 'ring-2 ring-teal-400 ring-offset-1'
      )}
    >
      <Handle
        type="target"
        position={Position.Top}
        className="!h-3 !w-3 !border-2 !border-teal-400 !bg-white"
      />

      <div className="px-3 py-2">
        <span className="mb-1 block text-[10px] font-medium uppercase tracking-wider text-teal-500">
          Notification
        </span>
        <p className="truncate text-sm font-medium text-foreground">
          {data.title || 'Untitled'}
        </p>
        <p className="mt-0.5 text-xs text-muted-foreground">
          {data.description ? data.description.slice(0, 40) : 'No message set'}
        </p>
      </div>

      <Handle
        type="source"
        position={Position.Bottom}
        className="!h-3 !w-3 !border-2 !border-teal-400 !bg-white"
      />
    </div>
  );
}
