using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WinterWay.Migrations
{
    /// <inheritdoc />
    public partial class CalendarRecordsByType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CalendarRecords_CalendarValues_FixedValueId",
                table: "CalendarRecords");

            migrationBuilder.DropIndex(
                name: "IX_CalendarRecords_FixedValueId",
                table: "CalendarRecords");

            migrationBuilder.DropColumn(
                name: "SerializedDefaultValue",
                table: "Calendars");

            migrationBuilder.DropColumn(
                name: "FixedValueId",
                table: "CalendarRecords");

            migrationBuilder.DropColumn(
                name: "SerializedValue",
                table: "CalendarRecords");

            migrationBuilder.AddColumn<int>(
                name: "DefaultRecordId",
                table: "Calendars",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "Date",
                table: "CalendarRecords",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "CalendarRecords",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "CalendarRecordBooleans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Value = table.Column<bool>(type: "boolean", nullable: false),
                    CalendarRecordId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarRecordBooleans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalendarRecordBooleans_CalendarRecords_CalendarRecordId",
                        column: x => x.CalendarRecordId,
                        principalTable: "CalendarRecords",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CalendarRecordFixeds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FixedValueId = table.Column<int>(type: "integer", nullable: false),
                    CalendarRecordId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarRecordFixeds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalendarRecordFixeds_CalendarRecords_CalendarRecordId",
                        column: x => x.CalendarRecordId,
                        principalTable: "CalendarRecords",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CalendarRecordFixeds_CalendarValues_FixedValueId",
                        column: x => x.FixedValueId,
                        principalTable: "CalendarValues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CalendarRecordNumerics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Value = table.Column<int>(type: "integer", nullable: false),
                    CalendarRecordId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarRecordNumerics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalendarRecordNumerics_CalendarRecords_CalendarRecordId",
                        column: x => x.CalendarRecordId,
                        principalTable: "CalendarRecords",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CalendarRecordTimes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Value = table.Column<TimeSpan>(type: "interval", nullable: false),
                    CalendarRecordId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarRecordTimes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalendarRecordTimes_CalendarRecords_CalendarRecordId",
                        column: x => x.CalendarRecordId,
                        principalTable: "CalendarRecords",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Calendars_DefaultRecordId",
                table: "Calendars",
                column: "DefaultRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarRecordBooleans_CalendarRecordId",
                table: "CalendarRecordBooleans",
                column: "CalendarRecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CalendarRecordFixeds_CalendarRecordId",
                table: "CalendarRecordFixeds",
                column: "CalendarRecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CalendarRecordFixeds_FixedValueId",
                table: "CalendarRecordFixeds",
                column: "FixedValueId");

            migrationBuilder.CreateIndex(
                name: "IX_CalendarRecordNumerics_CalendarRecordId",
                table: "CalendarRecordNumerics",
                column: "CalendarRecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CalendarRecordTimes_CalendarRecordId",
                table: "CalendarRecordTimes",
                column: "CalendarRecordId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Calendars_CalendarRecords_DefaultRecordId",
                table: "Calendars",
                column: "DefaultRecordId",
                principalTable: "CalendarRecords",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Calendars_CalendarRecords_DefaultRecordId",
                table: "Calendars");

            migrationBuilder.DropTable(
                name: "CalendarRecordBooleans");

            migrationBuilder.DropTable(
                name: "CalendarRecordFixeds");

            migrationBuilder.DropTable(
                name: "CalendarRecordNumerics");

            migrationBuilder.DropTable(
                name: "CalendarRecordTimes");

            migrationBuilder.DropIndex(
                name: "IX_Calendars_DefaultRecordId",
                table: "Calendars");

            migrationBuilder.DropColumn(
                name: "DefaultRecordId",
                table: "Calendars");

            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "CalendarRecords");

            migrationBuilder.AddColumn<string>(
                name: "SerializedDefaultValue",
                table: "Calendars",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "Date",
                table: "CalendarRecords",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1),
                oldClrType: typeof(DateOnly),
                oldType: "date",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FixedValueId",
                table: "CalendarRecords",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SerializedValue",
                table: "CalendarRecords",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CalendarRecords_FixedValueId",
                table: "CalendarRecords",
                column: "FixedValueId");

            migrationBuilder.AddForeignKey(
                name: "FK_CalendarRecords_CalendarValues_FixedValueId",
                table: "CalendarRecords",
                column: "FixedValueId",
                principalTable: "CalendarValues",
                principalColumn: "Id");
        }
    }
}
