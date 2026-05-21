using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Segmentation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddYieldAndScaleKpi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "YieldAndScaleScore",
                table: "SegmentationSimulationFarmers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "YieldAndScaleScore",
                table: "FarmerSegmentations",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "SegmentationConfigurationYieldAndScales",
                columns: table => new
                {
                    SegmentationConfigurationCultureTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MaxScore = table.Column<int>(type: "INTEGER", nullable: false),
                    Relevance = table.Column<decimal>(type: "TEXT", precision: 9, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentationConfigurationYieldAndScales", x => x.SegmentationConfigurationCultureTypeId);
                    table.ForeignKey(
                        name: "FK_SegmentationConfigurationYieldAndScales_SegmentationConfigurationCultureTypes_SegmentationConfigurationCultureTypeId",
                        column: x => x.SegmentationConfigurationCultureTypeId,
                        principalTable: "SegmentationConfigurationCultureTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YieldAndScaleRanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SegmentationConfigurationCultureTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    YieldAndScaleCropSeasonAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    MinimumYield = table.Column<int>(type: "INTEGER", nullable: false),
                    MaximumYield = table.Column<int>(type: "INTEGER", nullable: false),
                    MinimumModule = table.Column<int>(type: "INTEGER", nullable: false),
                    MaximumModule = table.Column<int>(type: "INTEGER", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YieldAndScaleRanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YieldAndScaleRanges_SegmentationConfigurationYieldAndScales_SegmentationConfigurationCultureTypeId",
                        column: x => x.SegmentationConfigurationCultureTypeId,
                        principalTable: "SegmentationConfigurationYieldAndScales",
                        principalColumn: "SegmentationConfigurationCultureTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_YieldAndScaleRanges_SegmentationConfigurationCultureTypeId",
                table: "YieldAndScaleRanges",
                column: "SegmentationConfigurationCultureTypeId");

            migrationBuilder.Sql(
                """
                INSERT INTO "SegmentationConfigurationYieldAndScales" ("SegmentationConfigurationCultureTypeId", "MaxScore", "Relevance")
                SELECT ct."Id", 0, 0
                FROM "SegmentationConfigurationCultureTypes" ct
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM "SegmentationConfigurationYieldAndScales" ys
                    WHERE ys."SegmentationConfigurationCultureTypeId" = ct."Id"
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "YieldAndScaleRanges");

            migrationBuilder.DropTable(
                name: "SegmentationConfigurationYieldAndScales");

            migrationBuilder.DropColumn(
                name: "YieldAndScaleScore",
                table: "SegmentationSimulationFarmers");

            migrationBuilder.DropColumn(
                name: "YieldAndScaleScore",
                table: "FarmerSegmentations");
        }
    }
}
