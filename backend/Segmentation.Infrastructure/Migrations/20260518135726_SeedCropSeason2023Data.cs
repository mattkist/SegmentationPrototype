using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Segmentation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedCropSeason2023Data : Migration
    {
        private const int SourceSeasonId = 2024;
        private const int TargetSeasonId = 2023;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                $"""
                INSERT INTO "CropSeasons" ("Id", "Code")
                SELECT {TargetSeasonId}, '{TargetSeasonId}'
                WHERE NOT EXISTS (SELECT 1 FROM "CropSeasons" WHERE "Id" = {TargetSeasonId});
                """);

            CopyKpiTable(migrationBuilder, "LoyaltyKpis", """
                "Id", "FarmerId", "CropSeasonId", "CultureTypeCode", "DeliveredPercentage"
                """, """
                ('d' || substr("Id", 2)), "FarmerId", {0}, "CultureTypeCode", "DeliveredPercentage"
                """);

            CopyKpiTable(migrationBuilder, "QualityKpis", """
                "Id", "FarmerId", "CropSeasonId", "CultureTypeCode", "Iqs", "HadNtrm", "HadQualityMixture"
                """, """
                ('d' || substr("Id", 2)), "FarmerId", {0}, "CultureTypeCode", "Iqs", "HadNtrm", "HadQualityMixture"
                """);

            CopyKpiTable(migrationBuilder, "FinancialKpis", """
                "Id", "FarmerId", "CropSeasonId", "CultureTypeCode", "SelfFundingPercentage", "HaveDebt"
                """, """
                ('d' || substr("Id", 2)), "FarmerId", {0}, "CultureTypeCode", "SelfFundingPercentage", "HaveDebt"
                """);

            CopyKpiTable(migrationBuilder, "YieldKpis", """
                "Id", "FarmerId", "CropSeasonId", "CultureTypeCode", "Yield"
                """, """
                ('d' || substr("Id", 2)), "FarmerId", {0}, "CultureTypeCode", "Yield"
                """);

            CopyKpiTable(migrationBuilder, "ScaleKpis", """
                "Id", "FarmerId", "CropSeasonId", "CultureTypeCode", "Scale"
                """, """
                ('d' || substr("Id", 2)), "FarmerId", {0}, "CultureTypeCode", "Scale"
                """);

            CopyKpiTable(migrationBuilder, "TechnologiesKpis", """
                "Id", "FarmerId", "CropSeasonId", "CultureTypeCode", "HasLargeBaseRidgeWithMulch", "HasBroadGrateFurnace", "HasTechnologyPackageAdherence", "HasStandardBarn"
                """, """
                ('d' || substr("Id", 2)), "FarmerId", {0}, "CultureTypeCode", "HasLargeBaseRidgeWithMulch", "HasBroadGrateFurnace", "HasTechnologyPackageAdherence", "HasStandardBarn"
                """);

            CopyKpiTable(migrationBuilder, "EsgKpis", """
                "Id", "FarmerId", "CropSeasonId", "CultureTypeCode", "ReforestationPercentage", "NativeForestPercentage", "HasMinorIrregularity", "HasMajorIrregularity"
                """, """
                ('d' || substr("Id", 2)), "FarmerId", {0}, "CultureTypeCode", "ReforestationPercentage", "NativeForestPercentage", "HasMinorIrregularity", "HasMajorIrregularity"
                """);
        }

        private static void CopyKpiTable(
            MigrationBuilder migrationBuilder,
            string table,
            string insertColumns,
            string selectColumnsTemplate)
        {
            var selectColumns = string.Format(selectColumnsTemplate, TargetSeasonId);
            migrationBuilder.Sql(
                $"""
                INSERT INTO "{table}" ({insertColumns})
                SELECT {selectColumns}
                FROM "{table}" AS src
                WHERE src."CropSeasonId" = {SourceSeasonId}
                  AND NOT EXISTS (
                    SELECT 1
                    FROM "{table}" AS existing
                    WHERE existing."FarmerId" = src."FarmerId"
                      AND existing."CropSeasonId" = {TargetSeasonId}
                      AND existing."CultureTypeCode" = src."CultureTypeCode"
                  );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"""DELETE FROM "LoyaltyKpis" WHERE "CropSeasonId" = {TargetSeasonId};""");
            migrationBuilder.Sql($"""DELETE FROM "QualityKpis" WHERE "CropSeasonId" = {TargetSeasonId};""");
            migrationBuilder.Sql($"""DELETE FROM "FinancialKpis" WHERE "CropSeasonId" = {TargetSeasonId};""");
            migrationBuilder.Sql($"""DELETE FROM "YieldKpis" WHERE "CropSeasonId" = {TargetSeasonId};""");
            migrationBuilder.Sql($"""DELETE FROM "ScaleKpis" WHERE "CropSeasonId" = {TargetSeasonId};""");
            migrationBuilder.Sql($"""DELETE FROM "TechnologiesKpis" WHERE "CropSeasonId" = {TargetSeasonId};""");
            migrationBuilder.Sql($"""DELETE FROM "EsgKpis" WHERE "CropSeasonId" = {TargetSeasonId};""");
            migrationBuilder.Sql($"""DELETE FROM "CropSeasons" WHERE "Id" = {TargetSeasonId};""");
        }
    }
}
