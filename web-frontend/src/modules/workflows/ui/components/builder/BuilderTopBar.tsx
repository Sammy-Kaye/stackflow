import { useState } from 'react';
import { ArrowLeft, MoreHorizontal, Trash2 } from 'lucide-react';
import { Button } from '@/modules/shared/ui/components/button';
import {
  AlertDialog,
  AlertDialogContent,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogCancel,
  AlertDialogAction,
} from '@/modules/shared/ui/components/alert-dialog';
import {
  DropdownMenu,
  DropdownMenuTrigger,
  DropdownMenuContent,
  DropdownMenuItem,
} from '@/modules/shared/ui/components/dropdown-menu';
import { cn } from '@/modules/shared/lib/utils';

export type SaveStatus = 'idle' | 'saving' | 'saved' | 'dirty';

interface BuilderTopBarProps {
  workflowName: string;
  saveStatus: SaveStatus;
  isEditMode: boolean;
  isPublished: boolean;
  canPublish: boolean;
  isSaving: boolean;
  onNameChange: (name: string) => void;
  onBack: () => void;
  onSave: () => void;
  onPublish: () => void;
  onDelete?: () => void;
}

export function BuilderTopBar({
  workflowName,
  saveStatus,
  isEditMode,
  isPublished,
  canPublish,
  isSaving,
  onNameChange,
  onBack,
  onSave,
  onPublish,
  onDelete,
}: BuilderTopBarProps) {
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);

  const saveLabel = isEditMode ? 'Save changes' : 'Save draft';

  const statusText =
    saveStatus === 'saving'
      ? 'Saving...'
      : saveStatus === 'saved'
        ? 'Saved'
        : saveStatus === 'dirty'
          ? 'Unsaved changes'
          : '';

  return (
    <header className="flex h-12 shrink-0 items-center gap-3 border-b border-border bg-background px-4">
      {/* Back navigation */}
      <button
        onClick={onBack}
        className="flex items-center gap-1.5 rounded-md px-1.5 py-1 text-sm text-muted-foreground hover:bg-muted hover:text-foreground"
      >
        <ArrowLeft className="size-4" />
        Workflows
      </button>

      <div className="h-5 w-px bg-border" />

      {/* Inline workflow name */}
      <input
        type="text"
        value={workflowName}
        onChange={(e) => onNameChange(e.target.value)}
        placeholder="Untitled workflow"
        className="min-w-0 flex-1 bg-transparent text-sm font-semibold text-foreground placeholder:text-muted-foreground focus:outline-none"
        aria-label="Workflow name"
      />

      {/* Auto-save / save status indicator */}
      {statusText && (
        <span
          className={cn(
            'shrink-0 text-xs',
            saveStatus === 'saving' && 'text-muted-foreground',
            saveStatus === 'saved' && 'text-emerald-600',
            saveStatus === 'dirty' && 'text-amber-600'
          )}
        >
          {statusText}
        </span>
      )}

      {/* Save button */}
      <Button
        variant="outline"
        size="sm"
        onClick={onSave}
        disabled={isSaving}
      >
        {isSaving ? 'Saving...' : saveLabel}
      </Button>

      {/* Publish button — available once the workflow has been saved at least once */}
      {canPublish && !isPublished && (
        <Button size="sm" onClick={onPublish} disabled={isSaving}>
          Publish
        </Button>
      )}

      {/* Overflow menu — edit mode only (Admin) */}
      {isEditMode && onDelete && (
        <DropdownMenu>
          <DropdownMenuTrigger
            className="rounded-md p-1.5 text-muted-foreground hover:bg-muted hover:text-foreground"
            aria-label="More options"
          >
            <MoreHorizontal className="size-4" />
          </DropdownMenuTrigger>
          <DropdownMenuContent>
            <DropdownMenuItem
              destructive
              onClick={() => setDeleteDialogOpen(true)}
            >
              <Trash2 className="size-4" />
              Delete workflow
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      )}

      {/* Delete confirmation dialog — rendered outside the overflow menu
          so it can be opened from the menu item click without z-index issues */}
      {isEditMode && onDelete && (
        <AlertDialog
          open={deleteDialogOpen}
          onOpenChange={(open) => setDeleteDialogOpen(open)}
        >
          <AlertDialogContent>
            <AlertDialogHeader>
              <AlertDialogTitle>Delete this workflow?</AlertDialogTitle>
              <AlertDialogDescription>
                This permanently deletes the workflow template and all its task
                definitions. This cannot be undone.
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel onClick={() => setDeleteDialogOpen(false)}>
                Cancel
              </AlertDialogCancel>
              <AlertDialogAction
                onClick={() => {
                  setDeleteDialogOpen(false);
                  onDelete();
                }}
              >
                Delete
              </AlertDialogAction>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialog>
      )}
    </header>
  );
}
