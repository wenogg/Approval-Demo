using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApprovalDemo.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowVersionTOApprovalItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WorkflowDefinitionVersion",
                table: "AppApprovalItems",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkflowDefinitionVersion",
                table: "AppApprovalItems");
        }
    }
}
