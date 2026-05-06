import { Handle, Position, type NodeProps } from 'reactflow';
import { cn } from '@/modules/shared/lib/utils';

export interface ConditionNodeData {
  title: string;
  conditionConfig?: string | null;
  yesLabel?: string;
  noLabel?: string;
}

export function ConditionNode({ data, selected }: NodeProps<ConditionNodeData>) {
  return (
    <div className="relative flex items-center justify-center" style={{ width: 120, height: 120 }}>
      {/* Diamond shape via rotated square */}
      <div
        className={cn(
          'absolute inset-0 rotate-45 rounded-lg border-2 border-amber-400 bg-card shadow-sm',
          selected && 'ring-2 ring-amber-300 ring-offset-2'
        )}
      />

      <Handle
        type="target"
        position={Position.Top}
        className="!h-3 !w-3 !border-2 !border-amber-400 !bg-white"
        style={{ top: 0 }}
      />

      {/* Content sits above the rotated div */}
      <div className="relative z-10 flex flex-col items-center px-2 text-center">
        <span className="text-[10px] font-medium uppercase tracking-wider text-amber-500">
          Condition
        </span>
        <p className="mt-0.5 text-xs font-medium text-foreground leading-tight">
          {data.title || 'If / Else'}
        </p>
      </div>

      {/* Yes handle — right side */}
      <Handle
        type="source"
        position={Position.Right}
        id="yes"
        className="!h-3 !w-3 !border-2 !border-amber-400 !bg-white"
        style={{ right: 0 }}
      />
      <span
        className="absolute right-[-24px] top-1/2 -translate-y-1/2 text-[10px] font-medium text-amber-600"
        style={{ pointerEvents: 'none' }}
      >
        {(data.yesLabel as string | undefined) ?? 'Yes'}
      </span>

      {/* No handle — bottom */}
      <Handle
        type="source"
        position={Position.Bottom}
        id="no"
        className="!h-3 !w-3 !border-2 !border-amber-400 !bg-white"
        style={{ bottom: 0 }}
      />
      <span
        className="absolute bottom-[-16px] left-1/2 -translate-x-1/2 text-[10px] font-medium text-amber-600"
        style={{ pointerEvents: 'none' }}
      >
        {(data.noLabel as string | undefined) ?? 'No'}
      </span>
    </div>
  );
}
