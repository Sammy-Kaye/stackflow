// skeleton.tsx
// Loading placeholder — shown while async data is in flight.
//
// A pulsing block that takes the size of the space it occupies. Pair it with a
// container div that matches the expected rendered layout so there is no layout
// shift when real content arrives.
//
// Usage:
//   <Skeleton className="h-4 w-32" />         ← single line of text
//   <Skeleton className="h-32 w-full" />       ← card placeholder

import { cn } from '@/modules/shared/lib/utils';

function Skeleton({ className, ...props }: React.HTMLAttributes<HTMLDivElement>) {
  return (
    <div
      data-slot="skeleton"
      className={cn('animate-pulse rounded-md bg-muted', className)}
      {...props}
    />
  );
}

export { Skeleton };
