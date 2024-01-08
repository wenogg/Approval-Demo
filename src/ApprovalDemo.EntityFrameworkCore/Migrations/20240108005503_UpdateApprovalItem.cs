using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApprovalDemo.Migrations
{
    /// <inheritdoc />
    public partial class UpdateApprovalItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "AppApprovalItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "AppApprovalItems");
        }
    }
}
