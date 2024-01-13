using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApprovalDemo.Migrations
{
    /// <inheritdoc />
    public partial class AddIsHotToApprovalItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHot",
                table: "AppApprovalItems",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHot",
                table: "AppApprovalItems");
        }
    }
}
