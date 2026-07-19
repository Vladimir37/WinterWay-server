using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WinterWay.Migrations
{
    /// <inheritdoc />
    public partial class TaskDistribution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "PlannedDate",
                table: "Tasks",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlannedScale",
                table: "Tasks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DistributionModes",
                table: "Boards",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlannedDate",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "PlannedScale",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "DistributionModes",
                table: "Boards");
        }
    }
}
