using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WinterWay.Migrations
{
    /// <inheritdoc />
    public partial class DiaryActivityFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiaryRecordActivities_DiaryActivities_DiaryActivityModelId",
                table: "DiaryRecordActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_DiaryRecordActivities_DiaryGroups_DiaryGroupId",
                table: "DiaryRecordActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_DiaryRecordActivities_DiaryRecordActivities_DiaryActivityId",
                table: "DiaryRecordActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_DiaryRecordActivities_DiaryRecordGroups_DiaryRecordGroupMod~",
                table: "DiaryRecordActivities");

            migrationBuilder.DropIndex(
                name: "IX_DiaryRecordActivities_DiaryActivityModelId",
                table: "DiaryRecordActivities");

            migrationBuilder.DropIndex(
                name: "IX_DiaryRecordActivities_DiaryRecordGroupModelId",
                table: "DiaryRecordActivities");

            migrationBuilder.DropColumn(
                name: "DiaryActivityModelId",
                table: "DiaryRecordActivities");

            migrationBuilder.DropColumn(
                name: "DiaryRecordGroupModelId",
                table: "DiaryRecordActivities");

            migrationBuilder.RenameColumn(
                name: "DiaryGroupId",
                table: "DiaryRecordActivities",
                newName: "DiaryRecordGroupId");

            migrationBuilder.RenameIndex(
                name: "IX_DiaryRecordActivities_DiaryGroupId",
                table: "DiaryRecordActivities",
                newName: "IX_DiaryRecordActivities_DiaryRecordGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_DiaryRecordActivities_DiaryActivities_DiaryActivityId",
                table: "DiaryRecordActivities",
                column: "DiaryActivityId",
                principalTable: "DiaryActivities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DiaryRecordActivities_DiaryRecordGroups_DiaryRecordGroupId",
                table: "DiaryRecordActivities",
                column: "DiaryRecordGroupId",
                principalTable: "DiaryRecordGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DiaryRecordActivities_DiaryActivities_DiaryActivityId",
                table: "DiaryRecordActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_DiaryRecordActivities_DiaryRecordGroups_DiaryRecordGroupId",
                table: "DiaryRecordActivities");

            migrationBuilder.RenameColumn(
                name: "DiaryRecordGroupId",
                table: "DiaryRecordActivities",
                newName: "DiaryGroupId");

            migrationBuilder.RenameIndex(
                name: "IX_DiaryRecordActivities_DiaryRecordGroupId",
                table: "DiaryRecordActivities",
                newName: "IX_DiaryRecordActivities_DiaryGroupId");

            migrationBuilder.AddColumn<int>(
                name: "DiaryActivityModelId",
                table: "DiaryRecordActivities",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiaryRecordGroupModelId",
                table: "DiaryRecordActivities",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiaryRecordActivities_DiaryActivityModelId",
                table: "DiaryRecordActivities",
                column: "DiaryActivityModelId");

            migrationBuilder.CreateIndex(
                name: "IX_DiaryRecordActivities_DiaryRecordGroupModelId",
                table: "DiaryRecordActivities",
                column: "DiaryRecordGroupModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_DiaryRecordActivities_DiaryActivities_DiaryActivityModelId",
                table: "DiaryRecordActivities",
                column: "DiaryActivityModelId",
                principalTable: "DiaryActivities",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DiaryRecordActivities_DiaryGroups_DiaryGroupId",
                table: "DiaryRecordActivities",
                column: "DiaryGroupId",
                principalTable: "DiaryGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DiaryRecordActivities_DiaryRecordActivities_DiaryActivityId",
                table: "DiaryRecordActivities",
                column: "DiaryActivityId",
                principalTable: "DiaryRecordActivities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DiaryRecordActivities_DiaryRecordGroups_DiaryRecordGroupMod~",
                table: "DiaryRecordActivities",
                column: "DiaryRecordGroupModelId",
                principalTable: "DiaryRecordGroups",
                principalColumn: "Id");
        }
    }
}
