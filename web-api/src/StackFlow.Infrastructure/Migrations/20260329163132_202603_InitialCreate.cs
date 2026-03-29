using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StackFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class _202603_InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Workspaces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workspaces", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Workflows",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Category = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workflows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workflows_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ContextType = table.Column<int>(type: "integer", nullable: false),
                    BatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferenceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowStates_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowStates_Workspaces_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    AssigneeType = table.Column<int>(type: "integer", nullable: false),
                    DefaultAssignedToEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    DueAtOffsetDays = table.Column<int>(type: "integer", nullable: false),
                    NodeType = table.Column<int>(type: "integer", nullable: false),
                    ConditionConfig = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    ParentTaskId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowTasks_WorkflowTasks_ParentTaskId",
                        column: x => x.ParentTaskId,
                        principalTable: "WorkflowTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkflowTasks_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowAudits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowStateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActorEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Action = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OldValue = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    NewValue = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowAudits_WorkflowStates_WorkflowStateId",
                        column: x => x.WorkflowStateId,
                        principalTable: "WorkflowStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTaskStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowStateId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowTaskId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AssignedToEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletionToken = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TokenExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsTokenUsed = table.Column<bool>(type: "boolean", nullable: false),
                    CompletionNotes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DeclineReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTaskStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowTaskStates_WorkflowStates_WorkflowStateId",
                        column: x => x.WorkflowStateId,
                        principalTable: "WorkflowStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkflowTaskStates_WorkflowTasks_WorkflowTaskId",
                        column: x => x.WorkflowTaskId,
                        principalTable: "WorkflowTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowTaskAudits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkflowTaskStateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActorEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Action = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OldValue = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    NewValue = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowTaskAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkflowTaskAudits_WorkflowTaskStates_WorkflowTaskStateId",
                        column: x => x.WorkflowTaskStateId,
                        principalTable: "WorkflowTaskStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Workspaces",
                columns: new[] { "Id", "CreatedAt", "Name" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Demo Workspace" },
                    { new Guid("00000000-0000-0000-0000-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Global" }
                });

            migrationBuilder.InsertData(
                table: "Workflows",
                columns: new[] { "Id", "Category", "CreatedAt", "Description", "IsActive", "Name", "UpdatedAt", "WorkspaceId" },
                values: new object[,]
                {
                    { new Guid("10000000-0000-0000-0000-000000000001"), "HR", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Standard onboarding process for new employees.", true, "Employee Onboarding", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000002") },
                    { new Guid("10000000-0000-0000-0000-000000000002"), "Finance", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Approval workflow for purchase requests.", true, "Purchase Approval", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000002") },
                    { new Guid("10000000-0000-0000-0000-000000000003"), "Operations", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Structured process for offboarding departing clients.", true, "Client Offboarding", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000002") }
                });

            migrationBuilder.InsertData(
                table: "WorkflowTasks",
                columns: new[] { "Id", "AssigneeType", "ConditionConfig", "DefaultAssignedToEmail", "Description", "DueAtOffsetDays", "NodeType", "OrderIndex", "ParentTaskId", "Title", "WorkflowId" },
                values: new object[,]
                {
                    { new Guid("20000000-0000-0000-0000-000000000001"), 0, null, null, "Prepare and send the signed offer letter to the new employee.", 1, 0, 0, null, "Send offer letter", new Guid("10000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0000-000000000002"), 0, null, null, "Provision laptop, accounts, and access credentials for the new starter.", 3, 0, 1, null, "Set up workstation", new Guid("10000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0000-000000000003"), 0, null, null, "Book the orientation session and send calendar invites.", 3, 0, 2, null, "Schedule orientation session", new Guid("10000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0000-000000000004"), 0, null, null, "Ensure all employment forms, tax declarations, and NDAs are signed.", 5, 0, 3, null, "Complete HR paperwork", new Guid("10000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0000-000000000005"), 0, null, null, "Pair the new employee with an experienced team member.", 5, 0, 4, null, "Assign buddy / mentor", new Guid("10000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0000-000000000006"), 0, null, null, "Schedule and conduct a 30-day review meeting with the new starter.", 30, 0, 5, null, "30-day check-in", new Guid("10000000-0000-0000-0000-000000000001") },
                    { new Guid("20000000-0000-0000-0000-000000000007"), 0, null, null, "Complete the purchase request form with vendor details, cost, and business justification.", 1, 0, 0, null, "Submit purchase request", new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("20000000-0000-0000-0000-000000000008"), 0, null, null, "Line manager reviews and approves or declines the purchase request.", 3, 1, 1, null, "Manager approval", new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("20000000-0000-0000-0000-000000000009"), 0, null, null, "Finance team raises and issues the purchase order to the vendor.", 5, 0, 2, null, "Raise purchase order", new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("20000000-0000-0000-0000-000000000010"), 0, null, null, "Verify goods or services received and mark the purchase order as complete.", 14, 0, 3, null, "Confirm delivery and close", new Guid("10000000-0000-0000-0000-000000000002") },
                    { new Guid("20000000-0000-0000-0000-000000000011"), 0, null, null, "Notify relevant internal teams of the client's departure date.", 1, 0, 0, null, "Send offboarding notification", new Guid("10000000-0000-0000-0000-000000000003") },
                    { new Guid("20000000-0000-0000-0000-000000000012"), 0, null, null, "Collect any company equipment, access credentials, or materials from the client.", 3, 0, 1, null, "Retrieve client assets", new Guid("10000000-0000-0000-0000-000000000003") },
                    { new Guid("20000000-0000-0000-0000-000000000013"), 0, null, null, "Remove client access from all internal systems and shared resources.", 3, 0, 2, null, "Revoke system access", new Guid("10000000-0000-0000-0000-000000000003") },
                    { new Guid("20000000-0000-0000-0000-000000000014"), 0, null, null, "Generate and send the final invoice for any outstanding services.", 5, 0, 3, null, "Issue final invoice", new Guid("10000000-0000-0000-0000-000000000003") },
                    { new Guid("20000000-0000-0000-0000-000000000015"), 0, null, null, "Schedule and conduct an exit interview to capture feedback.", 7, 0, 4, null, "Conduct exit interview", new Guid("10000000-0000-0000-0000-000000000003") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_WorkspaceId",
                table: "Users",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowAudits_WorkflowStateId",
                table: "WorkflowAudits",
                column: "WorkflowStateId");

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_WorkspaceId",
                table: "Workflows",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStates_WorkflowId",
                table: "WorkflowStates",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStates_WorkspaceId",
                table: "WorkflowStates",
                column: "WorkspaceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTaskAudits_WorkflowTaskStateId",
                table: "WorkflowTaskAudits",
                column: "WorkflowTaskStateId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_ParentTaskId",
                table: "WorkflowTasks",
                column: "ParentTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTasks_WorkflowId",
                table: "WorkflowTasks",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTaskStates_WorkflowStateId",
                table: "WorkflowTaskStates",
                column: "WorkflowStateId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowTaskStates_WorkflowTaskId",
                table: "WorkflowTaskStates",
                column: "WorkflowTaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "WorkflowAudits");

            migrationBuilder.DropTable(
                name: "WorkflowTaskAudits");

            migrationBuilder.DropTable(
                name: "WorkflowTaskStates");

            migrationBuilder.DropTable(
                name: "WorkflowStates");

            migrationBuilder.DropTable(
                name: "WorkflowTasks");

            migrationBuilder.DropTable(
                name: "Workflows");

            migrationBuilder.DropTable(
                name: "Workspaces");
        }
    }
}
