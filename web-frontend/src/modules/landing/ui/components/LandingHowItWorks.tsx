// LandingHowItWorks.tsx
// Three-step "How It Works" section for the StackFlow landing page.
//
// Desktop: steps in a row with a horizontal connector line behind the number circles.
// Mobile: steps stacked vertically, connector line hidden.
// The connector is a 2px bg-border line drawn behind the step circles (z-0),
// with steps sitting on top (z-10).

const STEPS = [
  {
    number: '1',
    title: 'Build',
    description:
      'Design your workflow once in the drag-and-drop builder. Define tasks, assignees, and approval gates.',
  },
  {
    number: '2',
    title: 'Assign and launch',
    description:
      'Trigger a run for a client, project, or employee. StackFlow handles routing automatically.',
  },
  {
    number: '3',
    title: 'Track and complete',
    description:
      'Watch every step progress in real time. Intervene only when you choose to.',
  },
] as const;

export function LandingHowItWorks() {
  return (
    <section
      className="py-32 px-8"
      style={{ backgroundColor: 'var(--sf-surface)' }}
    >
      <div className="max-w-7xl mx-auto">
        {/* Section header */}
        <div className="text-center mb-20">
          <h2
            className="text-4xl font-bold tracking-tight mb-6"
            style={{ color: 'var(--sf-on-surface)', fontFamily: 'Manrope, sans-serif', letterSpacing: '-0.02em' }}
          >
            Up and running in minutes
          </h2>
          <p
            className="max-w-xl mx-auto"
            style={{ color: 'var(--sf-on-surface-variant)', fontFamily: 'Inter, sans-serif' }}
          >
            The path from manual chaos to automated efficiency is shorter than you think.
          </p>
        </div>

        {/* Steps */}
        <div className="relative flex flex-col md:flex-row items-center justify-between gap-12">
          {/* Horizontal connector line — desktop only */}
          <div
            className="hidden md:block absolute top-8 left-[10%] right-[10%] h-px z-0"
            style={{ backgroundColor: 'color-mix(in srgb, var(--sf-outline-variant) 20%, transparent)' }}
          />

          {STEPS.map(({ number, title, description }) => (
            <div
              key={number}
              className="relative z-10 flex flex-col items-center text-center space-y-5 max-w-xs"
            >
              {/* Number circle */}
              <div
                className="w-16 h-16 rounded-full flex items-center justify-center text-2xl font-bold shrink-0"
                style={{
                  backgroundColor: 'var(--sf-surface-container)',
                  border: '2px solid var(--sf-primary)',
                  color: 'var(--sf-primary)',
                  fontFamily: 'Manrope, sans-serif',
                }}
              >
                {number}
              </div>

              {/* Title */}
              <h4
                className="text-xl font-bold"
                style={{ color: 'var(--sf-on-surface)', fontFamily: 'Manrope, sans-serif' }}
              >
                {title}
              </h4>

              {/* Description */}
              <p
                className="text-sm leading-relaxed"
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
