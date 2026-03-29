---
name: ef-migration
description: >
  Guide EF Core migration creation and validation for StackFlow. Loaded once by
  backend-agent at Step 3 (Infrastructure layer) when a migration is needed.
  Do not auto-load — load explicitly just before running dotnet ef migrations add.
allowed-tools: Read, Write, Edit, Bash
---

<!--
  WHY THIS SKILL EXISTS
  ──────────────────────
  Migrations are the most dangerous routine operation in StackFlow.
  A poorly written migration can:
    - Drop a column with live data in it
    - Add a NOT NULL column with no default to a table with existing rows
    - Create an index with the wrong name that conflicts later
    - Leave the Down() method incorrect, making rollback impossible

  EF Core generates migrations from your entity configuration — but it does
  exactly what you told it, including mistakes. The SQL it generates is only
  as good as the Fluent API configuration it reads.

  This skill enforces the review gate: you always read the migration before
  you apply it. No exceptions.

  IF CLAUDE CODE CEASES TO EXIST:
  A developer can follow this skill to create, review, and apply every
  migration correctly. The commands, review checklist, and common pitfalls
  are all here — no external knowledge required.
-->

# StackFlow — EF Core Migrations

---

## The Core Rule

```
Never apply a migration without reading the generated Up() SQL.
Never accept a Down() method without verifying it correctly reverses Up().
One wrong migration on a live database can cause data loss.
```

---

## Fluent API Configuration — Always Before Migrating

All entity configuration lives in `StackFlow.Infrastructure/Persistence/Configurations/`.
One file per entity. No data annotations on domain entities — ever.

```csharp
// StackFlow.Infrastructure/Persistence/Configurations/WorkflowConfiguration.cs
public class WorkflowConfiguration : IEntityTypeConfiguration<Workflow>
{
    public void Configure(EntityTypeBuilder<Workflow> builder)
    {
        // Primary key
        builder.HasKey(w => w.Id);

        // Required fields with max lengths
        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.Description)
            .HasMaxLength(1000);

        // Default values for timestamps
        builder.Property(w => w.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.Property(w => w.UpdatedAt)
            .HasDefaultValueSql("NOW()");

        // Relationships — always explicit, never convention-only
        builder.HasMany(w => w.Tasks)
            .WithOne(t => t.Workflow)
            .HasForeignKey(t => t.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        // Table name — always explicit
        builder.ToTable("Workflows");
    }
}
```

**Configuration rules:**
- `HasKey` — always explicit, never rely on convention for composite keys
- `IsRequired()` — maps to NOT NULL in PostgreSQL
- `HasMaxLength()` — maps to `varchar(n)`, prevents unbounded strings
- `OnDelete` — always specify. `Cascade` for child records, `Restrict` for lookup references
- `ToTable()` — always explicit to prevent naming surprises

---

## Creating a Migration

Run from the `web-api/` directory:

```bash
# Standard migration command
dotnet ef migrations add {MigrationName} \
  --project src/StackFlow.Infrastructure \
  --startup-project src/StackFlow.Api

# Migration naming convention: {YYYYMMDDHHmm}_{PascalCaseDescription}
# Examples:
#   202503281430_AddWorkflowEntities
#   202503290900_AddPriorityToWorkflowTaskState
#   202504011200_AddExternalTokenFields
```

**After running the command — before doing anything else — open the generated migration file.**

---

## Migration Review Checklist

Every migration must pass all of these before `dotnet ef database update` is run.

### Up() method review

```
□ Does it create the tables/columns I expected and nothing else?
□ Are all NOT NULL columns either nullable, have a default, or are on a new table?
  (Adding NOT NULL with no default to an existing table with data will fail)
□ Are foreign key columns indexed? EF does not always add indexes automatically.
□ Are there any accidental DROP TABLE or DROP COLUMN operations?
  (A renamed entity can look like a drop + create to EF — check carefully)
□ Does the migration name match the convention: YYYYMMDDHHmm_PascalCaseDescription?
□ Are string columns using varchar with a length, not text or nvarchar(max)?
```

### Down() method review

```
□ Does Down() correctly reverse every operation in Up()?
□ If Up() creates a table, does Down() drop it?
□ If Up() adds a column, does Down() drop that column?
□ If Up() adds a foreign key, does Down() remove it?
□ Is the order reversed? (Down() undoes operations in reverse order of Up())
```

### PostgreSQL-specific checks

```
□ UUIDs use uuid type (EF maps Guid to uuid automatically with Npgsql)
□ Timestamps use timestamp with time zone (timestamptz) — not timestamp
□ Boolean uses bool — not int/tinyint
□ Enums stored as int or as varchar? (int is simpler; varchar is human-readable in DB)
```

