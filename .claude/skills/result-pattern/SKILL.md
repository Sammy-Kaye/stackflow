---
name: result-pattern
description: >
  Enforce the StackFlow Result pattern for handler return types and error handling.
  Auto-load when writing or reviewing any command handler, query handler, or
  controller action. Use when Result<T>, Result.Fail, or error handling is involved.
  Never throw business exceptions — always return Result.
allowed-tools: Read, Write, Edit
---

<!--
  WHY THIS SKILL EXISTS
  ──────────────────────
  Thrown exceptions create invisible control flow. A developer reading a
  handler cannot see what failure states are possible without tracing every
  catch block up the call stack. This makes the code harder to understand,
  harder to test, and harder to hand off.

  The Result pattern makes every success and failure path explicit and
  visible in the method signature. You can read a handler and know exactly
  what can go wrong — without reading anything else.

  This is the pattern that makes StackFlow handlers human-readable.
  Every handler tells the full story of what it does and what can go wrong,
  right in the code itself.

  IF CLAUDE CODE CEASES TO EXIST:
  A developer following this skill can write every handler correctly by
  reading the patterns and rules below. No tribal knowledge required.
-->

# StackFlow — Result Pattern

---

## The Core Rule

```
Handlers NEVER throw business exceptions.
Handlers ALWAYS return Result<T> or Result.
```

Every handler return type is either:
- `Result<T>` — for operations that return data on success
- `Result` — for operations that return nothing on success (fire-and-forget style commands)

---

## Result\<T\> Structure

```csharp
// In StackFlow.Application/Common/Result.cs

public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T? Value { get; private set; }
    public string? Error { get; private set; }

    // Static factory methods — always use these, never use constructor directly
    public static Result<T> Success(T value) => new() { IsSuccess = true, Value = value };
    public static Result<T> Fail(string error) => new() { IsSuccess = false, Error = error };
}

public class Result
{
    public bool IsSuccess { get; private set; }
    public string? Error { get; private set; }

    public static Result Success() => new() { IsSuccess = true };
    public static Result Fail(string error) => new() { IsSuccess = false, Error = error };
}
```

---

## Handler Patterns — CORRECT vs WRONG

### Returning data (queries and create commands)

```csharp
// ✅ CORRECT — explicit success and failure paths visible in the code
public async Task<Result<WorkflowDto>> Handle(
    GetWorkflowByIdQuery query, CancellationToken ct)
{
    var workflow = await _repo.GetByIdAsync(query.Id, ct);

    if (workflow is null)
        return Result<WorkflowDto>.Fail("Workflow not found");

    return Result<WorkflowDto>.Success(workflow.ToDto());
}

// ❌ WRONG — throws hide the failure path from callers and tests
public async Task<WorkflowDto> Handle(
    GetWorkflowByIdQuery query, CancellationToken ct)
{
    var workflow = await _repo.GetByIdAsync(query.Id, ct);
    if (workflow is null)
        throw new NotFoundException("Workflow not found");  // NEVER
    return workflow.ToDto();
}
```

### Write operations (commands that don't return data)

```csharp
// ✅ CORRECT
public async Task<Result> Handle(
    CancelWorkflowCommand command, CancellationToken ct)
{
    var workflow = await _repo.GetByIdAsync(command.WorkflowStateId, ct);

    if (workflow is null)
        return Result.Fail("Workflow not found");

    if (workflow.Status == WorkflowStatus.Completed)
        return Result.Fail("Cannot cancel a completed workflow");

    workflow.Status = WorkflowStatus.Cancelled;
    workflow.CancelledAt = DateTime.UtcNow;

    await _uow.SaveChangesAsync(ct);
    return Result.Success();
}
```

### Multiple failure conditions

```csharp
// ✅ CORRECT — each failure is named and explicit
public async Task<Result<WorkflowTaskStateDto>> Handle(
    CompleteTaskCommand command, CancellationToken ct)
{
    var taskState = await _repo.GetByIdAsync(command.TaskStateId, ct);

    if (taskState is null)
        return Result<WorkflowTaskStateDto>.Fail("Task not found");

    if (taskState.Status == TaskStatus.Completed)
        return Result<WorkflowTaskStateDto>.Fail("Task is already completed");

    if (taskState.Status == TaskStatus.Declined)
        return Result<WorkflowTaskStateDto>.Fail("Cannot complete a declined task");

    if (taskState.AssignedToUserId != _currentUser.UserId)
        return Result<WorkflowTaskStateDto>.Fail("You are not assigned to this task");

    // All guards passed — proceed with the mutation
    taskState.Status = TaskStatus.Completed;
    taskState.CompletionNotes = command.Notes;

    await _uow.SaveChangesAsync(ct);
    return Result<WorkflowTaskStateDto>.Success(taskState.ToDto());
}
```

