using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WinterWay.Migrations
{
    /// <inheritdoc />
    public partial class NullableBacklogSprint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Sprints_BacklogSprintId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<int>(
                name: "BacklogSprintId",
                table: "AspNetUsers",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Sprints_BacklogSprintId",
                table: "AspNetUsers",
                column: "BacklogSprintId",
                principalTable: "Sprints",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Sprints_BacklogSprintId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<int>(
                name: "BacklogSprintId",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Sprints_BacklogSprintId",
                table: "AspNetUsers",
                column: "BacklogSprintId",
                principalTable: "Sprints",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
