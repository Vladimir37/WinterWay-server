using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WinterWay.Migrations
{
    /// <inheritdoc />
    public partial class AddDiary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DiaryGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Multiple = table.Column<bool>(type: "boolean", nullable: false),
                    CanBeEmpty = table.Column<bool>(type: "boolean", nullable: false),
                    Archived = table.Column<bool>(type: "boolean", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiaryGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiaryGroups_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiaryRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Info = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiaryRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiaryRecords_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiaryActivities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Icon = table.Column<string>(type: "text", nullable: false),
                    Color = table.Column<string>(type: "text", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    Archived = table.Column<bool>(type: "boolean", nullable: false),
                    DiaryGroupId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiaryActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiaryActivities_DiaryGroups_DiaryGroupId",
                        column: x => x.DiaryGroupId,
                        principalTable: "DiaryGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiaryRecordGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DiaryRecordId = table.Column<int>(type: "integer", nullable: false),
                    DiaryGroupId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiaryRecordGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiaryRecordGroups_DiaryGroups_DiaryGroupId",
                        column: x => x.DiaryGroupId,
                        principalTable: "DiaryGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiaryRecordGroups_DiaryRecords_DiaryRecordId",
                        column: x => x.DiaryRecordId,
                        principalTable: "DiaryRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiaryRecordActivities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DiaryGroupId = table.Column<int>(type: "integer", nullable: false),
                    DiaryActivityId = table.Column<int>(type: "integer", nullable: false),
                    DiaryActivityModelId = table.Column<int>(type: "integer", nullable: true),
                    DiaryRecordGroupModelId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiaryRecordActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiaryRecordActivities_DiaryActivities_DiaryActivityModelId",
                        column: x => x.DiaryActivityModelId,
                        principalTable: "DiaryActivities",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DiaryRecordActivities_DiaryGroups_DiaryGroupId",
                        column: x => x.DiaryGroupId,
                        principalTable: "DiaryGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiaryRecordActivities_DiaryRecordActivities_DiaryActivityId",
                        column: x => x.DiaryActivityId,
                        principalTable: "DiaryRecordActivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiaryRecordActivities_DiaryRecordGroups_DiaryRecordGroupMod~",
                        column: x => x.DiaryRecordGroupModelId,
                        principalTable: "DiaryRecordGroups",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiaryActivities_DiaryGroupId",
                table: "DiaryActivities",
                column: "DiaryGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_DiaryGroups_UserId",
                table: "DiaryGroups",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DiaryRecordActivities_DiaryActivityId",
                table: "DiaryRecordActivities",
                column: "DiaryActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_DiaryRecordActivities_DiaryActivityModelId",
                table: "DiaryRecordActivities",
                column: "DiaryActivityModelId");

            migrationBuilder.CreateIndex(
                name: "IX_DiaryRecordActivities_DiaryGroupId",
                table: "DiaryRecordActivities",
                column: "DiaryGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_DiaryRecordActivities_DiaryRecordGroupModelId",
                table: "DiaryRecordActivities",
                column: "DiaryRecordGroupModelId");

            migrationBuilder.CreateIndex(
                name: "IX_DiaryRecordGroups_DiaryGroupId",
                table: "DiaryRecordGroups",
                column: "DiaryGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_DiaryRecordGroups_DiaryRecordId",
                table: "DiaryRecordGroups",
                column: "DiaryRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_DiaryRecords_UserId",
                table: "DiaryRecords",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiaryRecordActivities");

            migrationBuilder.DropTable(
                name: "DiaryActivities");

            migrationBuilder.DropTable(
                name: "DiaryRecordGroups");

            migrationBuilder.DropTable(
                name: "DiaryGroups");

            migrationBuilder.DropTable(
                name: "DiaryRecords");
        }
    }
}
