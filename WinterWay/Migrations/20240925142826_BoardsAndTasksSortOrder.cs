using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WinterWay.Migrations
{
    /// <inheritdoc />
    public partial class BoardsAndTasksSortOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "Tasks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "Boards",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "Boards");
        }
    }
}
