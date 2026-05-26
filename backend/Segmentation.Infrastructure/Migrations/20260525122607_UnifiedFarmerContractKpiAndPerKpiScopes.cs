using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Segmentation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UnifiedFarmerContractKpiAndPerKpiScopes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FarmerContractKpis",
                columns: table => new
                {
                    FarmerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CropSeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    CultureTypeCode = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    DeliveredPercentage = table.Column<int>(type: "INTEGER", nullable: false),
                    DeliveredAmountKg = table.Column<int>(type: "INTEGER", nullable: false),
                    ContractedAmountKg = table.Column<int>(type: "INTEGER", nullable: false),
                    Iqs = table.Column<int>(type: "INTEGER", nullable: false),
                    HadNtrm = table.Column<bool>(type: "INTEGER", nullable: false),
                    HadQualityMixture = table.Column<bool>(type: "INTEGER", nullable: false),
                    SelfFundingPercentage = table.Column<int>(type: "INTEGER", nullable: false),
                    HaveDebt = table.Column<bool>(type: "INTEGER", nullable: false),
                    Yield = table.Column<int>(type: "INTEGER", nullable: false),
                    Scale = table.Column<int>(type: "INTEGER", nullable: false),
                    ReforestationPercentage = table.Column<int>(type: "INTEGER", nullable: false),
                    NativeForestPercentage = table.Column<int>(type: "INTEGER", nullable: false),
                    NonExclusive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FarmerContractKpis", x => new { x.FarmerId, x.CropSeasonId, x.CultureTypeCode });
                    table.ForeignKey(
                        name: "FK_FarmerContractKpis_CropSeasons_CropSeasonId",
                        column: x => x.CropSeasonId,
                        principalTable: "CropSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FarmerContractKpis_CultureTypes_CultureTypeCode",
                        column: x => x.CultureTypeCode,
                        principalTable: "CultureTypes",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FarmerContractKpis_Farmers_FarmerId",
                        column: x => x.FarmerId,
                        principalTable: "Farmers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                INSERT INTO "FarmerContractKpis" (
                    "FarmerId", "CropSeasonId", "CultureTypeCode",
                    "DeliveredPercentage", "DeliveredAmountKg", "ContractedAmountKg",
                    "Iqs", "HadNtrm", "HadQualityMixture",
                    "SelfFundingPercentage", "HaveDebt",
                    "Yield", "Scale",
                    "ReforestationPercentage", "NativeForestPercentage", "NonExclusive")
                SELECT
                    k."FarmerId", k."CropSeasonId", k."CultureTypeCode",
                    COALESCE(l."DeliveredPercentage", 0),
                    COALESCE(l."DeliveredAmountKg", 0),
                    COALESCE(l."ContractedAmountKg", COALESCE(ys."ContractedAmountKg", 0)),
                    COALESCE(q."Iqs", 0),
                    COALESCE(q."HadNtrm", 0),
                    COALESCE(q."HadQualityMixture", 0),
                    COALESCE(f."SelfFundingPercentage", 0),
                    COALESCE(f."HaveDebt", 0),
                    COALESCE(ys."Yield", 0),
                    COALESCE(ys."Scale", 0),
                    COALESCE(e."ReforestationPercentage", 0),
                    COALESCE(e."NativeForestPercentage", 0),
                    COALESCE(fr."NonExclusiveFarmer", 0)
                FROM (
                    SELECT "FarmerId", "CropSeasonId", "CultureTypeCode" FROM "LoyaltyKpis"
                    UNION SELECT "FarmerId", "CropSeasonId", "CultureTypeCode" FROM "QualityKpis"
                    UNION SELECT "FarmerId", "CropSeasonId", "CultureTypeCode" FROM "FinancialKpis"
                    UNION SELECT "FarmerId", "CropSeasonId", "CultureTypeCode" FROM "YieldAndScaleKpis"
                    UNION SELECT "FarmerId", "CropSeasonId", "CultureTypeCode" FROM "EsgKpis"
                ) k
                LEFT JOIN "LoyaltyKpis" l ON l."FarmerId" = k."FarmerId" AND l."CropSeasonId" = k."CropSeasonId" AND l."CultureTypeCode" = k."CultureTypeCode"
                LEFT JOIN "QualityKpis" q ON q."FarmerId" = k."FarmerId" AND q."CropSeasonId" = k."CropSeasonId" AND q."CultureTypeCode" = k."CultureTypeCode"
                LEFT JOIN "FinancialKpis" f ON f."FarmerId" = k."FarmerId" AND f."CropSeasonId" = k."CropSeasonId" AND f."CultureTypeCode" = k."CultureTypeCode"
                LEFT JOIN "YieldAndScaleKpis" ys ON ys."FarmerId" = k."FarmerId" AND ys."CropSeasonId" = k."CropSeasonId" AND ys."CultureTypeCode" = k."CultureTypeCode"
                LEFT JOIN "EsgKpis" e ON e."FarmerId" = k."FarmerId" AND e."CropSeasonId" = k."CropSeasonId" AND e."CultureTypeCode" = k."CultureTypeCode"
                LEFT JOIN "Farmers" fr ON fr."Id" = k."FarmerId";

                UPDATE "FarmerContractKpis"
                SET "ContractedAmountKg" = COALESCE(
                    (SELECT l."ContractedAmountKg" FROM "LoyaltyKpis" l
                     WHERE l."FarmerId" = "FarmerContractKpis"."FarmerId"
                       AND l."CropSeasonId" = "FarmerContractKpis"."CropSeasonId"
                       AND l."CultureTypeCode" = "FarmerContractKpis"."CultureTypeCode"
                       AND l."ContractedAmountKg" > 0),
                    "ContractedAmountKg");
                """);

            migrationBuilder.DropTable(name: "EsgKpis");
            migrationBuilder.DropTable(name: "FinancialKpis");
            migrationBuilder.DropTable(name: "LoyaltyKpis");
            migrationBuilder.DropTable(name: "QualityKpis");
            migrationBuilder.DropTable(name: "YieldAndScaleKpis");

            migrationBuilder.Sql("""
                UPDATE "SegmentationConfigurationCultureTypes"
                SET "MaximumScore" = "MaximumScore" - COALESCE(
                    (SELECT ys."MaxScore" FROM "SegmentationConfigurationYieldAndScales" ys
                     WHERE ys."SegmentationConfigurationCultureTypeId" = "SegmentationConfigurationCultureTypes"."Id"), 0);
                """);

            migrationBuilder.DropTable(name: "YieldAndScaleRanges");
            migrationBuilder.DropTable(name: "SegmentationConfigurationYieldAndScales");

            migrationBuilder.DropColumn(name: "YieldAndScaleScore", table: "SegmentationSimulationFarmers");
            migrationBuilder.DropColumn(name: "YieldAndScaleScore", table: "FarmerSegmentations");

            migrationBuilder.AddColumn<Guid>(
                name: "SegmentationConfigurationId",
                table: "FarmerSegmentations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "FarmerSegmentations"
                SET "SegmentationConfigurationId" = (
                    SELECT s."SegmentationConfigurationId"
                    FROM "SegmentationSimulations" s
                    WHERE s."CropSeasonId" = "FarmerSegmentations"."CropSeasonId" AND s."Status" = 'O'
                    ORDER BY s."SimulationDate" DESC
                    LIMIT 1);
                """);

            migrationBuilder.Sql("""
                UPDATE "FarmerSegmentations"
                SET "SegmentationConfigurationId" = (
                    SELECT c."Id" FROM "SegmentationConfigurations" c LIMIT 1)
                WHERE "SegmentationConfigurationId" IS NULL;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "SegmentationConfigurationId",
                table: "FarmerSegmentations",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "SegmentationSimulationKpiScopes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SegmentationSimulationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    KpiKind = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ValueAggregation = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentationSimulationKpiScopes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SegmentationSimulationKpiScopes_SegmentationSimulations_SegmentationSimulationId",
                        column: x => x.SegmentationSimulationId,
                        principalTable: "SegmentationSimulations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SegmentationSimulationKpiScopeSeasons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SegmentationSimulationKpiScopeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CropSeasonId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentationSimulationKpiScopeSeasons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SegmentationSimulationKpiScopeSeasons_CropSeasons_CropSeasonId",
                        column: x => x.CropSeasonId,
                        principalTable: "CropSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SegmentationSimulationKpiScopeSeasons_SegmentationSimulationKpiScopes_SegmentationSimulationKpiScopeId",
                        column: x => x.SegmentationSimulationKpiScopeId,
                        principalTable: "SegmentationSimulationKpiScopes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                INSERT INTO "SegmentationSimulationKpiScopes" ("Id", "SegmentationSimulationId", "KpiKind", "ValueAggregation")
                SELECT lower(hex(randomblob(16))), scs."SegmentationSimulationId", kinds."KpiKind", kinds."ValueAggregation"
                FROM "SegmentationSimulationCropSeasons" scs
                CROSS JOIN (
                    SELECT 'Loyalty' AS "KpiKind", NULL AS "ValueAggregation"
                    UNION ALL SELECT 'Quality', 'Average'
                    UNION ALL SELECT 'Financial', 'LastActiveCropData'
                    UNION ALL SELECT 'Esg', NULL
                    UNION ALL SELECT 'Technologies', NULL
                    UNION ALL SELECT 'Yield', 'Average'
                    UNION ALL SELECT 'Scale', 'LastActiveCropData'
                ) kinds
                GROUP BY scs."SegmentationSimulationId", kinds."KpiKind", kinds."ValueAggregation";

                INSERT INTO "SegmentationSimulationKpiScopeSeasons" ("Id", "SegmentationSimulationKpiScopeId", "CropSeasonId")
                SELECT lower(hex(randomblob(16))), scope."Id", scs."CropSeasonId"
                FROM "SegmentationSimulationKpiScopes" scope
                INNER JOIN "SegmentationSimulationCropSeasons" scs ON scs."SegmentationSimulationId" = scope."SegmentationSimulationId";
                """);

            migrationBuilder.DropTable(name: "SegmentationSimulationCropSeasons");

            migrationBuilder.CreateIndex(
                name: "IX_FarmerContractKpis_CropSeasonId",
                table: "FarmerContractKpis",
                column: "CropSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_FarmerContractKpis_CultureTypeCode",
                table: "FarmerContractKpis",
                column: "CultureTypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_FarmerSegmentations_SegmentationConfigurationId",
                table: "FarmerSegmentations",
                column: "SegmentationConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_SegmentationSimulationKpiScopes_SegmentationSimulationId_KpiKind",
                table: "SegmentationSimulationKpiScopes",
                columns: new[] { "SegmentationSimulationId", "KpiKind" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SegmentationSimulationKpiScopeSeasons_CropSeasonId",
                table: "SegmentationSimulationKpiScopeSeasons",
                column: "CropSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_SegmentationSimulationKpiScopeSeasons_SegmentationSimulationKpiScopeId_CropSeasonId",
                table: "SegmentationSimulationKpiScopeSeasons",
                columns: new[] { "SegmentationSimulationKpiScopeId", "CropSeasonId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FarmerSegmentations_SegmentationConfigurations_SegmentationConfigurationId",
                table: "FarmerSegmentations",
                column: "SegmentationConfigurationId",
                principalTable: "SegmentationConfigurations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException("Down migration is not supported for UnifiedFarmerContractKpiAndPerKpiScopes.");
        }
    }
}
