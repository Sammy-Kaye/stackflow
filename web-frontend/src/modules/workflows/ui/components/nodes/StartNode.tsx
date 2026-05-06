import { Handle, Position } from 'reactflow';

export function StartNode() {
  return (
    <div className="flex h-10 w-24 items-center justify-center rounded-full bg-emerald-500 text-sm font-semibold text-white shadow-sm">
      Start
      <Handle
        type="source"
        position={Position.Bottom}
        className="!h-3 !w-3 !border-2 !border-emerald-500 !bg-white"
      />
    </div>
  );
}