---

## Reading the Generated SQL

After creating the migration, generate the SQL script to review it directly:

```bash
# Generate SQL for the pending migration (dry run — does NOT apply)
dotnet ef migrations script \
  --project src/StackFlow.Infrastructure \
  --startup-project src/StackFlow.Api \
  --output migration-preview.sql

# Review the file
cat migration-preview.sql
```

This is the exact SQL that will run against your database. Read every line.

---

## Applying the Migration

Only after the review is complete:

```bash
# Apply pending migrations to the database
dotnet ef database update \
  --project src/StackFlow.Infrastructure \
  --startup-project src/StackFlow.Api

# Verify the migration was applied
dotnet ef migrations list \
  --project src/StackFlow.Infrastructure \
  --startup-project src/StackFlow.Api
# The new migration should show (Applied) in the list
```

---

## Common Pitfalls and Fixes

### Adding NOT NULL column to existing table

```csharp
// ❌ PROBLEM: This generates a NOT NULL column with no default.
// If the table has existing rows, the migration will fail.
builder.Property(w => w.Priority)
    .IsRequired();

// ✅ FIX OPTION 1: Add a default value
builder.Property(w => w.Priority)
    .IsRequired()
    .HasDefaultValue(Priority.Medium);

// ✅ FIX OPTION 2: Make it nullable first, backfill, then make it required
// (Do this in two separate migrations for production safety)
builder.Property(w => w.Priority)
    .IsRequired(false);  // Migration 1: add nullable
// ... Migration 2: after data backfill, make it required
```

### EF treating a rename as drop + create

```csharp
// If you rename an entity class (e.g. WorkflowTask → WorkflowStep),
// EF will generate: DROP TABLE WorkflowTasks + CREATE TABLE WorkflowSteps
// This is data loss. Fix it in the migration manually:

// In the generated migration Up():
// ❌ What EF generates:
migrationBuilder.DropTable("WorkflowTasks");
migrationBuilder.CreateTable("WorkflowSteps", ...);

// ✅ What you should replace it with:
migrationBuilder.RenameTable("WorkflowTasks", newName: "WorkflowSteps");
```

### Missing index on foreign key

```csharp
// EF does not always add indexes for FK columns automatically.
// Always check. Add manually in the migration if missing:
migrationBuilder.CreateIndex(
    name: "IX_WorkflowTaskStates_WorkflowStateId",
    table: "WorkflowTaskStates",
    column: "WorkflowStateId");

// Or add in Fluent API configuration:
builder.HasIndex(t => t.WorkflowStateId)
    .HasDatabaseName("IX_WorkflowTaskStates_WorkflowStateId");
```

### Migration conflict (two developers, diverged branches)

```bash
# Check the current state
dotnet ef migrations list \
  --project src/StackFlow.Infrastructure \
  --startup-project src/StackFlow.Api

# If there are two migrations with the same parent:
# 1. Roll back to the common base
# 2. Remove the conflicting migration
# 3. Merge the entity changes
# 4. Create a single new migration covering both sets of changes
dotnet ef migrations remove \
  --project src/StackFlow.Infrastructure \
  --startup-project src/StackFlow.Api
```

---

## Rolling Back a Migration

```bash
# Roll back to a specific migration (by name)
dotnet ef database update {PreviousMigrationName} \
  --project src/StackFlow.Infrastructure \
  --startup-project src/StackFlow.Api

# Roll back all migrations (empty database)
dotnet ef database update 0 \
  --project src/StackFlow.Infrastructure \
  --startup-project src/StackFlow.Api
```

**This is why Down() must be correct.** If a bad migration reaches production, rollback
is your recovery path. A broken `Down()` means you are stuck.

---

## Migration Completion Summary

After every migration, produce this note in the backend completion summary:

```
### Migration created
Name: {YYYYMMDDHHmm}_{PascalCaseDescription}
Up() changes:
  - Created table: {TableName} with columns: {list}
  - Added column: {ColumnName} ({type}, {nullable/required}) to {TableName}
  - Added index: {IndexName} on {TableName}.{Column}
Down() verified: Yes — reverses all Up() operations correctly
SQL reviewed: Yes — no unexpected drops, correct types confirmed
```

---

## What You Must Never Do

- Run `dotnet ef database update` without first reading the generated migration file
- Accept a migration with a broken or empty `Down()` method
- Add a NOT NULL column to an existing table without a default or a two-step migration plan
- Leave foreign key columns without indexes on tables that will have significant rows
- Rename entities without manually fixing the drop+create to a rename in the migration
- Create a migration with a vague name like `Update1` or `FixStuff` — always use the date convention
- Edit migration files after they have been applied to any environment
