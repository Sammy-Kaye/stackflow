// node-type.ts
// String constant object for WorkflowTask node types.
//
// Uses a const object + type union rather than a TypeScript enum because
// this project's tsconfig has erasableSyntaxOnly: true, which prohibits enums.
//
// Values MUST match the C# NodeType enum names exactly so they round-trip
// correctly through JSON serialisation. If the backend enum changes, update
// this file in the same PR.

export const NodeType = {
  Task: 'Task',
  Approval: 'Approval',
  Condition: 'Condition',
  Notification: 'Notification',
  ExternalStep: 'ExternalStep',
  Deadline: 'Deadline',
} as const;

export type NodeType = (typeof NodeType)[keyof typeof NodeType];
