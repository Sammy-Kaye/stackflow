import { useCallback, useEffect, useRef, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import ReactFlow, {
  Background,
  Controls,
  MiniMap,
  addEdge,
  useEdgesState,
  useNodesState,
  type Connection,
  type Edge,
  type Node,
  type NodeTypes,
  type OnConnect,
  type ReactFlowInstance,
} from 'reactflow';
import 'reactflow/dist/style.css';
import { toast } from 'sonner';

import { useWorkflow, useCreateWorkflow, useUpdateWorkflow, useDeleteWorkflow } from '../../hooks/use-workflows';
import type { WorkflowTaskDto } from '../../dtos/workflow-dtos';
import { AssigneeType } from '../../enums/assignee-type';
import { NodeType } from '../../enums/node-type';
import type { CreateWorkflowTaskDto } from '../../dtos/workflow-dtos';

import { TaskNode } from '../components/nodes/TaskNode';
import { ApprovalNode } from '../components/nodes/ApprovalNode';
import { ConditionNode } from '../components/nodes/ConditionNode';
import { NotificationNode } from '../components/nodes/NotificationNode';
import { StartNode } from '../components/nodes/StartNode';
import { EndNode } from '../components/nodes/EndNode';
import { BuilderTopBar, type SaveStatus } from '../components/builder/BuilderTopBar';
import { NodePalette } from '../components/builder/NodePalette';
import { PropertiesPanel } from '../components/builder/PropertiesPanel';
import { FirstUseHint } from '../components/builder/FirstUseHint';

// nodeTypes must be defined outside the component to prevent React Flow
// from re-registering them on every render, which causes node remounting.
const nodeTypes: NodeTypes = {
  task: TaskNode,
  approval: ApprovalNode,
  condition: ConditionNode,
  notification: NotificationNode,
  start: StartNode,
  end: EndNode,
};

const DEFAULT_EDGE_OPTIONS = {
  type: 'smoothstep' as const,
  animated: true,
  deletable: true,
};

// Map from React Flow node type string to the NodeType enum value sent to the API.
const NODE_TYPE_MAP: Record<string, NodeType> = {
  task: NodeType.Task,
  approval: NodeType.Approval,
  condition: NodeType.Condition,
  notification: NodeType.Notification,
};

function makeStartNode(): Node {
  return {
    id: 'start',
    type: 'start',
    position: { x: 360, y: 40 },
    data: {},
    draggable: false,
    deletable: false,
    selectable: false,
  };
}

function makeEndNode(taskCount: number): Node {
  return {
    id: 'end',
    type: 'end',
    position: { x: 360, y: 40 + (taskCount + 1) * 160 },
    data: {},
    draggable: false,
    deletable: false,
    selectable: false,
  };
}

function tasksToNodes(tasks: WorkflowTaskDto[]): Node[] {
  const sorted = [...tasks].sort((a, b) => a.orderIndex - b.orderIndex);
  return sorted.map((task, idx) => ({
    id: task.id,
    type: task.nodeType.toLowerCase(),
    position: { x: 320, y: 40 + (idx + 1) * 160 },
    data: {
      title: task.title,
      description: task.description,
      assigneeType: task.assigneeType,
      assigneeEmail: task.defaultAssignedToEmail,
      dueAtOffsetDays: task.dueAtOffsetDays,
      conditionConfig: task.conditionConfig,
      parentTaskId: task.parentTaskId,
    },
  }));
}

function tasksToEdges(tasks: WorkflowTaskDto[]): Edge[] {
  const sorted = [...tasks].sort((a, b) => a.orderIndex - b.orderIndex);
  const edges: Edge[] = [];

  if (sorted.length === 0) {
    edges.push({ id: 'e-start-end', source: 'start', target: 'end', ...DEFAULT_EDGE_OPTIONS });
    return edges;
  }

  edges.push({ id: `e-start-${sorted[0].id}`, source: 'start', target: sorted[0].id, ...DEFAULT_EDGE_OPTIONS });

  for (let i = 0; i < sorted.length - 1; i++) {
    edges.push({
      id: `e-${sorted[i].id}-${sorted[i + 1].id}`,
      source: sorted[i].id,
      target: sorted[i + 1].id,
      ...DEFAULT_EDGE_OPTIONS,
    });
  }

  edges.push({
    id: `e-${sorted[sorted.length - 1].id}-end`,
    source: sorted[sorted.length - 1].id,
    target: 'end',
    ...DEFAULT_EDGE_OPTIONS,
  });

  return edges;
}

export function WorkflowBuilderPage() {
  const { id } = useParams<{ id?: string }>();
  const navigate = useNavigate();
  const isEditMode = !!id;

  // After first POST in create mode, store the returned ID so subsequent saves use PUT.
  // Both a ref (for access inside callbacks without stale closure) and state (to trigger
  // a re-render so the Publish button appears after first save).
  const createdIdRef = useRef<string | null>(null);
  const [createdId, setCreatedId] = useState<string | null>(null);

  const [workflowName, setWorkflowName] = useState('Untitled workflow');
  const [isActive, setIsActive] = useState(false);
  const [saveStatus, setSaveStatus] = useState<SaveStatus>('idle');
  const [selectedNodeId, setSelectedNodeId] = useState<string | null>(null);

  const [nodes, setNodes, onNodesChange] = useNodesState([makeStartNode(), makeEndNode(0)]);
  const [edges, setEdges, onEdgesChange] = useEdgesState([
    { id: 'e-start-end', source: 'start', target: 'end', ...DEFAULT_EDGE_OPTIONS },
  ]);

  const createWorkflow = useCreateWorkflow();
  const updateWorkflow = useUpdateWorkflow();
  const deleteWorkflow = useDeleteWorkflow();

  // Edit mode: load existing workflow
  const { data: workflow, isLoading, isError } = useWorkflow(id);

  useEffect(() => {
    if (!workflow) return;

    setWorkflowName(workflow.name);
    setIsActive(workflow.isActive);

    const taskNodes = tasksToNodes(workflow.tasks);
    const taskEdges = tasksToEdges(workflow.tasks);
    const endNode = makeEndNode(workflow.tasks.length);

    setNodes([makeStartNode(), ...taskNodes, endNode]);
    setEdges(taskEdges);

    // Signal that the initial data load is complete — subsequent node/edge changes are user edits.
    pendingLoadRef.current = false;
  }, [workflow, setNodes, setEdges]);

  // beforeunload guard — fires the browser native confirm when there are unsaved changes
  useEffect(() => {
    const isDirty = saveStatus === 'dirty';
    if (!isDirty) return;

    const handler = (e: BeforeUnloadEvent) => {
      e.preventDefault();
    };

    window.addEventListener('beforeunload', handler);
    return () => window.removeEventListener('beforeunload', handler);
  }, [saveStatus]);

  // Mark dirty whenever nodes or edges change — but only after the initial canvas
  // population is complete. In create mode that is after first render. In edit mode
  // that is after the workflow data arrives and populates the canvas.
  const initializedRef = useRef(false);
  const pendingLoadRef = useRef(isEditMode); // true in edit mode until data arrives

  useEffect(() => {
    // The workflow data load effect runs before this one due to declaration order.
    // pendingLoadRef is cleared there; after that, any change here is a user edit.
    if (pendingLoadRef.current) return;

    if (!initializedRef.current) {
      initializedRef.current = true;
      return;
    }

    setSaveStatus('dirty');
  }, [nodes, edges]);

  // Auto-save — debounced 30 seconds after any canvas change
  const autoSaveTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  useEffect(() => {
    if (saveStatus !== 'dirty') return;

    if (autoSaveTimerRef.current) clearTimeout(autoSaveTimerRef.current);
    autoSaveTimerRef.current = setTimeout(() => {
      void performSave(false);
    }, 30_000);

    return () => {
      if (autoSaveTimerRef.current) clearTimeout(autoSaveTimerRef.current);
    };
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [saveStatus]);

  const serializeNodes = useCallback((): CreateWorkflowTaskDto[] => {
    const taskNodes = nodes.filter((n) => n.type !== 'start' && n.type !== 'end');
    const sorted = [...taskNodes].sort((a, b) => a.position.y - b.position.y);

    return sorted.map((node, idx) => {
      const rawAssigneeType = (node.data.assigneeType as string) ?? AssigneeType.Internal;
      const assigneeType: AssigneeType =
        rawAssigneeType === AssigneeType.External ? AssigneeType.External : AssigneeType.Internal;

      return {
        title: (node.data.title as string) || 'Untitled',
        description: (node.data.description as string | null) ?? null,
        assigneeType,
        defaultAssignedToEmail: (node.data.assigneeEmail as string | null) ?? null,
        orderIndex: idx,
        dueAtOffsetDays: (node.data.dueAtOffsetDays as number | null) ?? null,
        nodeType: NODE_TYPE_MAP[node.type ?? 'task'] ?? NodeType.Task,
        conditionConfig: (node.data.conditionConfig as string | null) ?? null,
        parentTaskId: (node.data.parentTaskId as string | null) ?? null,
      };
    });
  }, [nodes]);

  const performSave = useCallback(
    async (publish: boolean) => {
      const tasks = serializeNodes();
      const name = workflowName.trim() || 'Untitled workflow';
      const resolvedId = id ?? createdIdRef.current;

      setSaveStatus('saving');

      try {
        if (resolvedId) {
          await updateWorkflow.mutateAsync({
            id: resolvedId,
            dto: {
              name,
              description: null,
              category: null,
              isActive: publish ? true : isActive,
              tasks,
            },
          });
        } else {
          const result = await createWorkflow.mutateAsync({
            name,
            description: null,
            category: null,
            tasks,
          });
          createdIdRef.current = result.id;
          setCreatedId(result.id);
        }

        setSaveStatus('saved');
        if (publish) {
          navigate('/active');
        }
      } catch {
        setSaveStatus('dirty');
        toast.error('Failed to save. Please try again.');
      }
    },
    [serializeNodes, workflowName, id, isActive, updateWorkflow, createWorkflow, navigate]
  );

  const handleSave = () => void performSave(false);

  const handlePublish = async () => {
    await performSave(true);
  };

  const handleDelete = () => {
    const resolvedId = id ?? createdIdRef.current;
    if (!resolvedId) return;
    deleteWorkflow.mutate(resolvedId, {
      onSuccess: () => navigate('/workflows'),
    });
  };

  const handleBack = () => {
    if (saveStatus === 'dirty') {
      const confirmed = window.confirm(
        'You have unsaved changes. Leave anyway?'
      );
      if (!confirmed) return;
    }
    navigate('/workflows');
  };

  const handleNameChange = (name: string) => {
    setWorkflowName(name);
    setSaveStatus('dirty');
  };

  // React Flow drop handler — creates a new node at the drop position
  const reactFlowWrapper = useRef<HTMLDivElement>(null);
  const [reactFlowInstance, setReactFlowInstance] = useState<ReactFlowInstance | null>(null);

  const onDragOver = useCallback((event: React.DragEvent) => {
    event.preventDefault();
    event.dataTransfer.dropEffect = 'move';
  }, []);

  const onDrop = useCallback(
    (event: React.DragEvent) => {
      event.preventDefault();

      const nodeType = event.dataTransfer.getData('application/reactflow');
      if (!nodeType || !reactFlowInstance) return;

      const position = reactFlowInstance.screenToFlowPosition({
        x: event.clientX,
        y: event.clientY,
      });

      const newNode: Node = {
        id: `${nodeType}-${Date.now()}`,
        type: nodeType,
        position,
        data: {
          title: '',
          description: null,
          assigneeType: AssigneeType.Internal,
          assigneeEmail: null,
          dueAtOffsetDays: null,
          conditionConfig: null,
          parentTaskId: null,
          yesLabel: 'Yes',
          noLabel: 'No',
        },
      };

      setNodes((nds) => nds.concat(newNode));
    },
    [reactFlowInstance, setNodes]
  );

  const onConnect: OnConnect = useCallback(
    (connection: Connection) => {
      setEdges((eds) => addEdge({ ...connection, ...DEFAULT_EDGE_OPTIONS }, eds));
    },
    [setEdges]
  );

  const onNodesDelete = useCallback(
    (deleted: Node[]) => {
      // Guard: StartNode and EndNode cannot be deleted
      const forbidden = deleted.filter((n) => n.id === 'start' || n.id === 'end');
      if (forbidden.length > 0) {
        setNodes((nds) => [...nds, ...forbidden]);
      }
    },
    [setNodes]
  );

  const handleNodeDataChange = useCallback(
    (nodeId: string, field: string, value: unknown) => {
      setNodes((nds) =>
        nds.map((node) =>
          node.id === nodeId ? { ...node, data: { ...node.data, [field]: value } } : node
        )
      );
    },
    [setNodes]
  );

  const selectedNode = nodes.find((n) => n.id === selectedNodeId) ?? null;
  const canPublish = !!(id ?? createdId);
  const isSaving = createWorkflow.isPending || updateWorkflow.isPending;

  // Edit mode loading state
  if (isEditMode && isLoading) {
    return (
      <div className="flex h-full items-center justify-center">
        <p className="text-sm text-muted-foreground">Loading workflow...</p>
      </div>
    );
  }

  if (isEditMode && isError) {
    return (
      <div className="flex h-full items-center justify-center">
        <p className="text-sm text-destructive">
          Failed to load workflow. Please refresh.
        </p>
      </div>
    );
  }

  return (
    <div className="flex h-full flex-col overflow-hidden">
      <BuilderTopBar
        workflowName={workflowName}
        saveStatus={saveStatus}
        isEditMode={isEditMode}
        isPublished={isActive}
        canPublish={canPublish}
        isSaving={isSaving}
        onNameChange={handleNameChange}
        onBack={handleBack}
        onSave={handleSave}
        onPublish={handlePublish}
        onDelete={isEditMode ? handleDelete : undefined}
      />

      {/* Live workflow amber banner */}
      {isEditMode && isActive && (
        <div className="flex items-center gap-2 border-b border-amber-300 bg-amber-50 px-4 py-2 text-xs text-amber-800">
          <span className="font-medium">This workflow is currently live.</span>
          Changes will apply to new instances only — running instances are not affected.
        </div>
      )}

      <div className="flex flex-1 overflow-hidden">
        <NodePalette />

        {/* React Flow canvas */}
        <div className="relative flex-1 [&_.react-flow__pane]:cursor-default" ref={reactFlowWrapper}>
          <FirstUseHint />

          <ReactFlow
            nodes={nodes}
            edges={edges}
            nodeTypes={nodeTypes}
            onNodesChange={onNodesChange}
            onEdgesChange={onEdgesChange}
            onConnect={onConnect}
            onNodesDelete={onNodesDelete}
            onDrop={onDrop}
            onDragOver={onDragOver}
            onInit={(instance) => setReactFlowInstance(instance)}
            onNodeClick={(_event, node) => {
              if (node.id !== 'start' && node.id !== 'end') {
                setSelectedNodeId(node.id);
              }
            }}
            onPaneClick={() => setSelectedNodeId(null)}
            defaultEdgeOptions={DEFAULT_EDGE_OPTIONS}
            deleteKeyCode="Delete"
            fitView
            fitViewOptions={{ padding: 0.2 }}
            className="bg-muted/30"
          >
            <Controls />
            <MiniMap className="!bottom-4 !right-4" />
            <Background gap={16} color="hsl(var(--border))" />
          </ReactFlow>
        </div>

        <PropertiesPanel
          selectedNode={selectedNode}
          onClose={() => setSelectedNodeId(null)}
          onNodeDataChange={handleNodeDataChange}
        />
      </div>
    </div>
  );
}
