import { CheckSquare, ThumbsUp, GitBranch, Bell, Users, Clock } from 'lucide-react';
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from '@/modules/shared/ui/components/tooltip';

interface PaletteItem {
  type: string;
  label: string;
  icon: React.ReactNode;
  color: string;
  disabled?: boolean;
}

const PALETTE_ITEMS: PaletteItem[] = [
  {
    type: 'task',
    label: 'Task',
    icon: <CheckSquare className="size-4" />,
    color: 'border-blue-400 text-blue-600 bg-blue-50',
  },
  {
    type: 'approval',
    label: 'Approval',
    icon: <ThumbsUp className="size-4" />,
    color: 'border-violet-400 text-violet-600 bg-violet-50',
  },
  {
    type: 'condition',
    label: 'Condition',
    icon: <GitBranch className="size-4" />,
    color: 'border-amber-400 text-amber-600 bg-amber-50',
  },
  {
    type: 'notification',
    label: 'Notification',
    icon: <Bell className="size-4" />,
    color: 'border-teal-400 text-teal-600 bg-teal-50',
  },
  {
    type: 'externalstep',
    label: 'External Step',
    icon: <Users className="size-4" />,
    color: 'border-slate-400 text-slate-600 bg-slate-50',
    disabled: true,
  },
  {
    type: 'deadline',
    label: 'Deadline',
    icon: <Clock className="size-4" />,
    color: 'border-red-400 text-red-600 bg-red-50',
    disabled: true,
  },
];

export function NodePalette() {
  const handleDragStart = (event: React.DragEvent, nodeType: string, disabled?: boolean) => {
    if (disabled) {
      event.preventDefault();
      return;
    }
    event.dataTransfer.setData('application/reactflow', nodeType);
    event.dataTransfer.effectAllowed = 'move';
  };

  return (
    <TooltipProvider>
      <aside className="flex w-[120px] shrink-0 flex-col gap-2 border-r border-border bg-background p-2">
        <p className="px-1 text-[10px] font-semibold uppercase tracking-wider text-muted-foreground">
          Nodes
        </p>

        {PALETTE_ITEMS.map((item) => {
          const chip = (
            <div
              draggable={!item.disabled}
              onDragStart={(e) => handleDragStart(e, item.type, item.disabled)}
              className={`flex flex-col items-center gap-1.5 rounded-lg border px-2 py-2.5 text-center transition-colors ${item.color} ${
                item.disabled
                  ? 'cursor-not-allowed opacity-50'
                  : 'cursor-grab active:cursor-grabbing hover:brightness-95'
              }`}
            >
              {item.icon}
              <span className="text-[10px] font-medium leading-tight">{item.label}</span>
            </div>
          );

          if (item.disabled) {
            return (
              <Tooltip key={item.type}>
                <TooltipTrigger className="block w-full text-left">
                  {chip}
                </TooltipTrigger>
                <TooltipContent side="right">Coming in Phase 2</TooltipContent>
              </Tooltip>
            );
          }

          return chip;
        })}
      </aside>
    </TooltipProvider>
  );
}
