// WorkflowCard.tsx
// Displays one workflow template in the WorkflowsListPage card grid.
//
// Shows:
//   - Name (card title)
//   - Description (2-line truncation via line-clamp-2)
//   - Task count + last edited date (formatted via date-fns)
//   - IsActive badge: "Active" (default/teal) or "Draft" (secondary/gray)
//   - IsGlobal badge: "Global template" (outline) — only shown for global templates
//   - Edit button → navigates to /workflows/{id}/edit (Feature 9 builder)
//   - Delete button — Admin only, disabled when isGlobal is true
//
// The delete flow is:
//   1. User clicks Delete → DeleteWorkflowDialog opens (local open state)
//   2. User confirms → onDelete callback fires → parent handles mutation
//
// Props:
//   workflow  — WorkflowSummaryDto from the list query
//   isAdmin   — whether the current user has Admin role; hides Delete if false
//   onDelete  — called with the workflow id when the user confirms deletion;
//               the parent owns the mutation (useDeleteWorkflow) so the card
//               stays dumb about mutation state

import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { formatDistanceToNow } from 'date-fns';
import { Pencil, Trash2, ListTodo } from 'lucide-react';
import {
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
  CardContent,
  CardFooter,
} from '@/modules/shared/ui/components/card';
import { Badge } from '@/modules/shared/ui/components/badge';
import { Button } from '@/modules/shared/ui/components/button';
import { DeleteWorkflowDialog } from './DeleteWorkflowDialog';
import type { WorkflowSummaryDto } from '../../dtos/workflow-dtos';

interface WorkflowCardProps {
  workflow: WorkflowSummaryDto;
  isAdmin: boolean;
  onDelete: (id: string) => void;
  isDeleting?: boolean;
}

export function WorkflowCard({
  workflow,
  isAdmin,
  onDelete,
  isDeleting = false,
}: WorkflowCardProps) {
  const navigate = useNavigate();
  const [deleteOpen, setDeleteOpen] = useState(false);

  const lastEdited = formatDistanceToNow(new Date(workflow.updatedAt), {
    addSuffix: true,
  });

  function handleConfirmDelete() {
    onDelete(workflow.id);
    setDeleteOpen(false);
  }

  return (
    <>
      <Card className="flex flex-col">
        <CardHeader>
          {/* Badge row — active status + global indicator */}
          <div className="flex flex-wrap items-center gap-2">
            <Badge variant={workflow.isActive ? 'default' : 'secondary'}>
              {workflow.isActive ? 'Active' : 'Draft'}
            </Badge>
            {workflow.isGlobal && (
              <Badge variant="outline">Global template</Badge>
            )}
          </div>

          {/* Workflow name */}
          <CardTitle className="mt-1">{workflow.name}</CardTitle>

          {/* Description — 2-line clamp */}
          {workflow.description && (
            <CardDescription className="line-clamp-2">
              {workflow.description}
            </CardDescription>
          )}
        </CardHeader>

        {/* Meta: task count + last edited */}
        <CardContent className="flex-1">
          <div className="flex items-center gap-4 text-sm text-muted-foreground">
            <span className="flex items-center gap-1.5">
              <ListTodo className="size-3.5 shrink-0" aria-hidden="true" />
              {workflow.taskCount} {workflow.taskCount === 1 ? 'task' : 'tasks'}
            </span>
            <span>Edited {lastEdited}</span>
          </div>

          {workflow.category && (
            <p className="mt-2 text-xs text-muted-foreground">
              Category: {workflow.category}
            </p>
          )}
        </CardContent>

        {/* Action buttons */}
        <CardFooter className="gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={() => navigate(`/workflows/${workflow.id}/edit`)}
          >
            <Pencil aria-hidden="true" />
            Edit
          </Button>

          {isAdmin && (
            <Button
              variant="destructive"
              size="sm"
              disabled={workflow.isGlobal || isDeleting}
              onClick={() => setDeleteOpen(true)}
              title={workflow.isGlobal ? 'Global templates cannot be deleted' : undefined}
            >
              <Trash2 aria-hidden="true" />
              Delete
            </Button>
          )}
        </CardFooter>
      </Card>

      <DeleteWorkflowDialog
        workflowName={workflow.name}
        open={deleteOpen}
        onOpenChange={setDeleteOpen}
        onConfirm={handleConfirmDelete}
        isPending={isDeleting}
      />
    </>
  );
}
