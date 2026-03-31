// LandingFeatures.tsx
// Three-column feature card grid for the StackFlow landing page.
//
// Card background uses bg-surface-container tonal shift — no borders.
// Icons from lucide-react: Layers, Users, ClipboardList.
// Section id="features" matches the navbar anchor links.

import { Layers, Users, ClipboardList } from 'lucide-react';

const FEATURES = [
  {
    icon: Layers,
    title: 'Build once, run forever',
    description:
      'Workflow templates you define once and launch as many times as you need. The engine handles routing, notifications, and exceptions automatically.',
  },
  {
    icon: Users,
    title: 'Everyone knows their next step',
    description:
      'Tasks are assigned automatically. No chasing, no ambiguity — just clear ownership and a personalised view of every team member\'s priorities.',
  },
  {
    icon: ClipboardList,
    title: 'Every action, recorded',
    description:
      'A complete audit trail of every status change, approval, and completion. See exactly what happened, when, and by whom — always.',
  },
] as const;

export function LandingFeatures() {
  return (
    <section
      id="features"
      className="py-32 px-8"
      style={{ backgroundColor: 'var(--sf-surface-container-low)' }}
    >
      <div className="max-w-7xl mx-auto">
        {/* Section header */}
        <div className="flex flex-col mb-16">
          <span
            className="text-sm font-bold uppercase tracking-widest mb-4"
            style={{ color: 'var(--sf-primary)', fontFamily: 'Manrope, sans-serif' }}
          >
            Capabilities
          </span>
          <h2
            className="text-4xl font-bold tracking-tight"
            style={{ color: 'var(--sf-on-surface)', fontFamily: 'Manrope, sans-serif', letterSpacing: '-0.02em' }}
          >
            Precision-engineered workflows
          </h2>
        </div>

        {/* Cards */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          {FEATURES.map(({ icon: Icon, title, description }) => (
            <div
              key={title}
              className="p-10 rounded-xl space-y-6 transition-colors duration-300"
              style={{ backgroundColor: 'var(--sf-surface-container)' }}
              onMouseEnter={(e) => {
                (e.currentTarget as HTMLDivElement).style.backgroundColor = 'var(--sf-surface-container-high)';
              }}
              onMouseLeave={(e) => {
                (e.currentTarget as HTMLDivElement).style.backgroundColor = 'var(--sf-surface-container)';
              }}
            >
              {/* Icon container */}
              <div
                className="w-12 h-12 flex items-center justify-center rounded-lg"
                style={{
                  backgroundColor: 'color-mix(in srgb, var(--sf-primary) 10%, transparent)',
                  color: 'var(--sf-primary)',
                }}
              >
                <Icon className="size-5" />
              </div>

              {/* Title */}
              <h3
                className="text-xl font-bold"
                style={{ color: 'var(--sf-on-surface)', fontFamily: 'Manrope, sans-serif' }}
              >
                {title}
              </h3>

              {/* Description */}
              <p
                className="leading-relaxed"
                style={{ color: 'var(--sf-on-surface-variant)', fontFamily: 'Inter, sans-serif' }}
              >
                {description}
              </p>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
