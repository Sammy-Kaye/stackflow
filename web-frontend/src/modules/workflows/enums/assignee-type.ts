// assignee-type.ts
// String constant object for WorkflowTask assignee types.
//
// Uses a const object + type union rather than a TypeScript enum because
// this project's tsconfig has erasableSyntaxOnly: true, which prohibits enums.
//
// Values MUST match the C# AssigneeType enum names exactly so they round-trip
// correctly through JSON serialisation. If the backend enum changes, update
// this file in the same PR.

export const AssigneeType = {
  Internal: 'Internal',
  External: 'External',
} as const;

export type AssigneeType = (typeof AssigneeType)[keyof typeof AssigneeType];
