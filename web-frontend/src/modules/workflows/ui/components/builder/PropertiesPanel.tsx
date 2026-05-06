import type { Node } from 'reactflow';
import { X } from 'lucide-react';
import { TaskProperties } from './TaskProperties';
import { ConditionProperties } from './ConditionProperties';
import { NotificationProperties } from './NotificationProperties';

interface PropertiesPanelProps {
  selectedNode: Node | null;
  onClose: () => void;
  onNodeDataChange: (nodeId: string, field: string, value: unknown) => void;
}

export function PropertiesPanel({ selectedNode, onClose, onNodeDataChange }: PropertiesPanelProps) {
  if (!selectedNode) return null;

  const nodeType = selectedNode.type;

  const handleChange = (field: string, value: unknown) => {
    onNodeDataChange(selectedNode.id, field, value);
  };

  return (
    <aside className="flex w-[280px] shrink-0 flex-col border-l border-border bg-background">
      <div className="flex items-center justify-between border-b border-border px-4 py-3">
        <h3 className="text-sm font-semibold text-foreground">Properties</h3>
        <button
          onClick={onClose}
          className="rounded-md p-1 text-muted-foreground hover:bg-muted hover:text-foreground"
          aria-label="Close properties panel"
        >
          <X className="size-4" />
        </button>
      </div>

      <div className="flex-1 overflow-y-auto p-4">
        {/* Node type read-only label */}
        <div className="mb-4 flex flex-col gap-1">
          <span className="text-xs font-medium text-muted-foreground">Node type</span>
          <span className="text-sm font-medium text-foreground capitalize">{nodeType}</span>
        </div>

        {(nodeType === 'task' || nodeType === 'approval') && (
          <TaskProperties
            nodeData={selectedNode.data}
            onChange={handleChange}
          />
        )}

        {nodeType === 'condition' && (
          <ConditionProperties
            nodeData={selectedNode.data}
            onChange={handleChange}
          />
        )}

        {nodeType === 'notification' && (
          <NotificationProperties
            nodeData={selectedNode.data}
            onChange={handleChange}
          />
        )}
      </div>
    </aside>
  );
}
