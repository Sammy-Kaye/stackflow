import { useState } from 'react';
import { Lightbulb } from 'lucide-react';

const HINT_KEY = 'stackflow:builder-hint-dismissed';

export function FirstUseHint() {
  const [visible, setVisible] = useState(() => {
    return localStorage.getItem(HINT_KEY) !== 'true';
  });

  if (!visible) return null;

  const dismiss = () => {
    localStorage.setItem(HINT_KEY, 'true');
    setVisible(false);
  };

  return (
    <div className="absolute left-4 top-4 z-10 flex max-w-sm items-start gap-2 rounded-lg border border-amber-300 bg-amber-50 px-3 py-2.5 shadow-sm">
      <Lightbulb className="mt-0.5 size-4 shrink-0 text-amber-600" />
      <p className="text-xs text-amber-800">
        New to the builder? Start with a Task node — drag it onto the canvas and connect it to Start.
      </p>
      <button
        onClick={dismiss}
        className="ml-auto shrink-0 rounded px-1.5 py-0.5 text-xs font-medium text-amber-700 hover:bg-amber-100"
      >
        Got it
      </button>
    </div>
  );
}
