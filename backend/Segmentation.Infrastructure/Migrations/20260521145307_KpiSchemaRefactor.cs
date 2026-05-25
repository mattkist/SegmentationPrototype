using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Segmentation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class KpiSchemaRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TechnologiesKpis_FarmerId_CropSeasonId_CultureTypeCode",
                table: "TechnologiesKpis");

            migrationBuilder.Sql("""
                CREATE TABLE "_TechnologiesKpiUnpivot" (
                    "Id" TEXT NOT NULL PRIMARY KEY,
                    "FarmerId" TEXT NOT NULL,
                    "CropSeasonId" INTEGER NOT NULL,
                    "CultureTypeCode" TEXT NOT NULL,
                    "TechnologyId" INTEGER NOT NULL);
                DELETE FROM "_TechnologiesKpiUnpivot";
                INSERT INTO "_TechnologiesKpiUnpivot" ("Id", "FarmerId", "CropSeasonId", "CultureTypeCode", "TechnologyId")
                SELECT lower(hex(randomblob(16))), "FarmerId", "CropSeasonId", "CultureTypeCode", 1
                FROM "TechnologiesKpis" WHERE "HasLargeBaseRidgeWithMulch" = 1;
                INSERT INTO "_TechnologiesKpiUnpivot" ("Id", "FarmerId", "CropSeasonId", "CultureTypeCode", "TechnologyId")
                SELECT lower(hex(randomblob(16))), "FarmerId", "CropSeasonId", "CultureTypeCode", 2
                FROM "TechnologiesKpis" WHERE "HasBroadGrateFurnace" = 1;
                INSERT INTO "_TechnologiesKpiUnpivot" ("Id", "FarmerId", "CropSeasonId", "CultureTypeCode", "TechnologyId")
                SELECT lower(hex(randomblob(16))), "FarmerId", "CropSeasonId", "CultureTypeCode", 3
                FROM "TechnologiesKpis" WHERE "HasTechnologyPackageAdherence" = 1;
                INSERT INTO "_TechnologiesKpiUnpivot" ("Id", "FarmerId", "CropSeasonId", "CultureTypeCode", "TechnologyId")
                SELECT lower(hex(randomblob(16))), "FarmerId", "CropSeasonId", "CultureTypeCode", 4
                FROM "TechnologiesKpis" WHERE "HasStandardBarn" = 1;
                DROP TABLE "TechnologiesKpis";

                CREATE TABLE "_EsgIrregularityUnpivot" (
                    "Id" TEXT NOT NULL PRIMARY KEY,
                    "FarmerId" TEXT NOT NULL,
                    "CropSeasonId" INTEGER NOT NULL,
                    "CultureTypeCode" TEXT NOT NULL,
                    "IrregularityTypeId" INTEGER NOT NULL);
                DELETE FROM "_EsgIrregularityUnpivot";
                INSERT INTO "_EsgIrregularityUnpivot" ("Id", "FarmerId", "CropSeasonId", "CultureTypeCode", "IrregularityTypeId")
                SELECT lower(hex(randomblob(16))), "FarmerId", "CropSeasonId", "CultureTypeCode", 1
                FROM "EsgKpis" WHERE "HasMinorIrregularity" = 1;
                INSERT INTO "_EsgIrregularityUnpivot" ("Id", "FarmerId", "CropSeasonId", "CultureTypeCode", "IrregularityTypeId")
                SELECT lower(hex(randomblob(16))), "FarmerId", "CropSeasonId", "CultureTypeCode", 2
                FROM "EsgKpis" WHERE "HasMajorIrregularity" = 1;
                """);

            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS "_TechScoreMigration" (
                    "SegmentationConfigurationCultureTypeId" TEXT NOT NULL,
                    "TechnologyId" INTEGER NOT NULL,
                    "Score" INTEGER NOT NULL);
                DELETE FROM "_TechScoreMigration";
                INSERT INTO "_TechScoreMigration" SELECT "SegmentationConfigurationCultureTypeId", 1, "HasLargeBaseRidgeWithMulchScore" FROM "SegmentationConfigurationTechnologies" WHERE "HasLargeBaseRidgeWithMulchScore" > 0;
                INSERT INTO "_TechScoreMigration" SELECT "SegmentationConfigurationCultureTypeId", 2, "HasBroadGrateFurnaceScore" FROM "SegmentationConfigurationTechnologies" WHERE "HasBroadGrateFurnaceScore" > 0;
                INSERT INTO "_TechScoreMigration" SELECT "SegmentationConfigurationCultureTypeId", 3, "HasTechnologyPackageAdherenceScore" FROM "SegmentationConfigurationTechnologies" WHERE "HasTechnologyPackageAdherenceScore" > 0;
                INSERT INTO "_TechScoreMigration" SELECT "SegmentationConfigurationCultureTypeId", 4, "HasStandardBarnScore" FROM "SegmentationConfigurationTechnologies" WHERE "HasStandardBarnScore" > 0;

                CREATE TABLE IF NOT EXISTS "_EsgIrrScoreMigration" (
                    "SegmentationConfigurationCultureTypeId" TEXT NOT NULL,
                    "IrregularityTypeId" INTEGER NOT NULL,
                    "Score" INTEGER NOT NULL);
                DELETE FROM "_EsgIrrScoreMigration";
                INSERT INTO "_EsgIrrScoreMigration" SELECT "SegmentationConfigurationCultureTypeId", 1, "MinorIrregularityScore" FROM "SegmentationConfigurationEsgs" WHERE "MinorIrregularityScore" <> 0;
                INSERT INTO "_EsgIrrScoreMigration" SELECT "SegmentationConfigurationCultureTypeId", 2, "MajorIrregularityScore" FROM "SegmentationConfigurationEsgs" WHERE "MajorIrregularityScore" <> 0;
                """);

            migrationBuilder.DropColumn(
                name: "HasBroadGrateFurnaceScore",
                table: "SegmentationConfigurationTechnologies");

            migrationBuilder.DropColumn(
                name: "HasLargeBaseRidgeWithMulchScore",
                table: "SegmentationConfigurationTechnologies");

            migrationBuilder.DropColumn(
                name: "HasStandardBarnScore",
                table: "SegmentationConfigurationTechnologies");

            migrationBuilder.DropColumn(
                name: "HasTechnologyPackageAdherenceScore",
                table: "SegmentationConfigurationTechnologies");

            migrationBuilder.DropColumn(
                name: "MajorIrregularityScore",
                table: "SegmentationConfigurationEsgs");

            migrationBuilder.DropColumn(
                name: "MinorIrregularityScore",
                table: "SegmentationConfigurationEsgs");

            migrationBuilder.DropColumn(
                name: "HasMajorIrregularity",
                table: "EsgKpis");

            migrationBuilder.DropColumn(
                name: "HasMinorIrregularity",
                table: "EsgKpis");

            migrationBuilder.AddColumn<bool>(
                name: "IsNewFarmer",
                table: "SegmentationSimulationFarmers",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ContractedAmountKg",
                table: "LoyaltyKpis",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DeliveredAmountKg",
                table: "LoyaltyKpis",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "IrregularityTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IrregularityTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Technologies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Technologies", x => x.Id);
                });

            migrationBuilder.Sql("""
                INSERT OR IGNORE INTO "Technologies" ("Id", "Name") VALUES
                (1, 'Large Base Ridge With Mulch'),
                (2, 'Broad Grate Furnace'),
                (3, 'Tecknology Package Adderence'),
                (4, 'Standard Barn');

                INSERT OR IGNORE INTO "IrregularityTypes" ("Id", "Name") VALUES
                (1, 'Minor Irregularity'),
                (2, 'Major Irregularity');
                """);

            migrationBuilder.CreateTable(
                name: "TechnologiesKpis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FarmerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CropSeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    CultureTypeCode = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    TechnologyId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnologiesKpis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TechnologiesKpis_CropSeasons_CropSeasonId",
                        column: x => x.CropSeasonId,
                        principalTable: "CropSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TechnologiesKpis_CultureTypes_CultureTypeCode",
                        column: x => x.CultureTypeCode,
                        principalTable: "CultureTypes",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TechnologiesKpis_Farmers_FarmerId",
                        column: x => x.FarmerId,
                        principalTable: "Farmers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TechnologiesKpis_Technologies_TechnologyId",
                        column: x => x.TechnologyId,
                        principalTable: "Technologies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql("""
                INSERT INTO "TechnologiesKpis" ("Id", "FarmerId", "CropSeasonId", "CultureTypeCode", "TechnologyId")
                SELECT "Id", "FarmerId", "CropSeasonId", "CultureTypeCode", "TechnologyId" FROM "_TechnologiesKpiUnpivot";
                DROP TABLE IF EXISTS "_TechnologiesKpiUnpivot";
                """);

            migrationBuilder.CreateTable(
                name: "YieldAndScaleKpis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FarmerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CropSeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    CultureTypeCode = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Yield = table.Column<int>(type: "INTEGER", nullable: false),
                    Scale = table.Column<int>(type: "INTEGER", nullable: false),
                    ContractedAmountKg = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YieldAndScaleKpis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YieldAndScaleKpis_CropSeasons_CropSeasonId",
                        column: x => x.CropSeasonId,
                        principalTable: "CropSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_YieldAndScaleKpis_CultureTypes_CultureTypeCode",
                        column: x => x.CultureTypeCode,
                        principalTable: "CultureTypes",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_YieldAndScaleKpis_Farmers_FarmerId",
                        column: x => x.FarmerId,
                        principalTable: "Farmers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EsgIrregularityKpis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FarmerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CropSeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    CultureTypeCode = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    IrregularityTypeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EsgIrregularityKpis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EsgIrregularityKpis_CropSeasons_CropSeasonId",
                        column: x => x.CropSeasonId,
                        principalTable: "CropSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EsgIrregularityKpis_CultureTypes_CultureTypeCode",
                        column: x => x.CultureTypeCode,
                        principalTable: "CultureTypes",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EsgIrregularityKpis_Farmers_FarmerId",
                        column: x => x.FarmerId,
                        principalTable: "Farmers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EsgIrregularityKpis_IrregularityTypes_IrregularityTypeId",
                        column: x => x.IrregularityTypeId,
                        principalTable: "IrregularityTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SegmentationConfigurationEsgIrregularityScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SegmentationConfigurationCultureTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IrregularityTypeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentationConfigurationEsgIrregularityScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SegmentationConfigurationEsgIrregularityScores_IrregularityTypes_IrregularityTypeId",
                        column: x => x.IrregularityTypeId,
                        principalTable: "IrregularityTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SegmentationConfigurationEsgIrregularityScores_SegmentationConfigurationEsgs_SegmentationConfigurationCultureTypeId",
                        column: x => x.SegmentationConfigurationCultureTypeId,
                        principalTable: "SegmentationConfigurationEsgs",
                        principalColumn: "SegmentationConfigurationCultureTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SegmentationConfigurationTechnologyScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SegmentationConfigurationCultureTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TechnologyId = table.Column<int>(type: "INTEGER", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentationConfigurationTechnologyScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SegmentationConfigurationTechnologyScores_SegmentationConfigurationTechnologies_SegmentationConfigurationCultureTypeId",
                        column: x => x.SegmentationConfigurationCultureTypeId,
                        principalTable: "SegmentationConfigurationTechnologies",
                        principalColumn: "SegmentationConfigurationCultureTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SegmentationConfigurationTechnologyScores_Technologies_TechnologyId",
                        column: x => x.TechnologyId,
                        principalTable: "Technologies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TechnologiesKpis_CropSeasonId",
                table: "TechnologiesKpis",
                column: "CropSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_TechnologiesKpis_CultureTypeCode",
                table: "TechnologiesKpis",
                column: "CultureTypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_TechnologiesKpis_FarmerId_CropSeasonId_CultureTypeCode_TechnologyId",
                table: "TechnologiesKpis",
                columns: new[] { "FarmerId", "CropSeasonId", "CultureTypeCode", "TechnologyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TechnologiesKpis_TechnologyId",
                table: "TechnologiesKpis",
                column: "TechnologyId");

            migrationBuilder.CreateIndex(
                name: "IX_EsgIrregularityKpis_CropSeasonId",
                table: "EsgIrregularityKpis",
                column: "CropSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_EsgIrregularityKpis_CultureTypeCode",
                table: "EsgIrregularityKpis",
                column: "CultureTypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_EsgIrregularityKpis_FarmerId_CropSeasonId_CultureTypeCode_IrregularityTypeId",
                table: "EsgIrregularityKpis",
                columns: new[] { "FarmerId", "CropSeasonId", "CultureTypeCode", "IrregularityTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EsgIrregularityKpis_IrregularityTypeId",
                table: "EsgIrregularityKpis",
                column: "IrregularityTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SegmentationConfigurationEsgIrregularityScores_IrregularityTypeId",
                table: "SegmentationConfigurationEsgIrregularityScores",
                column: "IrregularityTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SegmentationConfigurationEsgIrregularityScores_SegmentationConfigurationCultureTypeId_IrregularityTypeId",
                table: "SegmentationConfigurationEsgIrregularityScores",
                columns: new[] { "SegmentationConfigurationCultureTypeId", "IrregularityTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SegmentationConfigurationTechnologyScores_SegmentationConfigurationCultureTypeId_TechnologyId",
                table: "SegmentationConfigurationTechnologyScores",
                columns: new[] { "SegmentationConfigurationCultureTypeId", "TechnologyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SegmentationConfigurationTechnologyScores_TechnologyId",
                table: "SegmentationConfigurationTechnologyScores",
                column: "TechnologyId");

            migrationBuilder.Sql("""
                INSERT INTO "SegmentationConfigurationTechnologyScores" ("Id", "SegmentationConfigurationCultureTypeId", "TechnologyId", "Score")
                SELECT lower(hex(randomblob(16))), "SegmentationConfigurationCultureTypeId", "TechnologyId", "Score" FROM "_TechScoreMigration";

                INSERT INTO "SegmentationConfigurationEsgIrregularityScores" ("Id", "SegmentationConfigurationCultureTypeId", "IrregularityTypeId", "Score")
                SELECT lower(hex(randomblob(16))), "SegmentationConfigurationCultureTypeId", "IrregularityTypeId", "Score" FROM "_EsgIrrScoreMigration";

                DROP TABLE IF EXISTS "_TechScoreMigration";
                DROP TABLE IF EXISTS "_EsgIrrScoreMigration";
                """);

            migrationBuilder.Sql("""
                INSERT INTO "EsgIrregularityKpis" ("Id", "FarmerId", "CropSeasonId", "CultureTypeCode", "IrregularityTypeId")
                SELECT "Id", "FarmerId", "CropSeasonId", "CultureTypeCode", "IrregularityTypeId" FROM "_EsgIrregularityUnpivot";
                DROP TABLE IF EXISTS "_EsgIrregularityUnpivot";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_YieldAndScaleKpis_CropSeasonId",
                table: "YieldAndScaleKpis",
                column: "CropSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_YieldAndScaleKpis_CultureTypeCode",
                table: "YieldAndScaleKpis",
                column: "CultureTypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_YieldAndScaleKpis_FarmerId_CropSeasonId_CultureTypeCode",
                table: "YieldAndScaleKpis",
                columns: new[] { "FarmerId", "CropSeasonId", "CultureTypeCode" },
                unique: true);

            migrationBuilder.Sql("""
                INSERT INTO "YieldAndScaleKpis" ("Id", "FarmerId", "CropSeasonId", "CultureTypeCode", "Yield", "Scale", "ContractedAmountKg")
                SELECT lower(hex(randomblob(16))), y."FarmerId", y."CropSeasonId", y."CultureTypeCode", y."Yield", s."Scale", y."Yield" * s."Scale"
                FROM "YieldKpis" y
                INNER JOIN "ScaleKpis" s ON y."FarmerId" = s."FarmerId" AND y."CropSeasonId" = s."CropSeasonId" AND y."CultureTypeCode" = s."CultureTypeCode"
                WHERE NOT EXISTS (
                    SELECT 1 FROM "YieldAndScaleKpis" x
                    WHERE x."FarmerId" = y."FarmerId" AND x."CropSeasonId" = y."CropSeasonId" AND x."CultureTypeCode" = y."CultureTypeCode");
                """);

            migrationBuilder.Sql("""
                UPDATE "LoyaltyKpis"
                SET "DeliveredAmountKg" = CAST(ROUND("DeliveredPercentage" * 10000.0 / 100.0) AS INTEGER),
                    "ContractedAmountKg" = CASE WHEN "ContractedAmountKg" = 0 THEN 10000 ELSE "ContractedAmountKg" END
                WHERE "DeliveredAmountKg" = 0 OR "ContractedAmountKg" = 0;
                """);

            migrationBuilder.DropTable(name: "ScaleKpis");
            migrationBuilder.DropTable(name: "YieldKpis");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TechnologiesKpis_Technologies_TechnologyId",
                table: "TechnologiesKpis");

            migrationBuilder.DropTable(
                name: "EsgIrregularityKpis");

            migrationBuilder.DropTable(
                name: "SegmentationConfigurationEsgIrregularityScores");

            migrationBuilder.DropTable(
                name: "SegmentationConfigurationTechnologyScores");

            migrationBuilder.DropTable(
                name: "YieldAndScaleKpis");

            migrationBuilder.DropTable(
                name: "IrregularityTypes");

            migrationBuilder.DropTable(
                name: "Technologies");

            migrationBuilder.DropIndex(
                name: "IX_TechnologiesKpis_FarmerId_CropSeasonId_CultureTypeCode_TechnologyId",
                table: "TechnologiesKpis");

            migrationBuilder.DropIndex(
                name: "IX_TechnologiesKpis_TechnologyId",
                table: "TechnologiesKpis");

            migrationBuilder.DropColumn(
                name: "IsNewFarmer",
                table: "SegmentationSimulationFarmers");

            migrationBuilder.DropColumn(
                name: "ContractedAmountKg",
                table: "LoyaltyKpis");

            migrationBuilder.DropColumn(
                name: "DeliveredAmountKg",
                table: "LoyaltyKpis");

            migrationBuilder.RenameColumn(
                name: "TechnologyId",
                table: "TechnologiesKpis",
                newName: "HasTechnologyPackageAdherence");

            migrationBuilder.AddColumn<bool>(
                name: "HasBroadGrateFurnace",
                table: "TechnologiesKpis",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasLargeBaseRidgeWithMulch",
                table: "TechnologiesKpis",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasStandardBarn",
                table: "TechnologiesKpis",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "HasBroadGrateFurnaceScore",
                table: "SegmentationConfigurationTechnologies",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HasLargeBaseRidgeWithMulchScore",
                table: "SegmentationConfigurationTechnologies",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HasStandardBarnScore",
                table: "SegmentationConfigurationTechnologies",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HasTechnologyPackageAdherenceScore",
                table: "SegmentationConfigurationTechnologies",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MajorIrregularityScore",
                table: "SegmentationConfigurationEsgs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinorIrregularityScore",
                table: "SegmentationConfigurationEsgs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "HasMajorIrregularity",
                table: "EsgKpis",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasMinorIrregularity",
                table: "EsgKpis",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ScaleKpis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CropSeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    CultureTypeCode = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    FarmerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Scale = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScaleKpis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScaleKpis_CropSeasons_CropSeasonId",
                        column: x => x.CropSeasonId,
                        principalTable: "CropSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScaleKpis_CultureTypes_CultureTypeCode",
                        column: x => x.CultureTypeCode,
                        principalTable: "CultureTypes",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScaleKpis_Farmers_FarmerId",
                        column: x => x.FarmerId,
                        principalTable: "Farmers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YieldKpis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CropSeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    CultureTypeCode = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    FarmerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Yield = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YieldKpis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YieldKpis_CropSeasons_CropSeasonId",
                        column: x => x.CropSeasonId,
                        principalTable: "CropSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_YieldKpis_CultureTypes_CultureTypeCode",
                        column: x => x.CultureTypeCode,
                        principalTable: "CultureTypes",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_YieldKpis_Farmers_FarmerId",
                        column: x => x.FarmerId,
                        principalTable: "Farmers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TechnologiesKpis_FarmerId_CropSeasonId_CultureTypeCode",
                table: "TechnologiesKpis",
                columns: new[] { "FarmerId", "CropSeasonId", "CultureTypeCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScaleKpis_CropSeasonId",
                table: "ScaleKpis",
                column: "CropSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_ScaleKpis_CultureTypeCode",
                table: "ScaleKpis",
                column: "CultureTypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_ScaleKpis_FarmerId_CropSeasonId_CultureTypeCode",
                table: "ScaleKpis",
                columns: new[] { "FarmerId", "CropSeasonId", "CultureTypeCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_YieldKpis_CropSeasonId",
                table: "YieldKpis",
                column: "CropSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_YieldKpis_CultureTypeCode",
                table: "YieldKpis",
                column: "CultureTypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_YieldKpis_FarmerId_CropSeasonId_CultureTypeCode",
                table: "YieldKpis",
                columns: new[] { "FarmerId", "CropSeasonId", "CultureTypeCode" },
                unique: true);
        }
    }
}
