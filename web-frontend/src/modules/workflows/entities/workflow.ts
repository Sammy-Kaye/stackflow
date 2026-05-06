// workflow.ts
// Domain entity interfaces for the workflow module.
//
// These mirror the backend domain model and represent the shape of data as it
// exists in the system — not as it travels over the wire (that is the DTO layer).
//
// Workflow      — a reusable workflow template blueprint.
// WorkflowTask  — one step node in a workflow template.

import type { AssigneeType } from '../enums/assignee-type';
import type { NodeType } from '../enums/node-type';

export interface WorkflowTask {
  id: string;                          // UUID string
  workflowId: string;                  // UUID string
  title: string;
  description: string | null;
  assigneeType: AssigneeType;
  defaultAssignedToEmail: string | null;
  orderIndex: number;
  dueAtOffsetDays: number | null;
  nodeType: NodeType;
  conditionConfig: string | null;
  parentTaskId: string | null;         // UUID string | null
}

export interface Workflow {
  id: string;                          // UUID string
  name: string;
  description: string | null;
  category: string | null;
  workspaceId: string;                 // UUID string
  isActive: boolean;
  createdAt: string;                   // ISO 8601
  updatedAt: string;                   // ISO 8601
  tasks: WorkflowTask[];
}
