using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Segmentation.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CropSeasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CropSeasons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FarmerClusters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FarmerClusters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Farmers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    NonExclusiveFarmer = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Farmers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SegmentationConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    MaximumScore = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentationConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EsgKpis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FarmerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CropSeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReforestationPercentage = table.Column<int>(type: "INTEGER", nullable: false),
                    NativeForestPercentage = table.Column<int>(type: "INTEGER", nullable: false),
                    HasMinorIrregularity = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasMajorIrregularity = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EsgKpis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EsgKpis_CropSeasons_CropSeasonId",
                        column: x => x.CropSeasonId,
                        principalTable: "CropSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EsgKpis_Farmers_FarmerId",
                        column: x => x.FarmerId,
                        principalTable: "Farmers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FarmerClusterFarmers",
                columns: table => new
                {
                    ClusterId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FarmerId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FarmerClusterFarmers", x => new { x.ClusterId, x.FarmerId });
                    table.ForeignKey(
                        name: "FK_FarmerClusterFarmers_FarmerClusters_ClusterId",
                        column: x => x.ClusterId,
                        principalTable: "FarmerClusters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FarmerClusterFarmers_Farmers_FarmerId",
                        column: x => x.FarmerId,
                        principalTable: "Farmers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FinancialKpis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FarmerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CropSeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    SelfFundingPercentage = table.Column<int>(type: "INTEGER", nullable: false),
                    HaveDebt = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialKpis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialKpis_CropSeasons_CropSeasonId",
                        column: x => x.CropSeasonId,
                        principalTable: "CropSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialKpis_Farmers_FarmerId",
                        column: x => x.FarmerId,
                        principalTable: "Farmers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyKpis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FarmerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CropSeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    DeliveredPercentage = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyKpis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltyKpis_CropSeasons_CropSeasonId",
                        column: x => x.CropSeasonId,
                        principalTable: "CropSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoyaltyKpis_Farmers_FarmerId",
                        column: x => x.FarmerId,
                        principalTable: "Farmers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QualityKpis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FarmerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CropSeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    Iqs = table.Column<int>(type: "INTEGER", nullable: false),
                    HadNtrm = table.Column<bool>(type: "INTEGER", nullable: false),
                    HadQualityMixture = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityKpis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QualityKpis_CropSeasons_CropSeasonId",
                        column: x => x.CropSeasonId,
                        principalTable: "CropSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QualityKpis_Farmers_FarmerId",
                        column: x => x.FarmerId,
                        principalTable: "Farmers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScaleKpis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FarmerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CropSeasonId = table.Column<int>(type: "INTEGER", nullable: false),
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
                        name: "FK_ScaleKpis_Farmers_FarmerId",
                        column: x => x.FarmerId,
                        principalTable: "Farmers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TechnologiesKpis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FarmerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CropSeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    HasLargeBaseRidgeWithMulch = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasBroadGrateFurnace = table.Column<bool>(type: "INTEGER", nullable: false),
                    HasTechnologyPackageAdherence = table.Column<bool>(type: "INTEGER", nullable: false)
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
                        name: "FK_TechnologiesKpis_Farmers_FarmerId",
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
                    FarmerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CropSeasonId = table.Column<int>(type: "INTEGER", nullable: false),
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
                        name: "FK_YieldKpis_Farmers_FarmerId",
                        column: x => x.FarmerId,
                        principalTable: "Farmers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SegmentationConfigurationEsgs",
                columns: table => new
                {
                    SegmentationConfigurationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MaxScore = table.Column<int>(type: "INTEGER", nullable: false),
                    Relevance = table.Column<decimal>(type: "TEXT", precision: 9, scale: 4, nullable: false),
                    ReforestationCropSeason = table.Column<int>(type: "INTEGER", nullable: false),
                    ReforestationScorePerPercentualPoint = table.Column<int>(type: "INTEGER", nullable: false),
                    ReforestationMaximumScore = table.Column<int>(type: "INTEGER", nullable: false),
                    NativeForestCropSeason = table.Column<int>(type: "INTEGER", nullable: false),
                    NativeForestScorePerPercentualPoint = table.Column<int>(type: "INTEGER", nullable: false),
                    NativeForestMaximumScore = table.Column<int>(type: "INTEGER", nullable: false),
                    MinorIrregularityCropSeason = table.Column<int>(type: "INTEGER", nullable: false),
                    MinorIrregularityScore = table.Column<int>(type: "INTEGER", nullable: false),
                    MajorIrregularityCropSeason = table.Column<int>(type: "INTEGER", nullable: false),
                    MajorIrregularityScore = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentationConfigurationEsgs", x => x.SegmentationConfigurationId);
                    table.ForeignKey(
                        name: "FK_SegmentationConfigurationEsgs_SegmentationConfigurations_SegmentationConfigurationId",
                        column: x => x.SegmentationConfigurationId,
                        principalTable: "SegmentationConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SegmentationConfigurationFinancials",
                columns: table => new
                {
                    SegmentationConfigurationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MaxScore = table.Column<int>(type: "INTEGER", nullable: false),
                    Relevance = table.Column<decimal>(type: "TEXT", precision: 9, scale: 4, nullable: false),
                    DebtCropSeason = table.Column<int>(type: "INTEGER", nullable: false),
                    DebtScore = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentationConfigurationFinancials", x => x.SegmentationConfigurationId);
                    table.ForeignKey(
                        name: "FK_SegmentationConfigurationFinancials_SegmentationConfigurations_SegmentationConfigurationId",
                        column: x => x.SegmentationConfigurationId,
                        principalTable: "SegmentationConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SegmentationConfigurationLoyalties",
                columns: table => new
                {
                    SegmentationConfigurationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MaxScore = table.Column<int>(type: "INTEGER", nullable: false),
                    Relevance = table.Column<decimal>(type: "TEXT", precision: 9, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentationConfigurationLoyalties", x => x.SegmentationConfigurationId);
                    table.ForeignKey(
                        name: "FK_SegmentationConfigurationLoyalties_SegmentationConfigurations_SegmentationConfigurationId",
                        column: x => x.SegmentationConfigurationId,
                        principalTable: "SegmentationConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SegmentationConfigurationQualities",
                columns: table => new
                {
                    SegmentationConfigurationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MaxScore = table.Column<int>(type: "INTEGER", nullable: false),
                    Relevance = table.Column<decimal>(type: "TEXT", precision: 9, scale: 4, nullable: false),
                    NtrmCropSeasonAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    NtrmCropSeasonStart = table.Column<int>(type: "INTEGER", nullable: false),
                    NtrmScore = table.Column<int>(type: "INTEGER", nullable: false),
                    MixtureCropSeasonAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    MixtureCropSeasonStart = table.Column<int>(type: "INTEGER", nullable: false),
                    MixtureScore = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentationConfigurationQualities", x => x.SegmentationConfigurationId);
                    table.ForeignKey(
                        name: "FK_SegmentationConfigurationQualities_SegmentationConfigurations_SegmentationConfigurationId",
                        column: x => x.SegmentationConfigurationId,
                        principalTable: "SegmentationConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SegmentationConfigurationScales",
                columns: table => new
                {
                    SegmentationConfigurationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MaxScore = table.Column<int>(type: "INTEGER", nullable: false),
                    Relevance = table.Column<decimal>(type: "TEXT", precision: 9, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentationConfigurationScales", x => x.SegmentationConfigurationId);
                    table.ForeignKey(
                        name: "FK_SegmentationConfigurationScales_SegmentationConfigurations_SegmentationConfigurationId",
                        column: x => x.SegmentationConfigurationId,
                        principalTable: "SegmentationConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SegmentationConfigurationTechnologies",
                columns: table => new
                {
                    SegmentationConfigurationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MaxScore = table.Column<int>(type: "INTEGER", nullable: false),
                    Relevance = table.Column<decimal>(type: "TEXT", precision: 9, scale: 4, nullable: false),
                    HasLargeBaseRidgeWithMulchCropSeason = table.Column<int>(type: "INTEGER", nullable: false),
                    HasLargeBaseRidgeWithMulchScore = table.Column<int>(type: "INTEGER", nullable: false),
                    HasBroadGrateFurnaceCropSeason = table.Column<int>(type: "INTEGER", nullable: false),
                    HasBroadGrateFurnaceScore = table.Column<int>(type: "INTEGER", nullable: false),
                    HasTechnologyPackageAdherenceCropSeason = table.Column<int>(type: "INTEGER", nullable: false),
                    HasTechnologyPackageAdherenceScore = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentationConfigurationTechnologies", x => x.SegmentationConfigurationId);
                    table.ForeignKey(
                        name: "FK_SegmentationConfigurationTechnologies_SegmentationConfigurations_SegmentationConfigurationId",
                        column: x => x.SegmentationConfigurationId,
                        principalTable: "SegmentationConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SegmentationConfigurationYields",
                columns: table => new
                {
                    SegmentationConfigurationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MaxScore = table.Column<int>(type: "INTEGER", nullable: false),
                    Relevance = table.Column<decimal>(type: "TEXT", precision: 9, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentationConfigurationYields", x => x.SegmentationConfigurationId);
                    table.ForeignKey(
                        name: "FK_SegmentationConfigurationYields_SegmentationConfigurations_SegmentationConfigurationId",
                        column: x => x.SegmentationConfigurationId,
                        principalTable: "SegmentationConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SegmentationSegments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SegmentationConfigurationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SegmentName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    RangeMin = table.Column<int>(type: "INTEGER", nullable: true),
                    OnlyExclusiveFarmer = table.Column<bool>(type: "INTEGER", nullable: false),
                    BankDepositDiscount = table.Column<int>(type: "INTEGER", nullable: false),
                    TobaccoDiscount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentationSegments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SegmentationSegments_SegmentationConfigurations_SegmentationConfigurationId",
                        column: x => x.SegmentationConfigurationId,
                        principalTable: "SegmentationConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SegmentationSimulations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SegmentationConfigurationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CropSeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    SimulationDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 1, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentationSimulations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SegmentationSimulations_CropSeasons_CropSeasonId",
                        column: x => x.CropSeasonId,
                        principalTable: "CropSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SegmentationSimulations_SegmentationConfigurations_SegmentationConfigurationId",
                        column: x => x.SegmentationConfigurationId,
                        principalTable: "SegmentationConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinancialSelfFundingRanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SegmentationConfigurationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Minimum = table.Column<int>(type: "INTEGER", nullable: false),
                    Maximum = table.Column<int>(type: "INTEGER", nullable: false),
                    CropSeasonAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    CropSeasonStart = table.Column<int>(type: "INTEGER", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialSelfFundingRanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialSelfFundingRanges_SegmentationConfigurationFinancials_SegmentationConfigurationId",
                        column: x => x.SegmentationConfigurationId,
                        principalTable: "SegmentationConfigurationFinancials",
                        principalColumn: "SegmentationConfigurationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyHistoricalVolumeRanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SegmentationConfigurationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MinimumDeliveryAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    MaximumDeliveryAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyHistoricalVolumeRanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltyHistoricalVolumeRanges_SegmentationConfigurationLoyalties_SegmentationConfigurationId",
                        column: x => x.SegmentationConfigurationId,
                        principalTable: "SegmentationConfigurationLoyalties",
                        principalColumn: "SegmentationConfigurationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoyaltySeasonQuantityRanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SegmentationConfigurationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlantingCropSeasonAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    CropSeasonStart = table.Column<int>(type: "INTEGER", nullable: false),
                    MinimumDeliveryAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    MaximumDeliveryAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    DeliveryCropSeasonAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltySeasonQuantityRanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltySeasonQuantityRanges_SegmentationConfigurationLoyalties_SegmentationConfigurationId",
                        column: x => x.SegmentationConfigurationId,
                        principalTable: "SegmentationConfigurationLoyalties",
                        principalColumn: "SegmentationConfigurationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QualityIqsRanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SegmentationConfigurationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Minimum = table.Column<int>(type: "INTEGER", nullable: false),
                    Maximum = table.Column<int>(type: "INTEGER", nullable: false),
                    CropSeasonAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    CropSeasonStart = table.Column<int>(type: "INTEGER", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityIqsRanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QualityIqsRanges_SegmentationConfigurationQualities_SegmentationConfigurationId",
                        column: x => x.SegmentationConfigurationId,
                        principalTable: "SegmentationConfigurationQualities",
                        principalColumn: "SegmentationConfigurationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScaleRanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SegmentationConfigurationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Minimum = table.Column<int>(type: "INTEGER", nullable: false),
                    Maximum = table.Column<int>(type: "INTEGER", nullable: false),
                    CropSeasonAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    CropSeasonStart = table.Column<int>(type: "INTEGER", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScaleRanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScaleRanges_SegmentationConfigurationScales_SegmentationConfigurationId",
                        column: x => x.SegmentationConfigurationId,
                        principalTable: "SegmentationConfigurationScales",
                        principalColumn: "SegmentationConfigurationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YieldRanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SegmentationConfigurationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Minimum = table.Column<int>(type: "INTEGER", nullable: false),
                    Maximum = table.Column<int>(type: "INTEGER", nullable: false),
                    CropSeasonAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    CropSeasonStart = table.Column<int>(type: "INTEGER", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YieldRanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YieldRanges_SegmentationConfigurationYields_SegmentationConfigurationId",
                        column: x => x.SegmentationConfigurationId,
                        principalTable: "SegmentationConfigurationYields",
                        principalColumn: "SegmentationConfigurationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FarmerSegmentations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FarmerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CropSeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalScore = table.Column<int>(type: "INTEGER", nullable: false),
                    LoyaltyScore = table.Column<int>(type: "INTEGER", nullable: false),
                    QualityScore = table.Column<int>(type: "INTEGER", nullable: false),
                    FinancialScore = table.Column<int>(type: "INTEGER", nullable: false),
                    TechnologiesScore = table.Column<int>(type: "INTEGER", nullable: false),
                    EsgScore = table.Column<int>(type: "INTEGER", nullable: false),
                    YieldScore = table.Column<int>(type: "INTEGER", nullable: false),
                    ScaleScore = table.Column<int>(type: "INTEGER", nullable: false),
                    NonExclusiveFarmer = table.Column<bool>(type: "INTEGER", nullable: false),
                    SegmentationConfigurationSegmentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Rank = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FarmerSegmentations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FarmerSegmentations_CropSeasons_CropSeasonId",
                        column: x => x.CropSeasonId,
                        principalTable: "CropSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FarmerSegmentations_Farmers_FarmerId",
                        column: x => x.FarmerId,
                        principalTable: "Farmers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FarmerSegmentations_SegmentationSegments_SegmentationConfigurationSegmentId",
                        column: x => x.SegmentationConfigurationSegmentId,
                        principalTable: "SegmentationSegments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SegmentationSimulationFarmers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SegmentationSimulationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FarmerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TotalScore = table.Column<int>(type: "INTEGER", nullable: false),
                    LoyaltyScore = table.Column<int>(type: "INTEGER", nullable: false),
                    QualityScore = table.Column<int>(type: "INTEGER", nullable: false),
                    FinancialScore = table.Column<int>(type: "INTEGER", nullable: false),
                    TechnologiesScore = table.Column<int>(type: "INTEGER", nullable: false),
                    EsgScore = table.Column<int>(type: "INTEGER", nullable: false),
                    YieldScore = table.Column<int>(type: "INTEGER", nullable: false),
                    ScaleScore = table.Column<int>(type: "INTEGER", nullable: false),
                    NonExclusiveFarmer = table.Column<bool>(type: "INTEGER", nullable: false),
                    SegmentationConfigurationSegmentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Rank = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentationSimulationFarmers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SegmentationSimulationFarmers_Farmers_FarmerId",
                        column: x => x.FarmerId,
                        principalTable: "Farmers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SegmentationSimulationFarmers_SegmentationSegments_SegmentationConfigurationSegmentId",
                        column: x => x.SegmentationConfigurationSegmentId,
                        principalTable: "SegmentationSegments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SegmentationSimulationFarmers_SegmentationSimulations_SegmentationSimulationId",
                        column: x => x.SegmentationSimulationId,
                        principalTable: "SegmentationSimulations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FinancialSelfFundingRangeSkippedCropSeasons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FinancialSelfFundingRangeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CropSeasonId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialSelfFundingRangeSkippedCropSeasons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialSelfFundingRangeSkippedCropSeasons_CropSeasons_CropSeasonId",
                        column: x => x.CropSeasonId,
                        principalTable: "CropSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialSelfFundingRangeSkippedCropSeasons_FinancialSelfFundingRanges_FinancialSelfFundingRangeId",
                        column: x => x.FinancialSelfFundingRangeId,
                        principalTable: "FinancialSelfFundingRanges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoyaltySeasonQuantityRangeSkippedCropSeasons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LoyaltySeasonQuantityRangeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CropSeasonId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltySeasonQuantityRangeSkippedCropSeasons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltySeasonQuantityRangeSkippedCropSeasons_CropSeasons_CropSeasonId",
                        column: x => x.CropSeasonId,
                        principalTable: "CropSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LoyaltySeasonQuantityRangeSkippedCropSeasons_LoyaltySeasonQuantityRanges_LoyaltySeasonQuantityRangeId",
                        column: x => x.LoyaltySeasonQuantityRangeId,
                        principalTable: "LoyaltySeasonQuantityRanges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QualityIqsRangeSkippedCropSeasons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    QualityIqsRangeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CropSeasonId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityIqsRangeSkippedCropSeasons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QualityIqsRangeSkippedCropSeasons_CropSeasons_CropSeasonId",
                        column: x => x.CropSeasonId,
                        principalTable: "CropSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QualityIqsRangeSkippedCropSeasons_QualityIqsRanges_QualityIqsRangeId",
                        column: x => x.QualityIqsRangeId,
                        principalTable: "QualityIqsRanges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScaleRangeSkippedCropSeasons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ScaleRangeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CropSeasonId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScaleRangeSkippedCropSeasons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScaleRangeSkippedCropSeasons_CropSeasons_CropSeasonId",
                        column: x => x.CropSeasonId,
                        principalTable: "CropSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ScaleRangeSkippedCropSeasons_ScaleRanges_ScaleRangeId",
                        column: x => x.ScaleRangeId,
                        principalTable: "ScaleRanges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YieldRangeSkippedCropSeasons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    YieldRangeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CropSeasonId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YieldRangeSkippedCropSeasons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YieldRangeSkippedCropSeasons_CropSeasons_CropSeasonId",
                        column: x => x.CropSeasonId,
                        principalTable: "CropSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_YieldRangeSkippedCropSeasons_YieldRanges_YieldRangeId",
                        column: x => x.YieldRangeId,
                        principalTable: "YieldRanges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EsgKpis_CropSeasonId",
                table: "EsgKpis",
                column: "CropSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_EsgKpis_FarmerId_CropSeasonId",
                table: "EsgKpis",
                columns: new[] { "FarmerId", "CropSeasonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FarmerClusterFarmers_FarmerId",
                table: "FarmerClusterFarmers",
                column: "FarmerId");

            migrationBuilder.CreateIndex(
                name: "IX_FarmerClusters_Code",
                table: "FarmerClusters",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Farmers_Code",
                table: "Farmers",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FarmerSegmentations_CropSeasonId",
                table: "FarmerSegmentations",
                column: "CropSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_FarmerSegmentations_FarmerId_CropSeasonId",
                table: "FarmerSegmentations",
                columns: new[] { "FarmerId", "CropSeasonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FarmerSegmentations_SegmentationConfigurationSegmentId",
                table: "FarmerSegmentations",
                column: "SegmentationConfigurationSegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialKpis_CropSeasonId",
                table: "FinancialKpis",
                column: "CropSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialKpis_FarmerId_CropSeasonId",
                table: "FinancialKpis",
                columns: new[] { "FarmerId", "CropSeasonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialSelfFundingRanges_SegmentationConfigurationId",
                table: "FinancialSelfFundingRanges",
                column: "SegmentationConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialSelfFundingRangeSkippedCropSeasons_CropSeasonId",
                table: "FinancialSelfFundingRangeSkippedCropSeasons",
                column: "CropSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialSelfFundingRangeSkippedCropSeasons_FinancialSelfFundingRangeId",
                table: "FinancialSelfFundingRangeSkippedCropSeasons",
                column: "FinancialSelfFundingRangeId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyHistoricalVolumeRanges_SegmentationConfigurationId",
                table: "LoyaltyHistoricalVolumeRanges",
                column: "SegmentationConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyKpis_CropSeasonId",
                table: "LoyaltyKpis",
                column: "CropSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyKpis_FarmerId_CropSeasonId",
                table: "LoyaltyKpis",
                columns: new[] { "FarmerId", "CropSeasonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltySeasonQuantityRanges_SegmentationConfigurationId",
                table: "LoyaltySeasonQuantityRanges",
                column: "SegmentationConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltySeasonQuantityRangeSkippedCropSeasons_CropSeasonId",
                table: "LoyaltySeasonQuantityRangeSkippedCropSeasons",
                column: "CropSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltySeasonQuantityRangeSkippedCropSeasons_LoyaltySeasonQuantityRangeId",
                table: "LoyaltySeasonQuantityRangeSkippedCropSeasons",
                column: "LoyaltySeasonQuantityRangeId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityIqsRanges_SegmentationConfigurationId",
                table: "QualityIqsRanges",
                column: "SegmentationConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityIqsRangeSkippedCropSeasons_CropSeasonId",
                table: "QualityIqsRangeSkippedCropSeasons",
                column: "CropSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityIqsRangeSkippedCropSeasons_QualityIqsRangeId",
                table: "QualityIqsRangeSkippedCropSeasons",
                column: "QualityIqsRangeId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityKpis_CropSeasonId",
                table: "QualityKpis",
                column: "CropSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityKpis_FarmerId_CropSeasonId",
                table: "QualityKpis",
                columns: new[] { "FarmerId", "CropSeasonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScaleKpis_CropSeasonId",
                table: "ScaleKpis",
                column: "CropSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_ScaleKpis_FarmerId_CropSeasonId",
                table: "ScaleKpis",
                columns: new[] { "FarmerId", "CropSeasonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScaleRanges_SegmentationConfigurationId",
                table: "ScaleRanges",
                column: "SegmentationConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_ScaleRangeSkippedCropSeasons_CropSeasonId",
                table: "ScaleRangeSkippedCropSeasons",
                column: "CropSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_ScaleRangeSkippedCropSeasons_ScaleRangeId",
                table: "ScaleRangeSkippedCropSeasons",
                column: "ScaleRangeId");

            migrationBuilder.CreateIndex(
                name: "IX_SegmentationSegments_SegmentationConfigurationId",
                table: "SegmentationSegments",
                column: "SegmentationConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_SegmentationSimulationFarmers_FarmerId",
                table: "SegmentationSimulationFarmers",
                column: "FarmerId");

            migrationBuilder.CreateIndex(
                name: "IX_SegmentationSimulationFarmers_SegmentationConfigurationSegmentId",
                table: "SegmentationSimulationFarmers",
                column: "SegmentationConfigurationSegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_SegmentationSimulationFarmers_SegmentationSimulationId_FarmerId",
                table: "SegmentationSimulationFarmers",
                columns: new[] { "SegmentationSimulationId", "FarmerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SegmentationSimulations_CropSeasonId",
                table: "SegmentationSimulations",
                column: "CropSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_SegmentationSimulations_SegmentationConfigurationId",
                table: "SegmentationSimulations",
                column: "SegmentationConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_TechnologiesKpis_CropSeasonId",
                table: "TechnologiesKpis",
                column: "CropSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_TechnologiesKpis_FarmerId_CropSeasonId",
                table: "TechnologiesKpis",
                columns: new[] { "FarmerId", "CropSeasonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_YieldKpis_CropSeasonId",
                table: "YieldKpis",
                column: "CropSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_YieldKpis_FarmerId_CropSeasonId",
                table: "YieldKpis",
                columns: new[] { "FarmerId", "CropSeasonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_YieldRanges_SegmentationConfigurationId",
                table: "YieldRanges",
                column: "SegmentationConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_YieldRangeSkippedCropSeasons_CropSeasonId",
                table: "YieldRangeSkippedCropSeasons",
                column: "CropSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_YieldRangeSkippedCropSeasons_YieldRangeId",
                table: "YieldRangeSkippedCropSeasons",
                column: "YieldRangeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EsgKpis");

            migrationBuilder.DropTable(
                name: "FarmerClusterFarmers");

            migrationBuilder.DropTable(
                name: "FarmerSegmentations");

            migrationBuilder.DropTable(
                name: "FinancialKpis");

            migrationBuilder.DropTable(
                name: "FinancialSelfFundingRangeSkippedCropSeasons");

            migrationBuilder.DropTable(
                name: "LoyaltyHistoricalVolumeRanges");

            migrationBuilder.DropTable(
                name: "LoyaltyKpis");

            migrationBuilder.DropTable(
                name: "LoyaltySeasonQuantityRangeSkippedCropSeasons");

            migrationBuilder.DropTable(
                name: "QualityIqsRangeSkippedCropSeasons");

            migrationBuilder.DropTable(
                name: "QualityKpis");

            migrationBuilder.DropTable(
                name: "ScaleKpis");

            migrationBuilder.DropTable(
                name: "ScaleRangeSkippedCropSeasons");

            migrationBuilder.DropTable(
                name: "SegmentationConfigurationEsgs");

            migrationBuilder.DropTable(
                name: "SegmentationConfigurationTechnologies");

            migrationBuilder.DropTable(
                name: "SegmentationSimulationFarmers");

            migrationBuilder.DropTable(
                name: "TechnologiesKpis");

            migrationBuilder.DropTable(
                name: "YieldKpis");

            migrationBuilder.DropTable(
                name: "YieldRangeSkippedCropSeasons");

            migrationBuilder.DropTable(
                name: "FarmerClusters");

            migrationBuilder.DropTable(
                name: "FinancialSelfFundingRanges");

            migrationBuilder.DropTable(
                name: "LoyaltySeasonQuantityRanges");

            migrationBuilder.DropTable(
                name: "QualityIqsRanges");

            migrationBuilder.DropTable(
                name: "ScaleRanges");

            migrationBuilder.DropTable(
                name: "SegmentationSegments");

            migrationBuilder.DropTable(
                name: "SegmentationSimulations");

            migrationBuilder.DropTable(
                name: "Farmers");

            migrationBuilder.DropTable(
                name: "YieldRanges");

            migrationBuilder.DropTable(
                name: "SegmentationConfigurationFinancials");

            migrationBuilder.DropTable(
                name: "SegmentationConfigurationLoyalties");

            migrationBuilder.DropTable(
                name: "SegmentationConfigurationQualities");

            migrationBuilder.DropTable(
                name: "SegmentationConfigurationScales");

            migrationBuilder.DropTable(
                name: "CropSeasons");

            migrationBuilder.DropTable(
                name: "SegmentationConfigurationYields");

            migrationBuilder.DropTable(
                name: "SegmentationConfigurations");
        }
    }
}
