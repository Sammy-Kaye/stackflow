// DeleteWorkflowDialog.tsx
// Destructive confirmation dialog for workflow deletion.
//
// Shown when the user clicks the Delete button on a WorkflowCard.
// Displays a clear warning and requires an explicit "Delete" click to proceed.
// The Cancel button dismisses without making any API call.
//
// Props:
//   workflowName — displayed in the description so the user knows what will
//                  be deleted (avoids "did I click the right one?" confusion)
//   open         — controlled open state from the parent card
//   onOpenChange — called by the dialog when it wants to change open state
//   onConfirm    — called when the user clicks "Delete" — the parent fires
//                  the mutation
//   isPending    — disables both buttons while the delete mutation is running

import {
  AlertDialog,
  AlertDialogContent,
  AlertDialogHeader,
  AlertDialogFooter,
  AlertDialogTitle,
  AlertDialogDescription,
  AlertDialogCancel,
  AlertDialogAction,
} from '@/modules/shared/ui/components/alert-dialog';

interface DeleteWorkflowDialogProps {
  workflowName: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirm: () => void;
  isPending?: boolean;
}

export function DeleteWorkflowDialog({
  workflowName,
  open,
  onOpenChange,
  onConfirm,
  isPending = false,
}: DeleteWorkflowDialogProps) {
  return (
    <AlertDialog open={open} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Delete this workflow?</AlertDialogTitle>
          <AlertDialogDescription>
            <strong className="font-medium text-foreground">{workflowName}</strong> will be
            permanently deleted along with all its task definitions. This cannot be undone.
          </AlertDialogDescription>
        </AlertDialogHeader>

        <AlertDialogFooter className="mt-6">
          <AlertDialogCancel
            disabled={isPending}
            onClick={() => onOpenChange(false)}
          >
            Cancel
          </AlertDialogCancel>

          <AlertDialogAction onClick={onConfirm} disabled={isPending}>
            {isPending ? 'Deleting…' : 'Delete'}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