---

## Controller Pattern — HandleResult()

Controllers never inspect Result directly. `BaseApiController.HandleResult()` maps
the Result to the correct HTTP response automatically.

```csharp
// ✅ CORRECT — one line per endpoint, zero business logic
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateWorkflowCommand command)
    => HandleResult(await Mediator.Send(command));

[HttpDelete("{id:guid}")]
public async Task<IActionResult> Delete(Guid id)
    => HandleResult(await Mediator.Send(new DeleteWorkflowCommand(id)));

// HandleResult maps:
//   Result.Success with value  → 200 OK with body
//   Result.Success (void)      → 204 No Content
//   Result.Fail                → 400 Bad Request with { "error": message }
//   Result.Fail "not found"    → 404 Not Found (if error contains "not found")
```

---

## Infrastructure Exceptions — Different Rule

Business logic failures use `Result.Fail`. But infrastructure failures (database
errors, network failures) should NOT be caught in handlers — let them bubble up to
the global error handling middleware.

```csharp
// ✅ CORRECT — infrastructure exception propagates to middleware
public async Task<Result<WorkflowDto>> Handle(
    CreateWorkflowCommand command, CancellationToken ct)
{
    // If the database is down, EF throws — that's fine, middleware handles it
    var workflow = new Workflow { ... };
    await _repo.AddAsync(workflow, ct);
    await _uow.SaveChangesAsync(ct);  // May throw DbException — let it
    return Result<WorkflowDto>.Success(workflow.ToDto());
}

// ❌ WRONG — catching and swallowing infrastructure exceptions
public async Task<Result<WorkflowDto>> Handle(
    CreateWorkflowCommand command, CancellationToken ct)
{
    try
    {
        await _repo.AddAsync(workflow, ct);
        await _uow.SaveChangesAsync(ct);
        return Result<WorkflowDto>.Success(workflow.ToDto());
    }
    catch (Exception ex)
    {
        return Result<WorkflowDto>.Fail(ex.Message);  // NEVER — hides real errors
    }
}
```

**The rule:**
- Business rule failures (not found, invalid state, permission denied) → `Result.Fail`
- Infrastructure failures (database down, network timeout) → let them throw

---

## Testing Result Pattern

```csharp
// Test success path
var result = await _sut.Handle(command, CancellationToken.None);
Assert.True(result.IsSuccess);
Assert.Equal("Expected Name", result.Value.Name);

// Test failure path — check IsSuccess and optionally the error message
var result = await _sut.Handle(invalidCommand, CancellationToken.None);
Assert.False(result.IsSuccess);
Assert.Contains("not found", result.Error, StringComparison.OrdinalIgnoreCase);

// Test SaveChangesAsync NOT called on failure
_uowMock.Verify(
    u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
    Times.Never);
```

---

## Quick Reference — What Returns What

| Scenario | Return type | Call |
|---|---|---|
| Query returning data | `Result<T>` | `Result<WorkflowDto>.Success(dto)` |
| Command creating entity | `Result<T>` | `Result<WorkflowDto>.Success(dto)` |
| Command with no return value | `Result` | `Result.Success()` |
| Entity not found | `Result<T>` or `Result` | `Result<T>.Fail("X not found")` |
| Invalid state | `Result<T>` or `Result` | `Result<T>.Fail("Cannot X because Y")` |
| Permission denied | `Result<T>` or `Result` | `Result<T>.Fail("You are not authorised to X")` |
| Infrastructure failure | — | Let it throw — middleware handles it |

---

## What You Must Never Do

- Throw `NotFoundException`, `ValidationException`, or any custom business exception from a handler
- Return `null` instead of `Result.Fail` when an entity is not found
- Catch `DbException` or `Exception` in a handler and convert it to `Result.Fail`
- Put `try/catch` blocks in handlers for anything other than very specific, expected exceptions
- Return `Result.Success` without the value when the caller expects data
