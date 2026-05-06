// dropdown-menu.tsx
// General-purpose dropdown menu built on @base-ui/react/menu.
//
// Follows the same wrapping pattern as alert-dialog.tsx and tooltip.tsx —
// thin named wrappers over the Base UI primitives with project styling applied.
//
// Anatomy:
//   DropdownMenu         — root state manager (open/close)
//   DropdownMenuTrigger  — element that opens the menu
//   DropdownMenuContent  — the floating panel itself (includes portal + positioner)
//   DropdownMenuItem     — a single action row inside the panel

import { Menu as MenuPrimitive } from '@base-ui/react/menu';
import { cn } from '@/modules/shared/lib/utils';

function DropdownMenu({ ...props }: MenuPrimitive.Root.Props) {
  return <MenuPrimitive.Root data-slot="dropdown-menu" {...props} />;
}

function DropdownMenuTrigger({ ...props }: MenuPrimitive.Trigger.Props) {
  return <MenuPrimitive.Trigger data-slot="dropdown-menu-trigger" {...props} />;
}

function DropdownMenuContent({
  className,
  side = 'bottom',
  align = 'end',
  sideOffset = 4,
  ...props
}: MenuPrimitive.Popup.Props &
  Pick<MenuPrimitive.Positioner.Props, 'side' | 'align' | 'sideOffset'>) {
  return (
    <MenuPrimitive.Portal>
      <MenuPrimitive.Positioner
        side={side}
        align={align}
        sideOffset={sideOffset}
        className="isolate z-50"
      >
        <MenuPrimitive.Popup
          data-slot="dropdown-menu-content"
          className={cn(
            'min-w-[160px] rounded-lg border border-border bg-background py-1 shadow-lg',
            'data-open:animate-in data-open:fade-in-0 data-open:zoom-in-95',
            'data-closed:animate-out data-closed:fade-out-0 data-closed:zoom-out-95',
            className
          )}
          {...props}
        />
      </MenuPrimitive.Positioner>
    </MenuPrimitive.Portal>
  );
}

function DropdownMenuItem({
  className,
  destructive = false,
  ...props
}: MenuPrimitive.Item.Props & { destructive?: boolean }) {
  return (
    <MenuPrimitive.Item
      data-slot="dropdown-menu-item"
      className={cn(
        'flex w-full cursor-default select-none items-center gap-2 px-3 py-2 text-sm outline-none transition-colors',
        destructive
          ? 'text-destructive hover:bg-destructive/5 focus:bg-destructive/5'
          : 'text-foreground hover:bg-muted focus:bg-muted',
        className
      )}
      {...props}
    />
  );
}

export { DropdownMenu, DropdownMenuTrigger, DropdownMenuContent, DropdownMenuItem };
