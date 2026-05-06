// workflow-service.ts
// Service layer for the workflows module — the only file that knows the workflow
// API exists.
//
// WHY a service layer: If an endpoint path, base URL, or request shape changes,
// this is the only file that needs updating. No hook or component imports
// apiClient directly — they call this service, which calls apiClient.
//
// getAll()           GET  /api/workflows         → WorkflowListDto
// getById(id)        GET  /api/workflows/{id}    → WorkflowDto
// create(dto)        POST /api/workflows          → WorkflowDto (201)
// update(id, dto)    PUT  /api/workflows/{id}    → WorkflowDto (200)
// remove(id)         DELETE /api/workflows/{id}  → 204

import { apiClient } from '@/modules/shared/infrastructure/api-client';
import type {
  CreateWorkflowDto,
  UpdateWorkflowDto,
  WorkflowDto,
  WorkflowListDto,
} from '../dtos/workflow-dtos';

export const workflowService = {
  getAll: () =>
    apiClient.get<WorkflowListDto>('/api/workflows'),

  getById: (id: string) =>
    apiClient.get<WorkflowDto>(`/api/workflows/${id}`),

  create: (dto: CreateWorkflowDto) =>
    apiClient.post<WorkflowDto>('/api/workflows', dto),

  update: (id: string, dto: UpdateWorkflowDto) =>
    apiClient.put<WorkflowDto>(`/api/workflows/${id}`, dto),

  remove: (id: string) =>
    apiClient.delete(`/api/workflows/${id}`),
};
