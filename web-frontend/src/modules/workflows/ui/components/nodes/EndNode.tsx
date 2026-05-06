import { Handle, Position } from 'reactflow';

export function EndNode() {
  return (
    <div className="flex h-10 w-24 items-center justify-center rounded-full bg-muted text-sm font-semibold text-muted-foreground shadow-sm">
      <Handle
        type="target"
        position={Position.Top}
        className="!h-3 !w-3 !border-2 !border-border !bg-white"
      />
      End
    </div>
  );
}
