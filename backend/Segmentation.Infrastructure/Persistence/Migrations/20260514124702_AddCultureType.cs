using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Segmentation.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCultureType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_YieldKpis_FarmerId_CropSeasonId",
                table: "YieldKpis");

            migrationBuilder.DropIndex(
                name: "IX_TechnologiesKpis_FarmerId_CropSeasonId",
                table: "TechnologiesKpis");

            migrationBuilder.DropIndex(
                name: "IX_ScaleKpis_FarmerId_CropSeasonId",
                table: "ScaleKpis");

            migrationBuilder.DropIndex(
                name: "IX_QualityKpis_FarmerId_CropSeasonId",
                table: "QualityKpis");

            migrationBuilder.DropIndex(
                name: "IX_LoyaltyKpis_FarmerId_CropSeasonId",
                table: "LoyaltyKpis");

            migrationBuilder.DropIndex(
                name: "IX_FinancialKpis_FarmerId_CropSeasonId",
                table: "FinancialKpis");

            migrationBuilder.DropIndex(
                name: "IX_EsgKpis_FarmerId_CropSeasonId",
                table: "EsgKpis");

            migrationBuilder.AddColumn<string>(
                name: "CultureTypeCode",
                table: "YieldKpis",
                type: "TEXT",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CultureTypeCode",
                table: "TechnologiesKpis",
                type: "TEXT",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CultureTypeCode",
                table: "SegmentationSimulations",
                type: "TEXT",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CultureTypeCode",
                table: "SegmentationConfigurations",
                type: "TEXT",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CultureTypeCode",
                table: "ScaleKpis",
                type: "TEXT",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CultureTypeCode",
                table: "QualityKpis",
                type: "TEXT",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CultureTypeCode",
                table: "LoyaltyKpis",
                type: "TEXT",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CultureTypeCode",
                table: "FinancialKpis",
                type: "TEXT",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CultureTypeCode",
                table: "EsgKpis",
                type: "TEXT",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CultureTypes",
                columns: table => new
                {
                    Code = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CultureTypes", x => x.Code);
                });

            migrationBuilder.CreateIndex(
                name: "IX_YieldKpis_CultureTypeCode",
                table: "YieldKpis",
                column: "CultureTypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_YieldKpis_FarmerId_CropSeasonId_CultureTypeCode",
                table: "YieldKpis",
                columns: new[] { "FarmerId", "CropSeasonId", "CultureTypeCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TechnologiesKpis_CultureTypeCode",
                table: "TechnologiesKpis",
                column: "CultureTypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_TechnologiesKpis_FarmerId_CropSeasonId_CultureTypeCode",
                table: "TechnologiesKpis",
                columns: new[] { "FarmerId", "CropSeasonId", "CultureTypeCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SegmentationSimulations_CultureTypeCode",
                table: "SegmentationSimulations",
                column: "CultureTypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_SegmentationConfigurations_CultureTypeCode",
                table: "SegmentationConfigurations",
                column: "CultureTypeCode");

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
                name: "IX_QualityKpis_CultureTypeCode",
                table: "QualityKpis",
                column: "CultureTypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_QualityKpis_FarmerId_CropSeasonId_CultureTypeCode",
                table: "QualityKpis",
                columns: new[] { "FarmerId", "CropSeasonId", "CultureTypeCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyKpis_CultureTypeCode",
                table: "LoyaltyKpis",
                column: "CultureTypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyKpis_FarmerId_CropSeasonId_CultureTypeCode",
                table: "LoyaltyKpis",
                columns: new[] { "FarmerId", "CropSeasonId", "CultureTypeCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialKpis_CultureTypeCode",
                table: "FinancialKpis",
                column: "CultureTypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialKpis_FarmerId_CropSeasonId_CultureTypeCode",
                table: "FinancialKpis",
                columns: new[] { "FarmerId", "CropSeasonId", "CultureTypeCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EsgKpis_CultureTypeCode",
                table: "EsgKpis",
                column: "CultureTypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_EsgKpis_FarmerId_CropSeasonId_CultureTypeCode",
                table: "EsgKpis",
                columns: new[] { "FarmerId", "CropSeasonId", "CultureTypeCode" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EsgKpis_CultureTypes_CultureTypeCode",
                table: "EsgKpis",
                column: "CultureTypeCode",
                principalTable: "CultureTypes",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialKpis_CultureTypes_CultureTypeCode",
                table: "FinancialKpis",
                column: "CultureTypeCode",
                principalTable: "CultureTypes",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LoyaltyKpis_CultureTypes_CultureTypeCode",
                table: "LoyaltyKpis",
                column: "CultureTypeCode",
                principalTable: "CultureTypes",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QualityKpis_CultureTypes_CultureTypeCode",
                table: "QualityKpis",
                column: "CultureTypeCode",
                principalTable: "CultureTypes",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ScaleKpis_CultureTypes_CultureTypeCode",
                table: "ScaleKpis",
                column: "CultureTypeCode",
                principalTable: "CultureTypes",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SegmentationConfigurations_CultureTypes_CultureTypeCode",
                table: "SegmentationConfigurations",
                column: "CultureTypeCode",
                principalTable: "CultureTypes",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SegmentationSimulations_CultureTypes_CultureTypeCode",
                table: "SegmentationSimulations",
                column: "CultureTypeCode",
                principalTable: "CultureTypes",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TechnologiesKpis_CultureTypes_CultureTypeCode",
                table: "TechnologiesKpis",
                column: "CultureTypeCode",
                principalTable: "CultureTypes",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_YieldKpis_CultureTypes_CultureTypeCode",
                table: "YieldKpis",
                column: "CultureTypeCode",
                principalTable: "CultureTypes",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EsgKpis_CultureTypes_CultureTypeCode",
                table: "EsgKpis");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialKpis_CultureTypes_CultureTypeCode",
                table: "FinancialKpis");

            migrationBuilder.DropForeignKey(
                name: "FK_LoyaltyKpis_CultureTypes_CultureTypeCode",
                table: "LoyaltyKpis");

            migrationBuilder.DropForeignKey(
                name: "FK_QualityKpis_CultureTypes_CultureTypeCode",
                table: "QualityKpis");

            migrationBuilder.DropForeignKey(
                name: "FK_ScaleKpis_CultureTypes_CultureTypeCode",
                table: "ScaleKpis");

            migrationBuilder.DropForeignKey(
                name: "FK_SegmentationConfigurations_CultureTypes_CultureTypeCode",
                table: "SegmentationConfigurations");

            migrationBuilder.DropForeignKey(
                name: "FK_SegmentationSimulations_CultureTypes_CultureTypeCode",
                table: "SegmentationSimulations");

            migrationBuilder.DropForeignKey(
                name: "FK_TechnologiesKpis_CultureTypes_CultureTypeCode",
                table: "TechnologiesKpis");

            migrationBuilder.DropForeignKey(
                name: "FK_YieldKpis_CultureTypes_CultureTypeCode",
                table: "YieldKpis");

            migrationBuilder.DropTable(
                name: "CultureTypes");

            migrationBuilder.DropIndex(
                name: "IX_YieldKpis_CultureTypeCode",
                table: "YieldKpis");

            migrationBuilder.DropIndex(
                name: "IX_YieldKpis_FarmerId_CropSeasonId_CultureTypeCode",
                table: "YieldKpis");

            migrationBuilder.DropIndex(
                name: "IX_TechnologiesKpis_CultureTypeCode",
                table: "TechnologiesKpis");

            migrationBuilder.DropIndex(
                name: "IX_TechnologiesKpis_FarmerId_CropSeasonId_CultureTypeCode",
                table: "TechnologiesKpis");

            migrationBuilder.DropIndex(
                name: "IX_SegmentationSimulations_CultureTypeCode",
                table: "SegmentationSimulations");

            migrationBuilder.DropIndex(
                name: "IX_SegmentationConfigurations_CultureTypeCode",
                table: "SegmentationConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_ScaleKpis_CultureTypeCode",
                table: "ScaleKpis");

            migrationBuilder.DropIndex(
                name: "IX_ScaleKpis_FarmerId_CropSeasonId_CultureTypeCode",
                table: "ScaleKpis");

            migrationBuilder.DropIndex(
                name: "IX_QualityKpis_CultureTypeCode",
                table: "QualityKpis");

            migrationBuilder.DropIndex(
                name: "IX_QualityKpis_FarmerId_CropSeasonId_CultureTypeCode",
                table: "QualityKpis");

            migrationBuilder.DropIndex(
                name: "IX_LoyaltyKpis_CultureTypeCode",
                table: "LoyaltyKpis");

            migrationBuilder.DropIndex(
                name: "IX_LoyaltyKpis_FarmerId_CropSeasonId_CultureTypeCode",
                table: "LoyaltyKpis");

            migrationBuilder.DropIndex(
                name: "IX_FinancialKpis_CultureTypeCode",
                table: "FinancialKpis");

            migrationBuilder.DropIndex(
                name: "IX_FinancialKpis_FarmerId_CropSeasonId_CultureTypeCode",
                table: "FinancialKpis");

            migrationBuilder.DropIndex(
                name: "IX_EsgKpis_CultureTypeCode",
                table: "EsgKpis");

            migrationBuilder.DropIndex(
                name: "IX_EsgKpis_FarmerId_CropSeasonId_CultureTypeCode",
                table: "EsgKpis");

            migrationBuilder.DropColumn(
                name: "CultureTypeCode",
                table: "YieldKpis");

            migrationBuilder.DropColumn(
                name: "CultureTypeCode",
                table: "TechnologiesKpis");

            migrationBuilder.DropColumn(
                name: "CultureTypeCode",
                table: "SegmentationSimulations");

            migrationBuilder.DropColumn(
                name: "CultureTypeCode",
                table: "SegmentationConfigurations");

            migrationBuilder.DropColumn(
                name: "CultureTypeCode",
                table: "ScaleKpis");

            migrationBuilder.DropColumn(
                name: "CultureTypeCode",
                table: "QualityKpis");

            migrationBuilder.DropColumn(
                name: "CultureTypeCode",
                table: "LoyaltyKpis");

            migrationBuilder.DropColumn(
                name: "CultureTypeCode",
                table: "FinancialKpis");

            migrationBuilder.DropColumn(
                name: "CultureTypeCode",
                table: "EsgKpis");

            migrationBuilder.CreateIndex(
                name: "IX_YieldKpis_FarmerId_CropSeasonId",
                table: "YieldKpis",
                columns: new[] { "FarmerId", "CropSeasonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TechnologiesKpis_FarmerId_CropSeasonId",
                table: "TechnologiesKpis",
                columns: new[] { "FarmerId", "CropSeasonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScaleKpis_FarmerId_CropSeasonId",
                table: "ScaleKpis",
                columns: new[] { "FarmerId", "CropSeasonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QualityKpis_FarmerId_CropSeasonId",
                table: "QualityKpis",
                columns: new[] { "FarmerId", "CropSeasonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyKpis_FarmerId_CropSeasonId",
                table: "LoyaltyKpis",
                columns: new[] { "FarmerId", "CropSeasonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialKpis_FarmerId_CropSeasonId",
                table: "FinancialKpis",
                columns: new[] { "FarmerId", "CropSeasonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EsgKpis_FarmerId_CropSeasonId",
                table: "EsgKpis",
                columns: new[] { "FarmerId", "CropSeasonId" },
                unique: true);
        }
    }
}
