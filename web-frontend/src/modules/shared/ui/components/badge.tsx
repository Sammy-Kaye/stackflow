// badge.tsx
// Inline status label — used for IsActive, IsGlobal, workflow status,
// task priority, and any other short categorical label across the UI.
//
// Variants:
//   default     — primary teal; InProgress, Medium
//   secondary   — muted gray; Pending, Low
//   success     — green; Completed
//   warning     — amber; Expired, High priority, Deadline nodes
//   destructive — red; Cancelled, Declined, Critical
//   outline     — subtle bordered; Skipped

import { cva, type VariantProps } from 'class-variance-authority';
import { cn } from '@/modules/shared/lib/utils';

const badgeVariants = cva(
  'inline-flex items-center gap-1 rounded-full border px-2 py-0.5 text-xs font-medium transition-colors',
  {
    variants: {
      variant: {
        default:
          'border-transparent bg-primary text-primary-foreground',
        secondary:
          'border-transparent bg-secondary text-secondary-foreground',
        success:
          'border-transparent bg-emerald-500/15 text-emerald-700 dark:bg-emerald-500/20 dark:text-emerald-400',
        warning:
          'border-transparent bg-amber-500/15 text-amber-700 dark:bg-amber-500/20 dark:text-amber-400',
        destructive:
          'border-transparent bg-destructive/10 text-destructive',
        outline:
          'border-border bg-transparent text-foreground',
      },
    },
    defaultVariants: {
      variant: 'default',
    },
  }
);

export interface BadgeProps
  extends React.HTMLAttributes<HTMLSpanElement>,
    VariantProps<typeof badgeVariants> {}

function Badge({ className, variant, ...props }: BadgeProps) {
  return (
    <span
      data-slot="badge"
      className={cn(badgeVariants({ variant }), className)}
      {...props}
    />
  );
}

export { Badge, badgeVariants };
