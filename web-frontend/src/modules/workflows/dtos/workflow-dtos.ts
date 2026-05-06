// workflow-dtos.ts
// API contract types for the workflow module.
//
// These shapes mirror the API contract exactly as defined in the Feature 8 brief.
// If the backend changes its response shape, update these types — not the components.
//
// WorkflowSummaryDto   — one item in GET /api/workflows list response
// WorkflowDto          — full response from GET/POST/PUT /api/workflows/{id}
// WorkflowTaskDto      — one task in a WorkflowDto tasks array
// CreateWorkflowDto    — request body for POST /api/workflows
// UpdateWorkflowDto    — request body for PUT /api/workflows/{id}
// CreateWorkflowTaskDto — task shape inside create/update request bodies
// WorkflowListDto      — top-level response envelope from GET /api/workflows

import type { AssigneeType } from '../enums/assignee-type';
import type { NodeType } from '../enums/node-type';

export interface WorkflowSummaryDto {
  id: string;                  // UUID string
  name: string;
  description: string | null;
  category: string | null;
  isActive: boolean;
  taskCount: number;
  createdAt: string;           // ISO 8601
  updatedAt: string;           // ISO 8601
  isGlobal: boolean;
}

export interface WorkflowTaskDto {
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

export interface WorkflowDto {
  id: string;                  // UUID string
  name: string;
  description: string | null;
  category: string | null;
  workspaceId: string;         // UUID string
  isActive: boolean;
  createdAt: string;           // ISO 8601
  updatedAt: string;           // ISO 8601
  tasks: WorkflowTaskDto[];
}

export interface CreateWorkflowTaskDto {
  title: string;
  description?: string | null;
  assigneeType: AssigneeType;
  defaultAssignedToEmail?: string | null;
  orderIndex: number;
  dueAtOffsetDays?: number | null;
  nodeType: NodeType;
  conditionConfig?: string | null;
  parentTaskId?: string | null;
}

export interface CreateWorkflowDto {
  name: string;
  description?: string | null;
  category?: string | null;
  tasks: CreateWorkflowTaskDto[];
}

export interface UpdateWorkflowDto {
  name: string;
  description?: string | null;
  category?: string | null;
  isActive: boolean;
  tasks: CreateWorkflowTaskDto[];
}

// Envelope returned by GET /api/workflows
export interface WorkflowListDto {
  items: WorkflowSummaryDto[];
  totalCount: number;
}
