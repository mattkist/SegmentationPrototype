using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Segmentation.Infrastructure.Migrations
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
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CropSeasons", x => x.Id);
                });

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
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false)
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
                    CultureTypeCode = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
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
                        name: "FK_EsgKpis_CultureTypes_CultureTypeCode",
                        column: x => x.CultureTypeCode,
                        principalTable: "CultureTypes",
                        principalColumn: "Code",
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
                    CultureTypeCode = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
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
                        name: "FK_FinancialKpis_CultureTypes_CultureTypeCode",
                        column: x => x.CultureTypeCode,
                        principalTable: "CultureTypes",
                        principalColumn: "Code",
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
                    CultureTypeCode = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
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
                        name: "FK_LoyaltyKpis_CultureTypes_CultureTypeCode",
                        column: x => x.CultureTypeCode,
                        principalTable: "CultureTypes",
                        principalColumn: "Code",
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
                    CultureTypeCode = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
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
                        name: "FK_QualityKpis_CultureTypes_CultureTypeCode",
                        column: x => x.CultureTypeCode,
                        principalTable: "CultureTypes",
                        principalColumn: "Code",
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
                    CultureTypeCode = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
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
                name: "TechnologiesKpis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FarmerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CropSeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    CultureTypeCode = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
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
                });

            migrationBuilder.CreateTable(
                name: "YieldKpis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FarmerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CropSeasonId = table.Column<int>(type: "INTEGER", nullable: false),
                    CultureTypeCode = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
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

            migrationBuilder.CreateTable(
                name: "SegmentationConfigurationCultureTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SegmentationConfigurationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CultureTypeCode = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    MaximumScore = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentationConfigurationCultureTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SegmentationConfigurationCultureTypes_CultureTypes_CultureTypeCode",
                        column: x => x.CultureTypeCode,
                        principalTable: "CultureTypes",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SegmentationConfigurationCultureTypes_SegmentationConfigurations_SegmentationConfigurationId",
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
                name: "SegmentationConfigurationEsgs",
                columns: table => new
                {
                    SegmentationConfigurationCultureTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MaxScore = table.Column<int>(type: "INTEGER", nullable: false),
                    Relevance = table.Column<decimal>(type: "TEXT", precision: 9, scale: 4, nullable: false),
                    ReforestationScorePerPercentualPoint = table.Column<int>(type: "INTEGER", nullable: false),
                    ReforestationMaximumScore = table.Column<int>(type: "INTEGER", nullable: false),
                    NativeForestScorePerPercentualPoint = table.Column<int>(type: "INTEGER", nullable: false),
                    NativeForestMaximumScore = table.Column<int>(type: "INTEGER", nullable: false),
                    MinorIrregularityScore = table.Column<int>(type: "INTEGER", nullable: false),
                    MajorIrregularityScore = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentationConfigurationEsgs", x => x.SegmentationConfigurationCultureTypeId);
                    table.ForeignKey(
                        name: "FK_SegmentationConfigurationEsgs_SegmentationConfigurationCultureTypes_SegmentationConfigurationCultureTypeId",
                        column: x => x.SegmentationConfigurationCultureTypeId,
                        principalTable: "SegmentationConfigurationCultureTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SegmentationConfigurationFinancials",
                columns: table => new
                {
                    SegmentationConfigurationCultureTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MaxScore = table.Column<int>(type: "INTEGER", nullable: false),
                    Relevance = table.Column<decimal>(type: "TEXT", precision: 9, scale: 4, nullable: false),
                    DebtScore = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentationConfigurationFinancials", x => x.SegmentationConfigurationCultureTypeId);
                    table.ForeignKey(
                        name: "FK_SegmentationConfigurationFinancials_SegmentationConfigurationCultureTypes_SegmentationConfigurationCultureTypeId",
                        column: x => x.SegmentationConfigurationCultureTypeId,
                        principalTable: "SegmentationConfigurationCultureTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SegmentationConfigurationLoyalties",
                columns: table => new
                {
                    SegmentationConfigurationCultureTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MaxScore = table.Column<int>(type: "INTEGER", nullable: false),
                    Relevance = table.Column<decimal>(type: "TEXT", precision: 9, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentationConfigurationLoyalties", x => x.SegmentationConfigurationCultureTypeId);
                    table.ForeignKey(
                        name: "FK_SegmentationConfigurationLoyalties_SegmentationConfigurationCultureTypes_SegmentationConfigurationCultureTypeId",
                        column: x => x.SegmentationConfigurationCultureTypeId,
                        principalTable: "SegmentationConfigurationCultureTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SegmentationConfigurationQualities",
                columns: table => new
                {
                    SegmentationConfigurationCultureTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MaxScore = table.Column<int>(type: "INTEGER", nullable: false),
                    Relevance = table.Column<decimal>(type: "TEXT", precision: 9, scale: 4, nullable: false),
                    NtrmScore = table.Column<int>(type: "INTEGER", nullable: false),
                    MixtureScore = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentationConfigurationQualities", x => x.SegmentationConfigurationCultureTypeId);
                    table.ForeignKey(
                        name: "FK_SegmentationConfigurationQualities_SegmentationConfigurationCultureTypes_SegmentationConfigurationCultureTypeId",
                        column: x => x.SegmentationConfigurationCultureTypeId,
                        principalTable: "SegmentationConfigurationCultureTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SegmentationConfigurationScales",
                columns: table => new
                {
                    SegmentationConfigurationCultureTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MaxScore = table.Column<int>(type: "INTEGER", nullable: false),
                    Relevance = table.Column<decimal>(type: "TEXT", precision: 9, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentationConfigurationScales", x => x.SegmentationConfigurationCultureTypeId);
                    table.ForeignKey(
                        name: "FK_SegmentationConfigurationScales_SegmentationConfigurationCultureTypes_SegmentationConfigurationCultureTypeId",
                        column: x => x.SegmentationConfigurationCultureTypeId,
                        principalTable: "SegmentationConfigurationCultureTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SegmentationConfigurationTechnologies",
                columns: table => new
                {
                    SegmentationConfigurationCultureTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MaxScore = table.Column<int>(type: "INTEGER", nullable: false),
                    Relevance = table.Column<decimal>(type: "TEXT", precision: 9, scale: 4, nullable: false),
                    HasLargeBaseRidgeWithMulchScore = table.Column<int>(type: "INTEGER", nullable: false),
                    HasBroadGrateFurnaceScore = table.Column<int>(type: "INTEGER", nullable: false),
                    HasTechnologyPackageAdherenceScore = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentationConfigurationTechnologies", x => x.SegmentationConfigurationCultureTypeId);
                    table.ForeignKey(
                        name: "FK_SegmentationConfigurationTechnologies_SegmentationConfigurationCultureTypes_SegmentationConfigurationCultureTypeId",
                        column: x => x.SegmentationConfigurationCultureTypeId,
                        principalTable: "SegmentationConfigurationCultureTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SegmentationConfigurationYields",
                columns: table => new
                {
                    SegmentationConfigurationCultureTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MaxScore = table.Column<int>(type: "INTEGER", nullable: false),
                    Relevance = table.Column<decimal>(type: "TEXT", precision: 9, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentationConfigurationYields", x => x.SegmentationConfigurationCultureTypeId);
                    table.ForeignKey(
                        name: "FK_SegmentationConfigurationYields_SegmentationConfigurationCultureTypes_SegmentationConfigurationCultureTypeId",
                        column: x => x.SegmentationConfigurationCultureTypeId,
                        principalTable: "SegmentationConfigurationCultureTypes",
                        principalColumn: "Id",
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
                name: "SegmentationConfigurationCultureTypeSegments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SegmentationConfigurationCultureTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SegmentationSegmentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RangeMin = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentationConfigurationCultureTypeSegments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SegmentationConfigurationCultureTypeSegments_SegmentationConfigurationCultureTypes_SegmentationConfigurationCultureTypeId",
                        column: x => x.SegmentationConfigurationCultureTypeId,
                        principalTable: "SegmentationConfigurationCultureTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SegmentationConfigurationCultureTypeSegments_SegmentationSegments_SegmentationSegmentId",
                        column: x => x.SegmentationSegmentId,
                        principalTable: "SegmentationSegments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SegmentationSimulationCropSeasons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SegmentationSimulationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CropSeasonId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentationSimulationCropSeasons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SegmentationSimulationCropSeasons_CropSeasons_CropSeasonId",
                        column: x => x.CropSeasonId,
                        principalTable: "CropSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SegmentationSimulationCropSeasons_SegmentationSimulations_SegmentationSimulationId",
                        column: x => x.SegmentationSimulationId,
                        principalTable: "SegmentationSimulations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    CultureTypeCode = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
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
                name: "FinancialSelfFundingRanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SegmentationConfigurationCultureTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Minimum = table.Column<int>(type: "INTEGER", nullable: false),
                    Maximum = table.Column<int>(type: "INTEGER", nullable: false),
                    CropSeasonAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialSelfFundingRanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialSelfFundingRanges_SegmentationConfigurationFinancials_SegmentationConfigurationCultureTypeId",
                        column: x => x.SegmentationConfigurationCultureTypeId,
                        principalTable: "SegmentationConfigurationFinancials",
                        principalColumn: "SegmentationConfigurationCultureTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyHistoricalVolumeRanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SegmentationConfigurationCultureTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MinimumDeliveryAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    MaximumDeliveryAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyHistoricalVolumeRanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltyHistoricalVolumeRanges_SegmentationConfigurationLoyalties_SegmentationConfigurationCultureTypeId",
                        column: x => x.SegmentationConfigurationCultureTypeId,
                        principalTable: "SegmentationConfigurationLoyalties",
                        principalColumn: "SegmentationConfigurationCultureTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoyaltySeasonQuantityRanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SegmentationConfigurationCultureTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlantingCropSeasonAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    MinimumDeliveryAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    MaximumDeliveryAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    DeliveryCropSeasonAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltySeasonQuantityRanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoyaltySeasonQuantityRanges_SegmentationConfigurationLoyalties_SegmentationConfigurationCultureTypeId",
                        column: x => x.SegmentationConfigurationCultureTypeId,
                        principalTable: "SegmentationConfigurationLoyalties",
                        principalColumn: "SegmentationConfigurationCultureTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QualityIqsRanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SegmentationConfigurationCultureTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Minimum = table.Column<int>(type: "INTEGER", nullable: false),
                    Maximum = table.Column<int>(type: "INTEGER", nullable: false),
                    CropSeasonAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityIqsRanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QualityIqsRanges_SegmentationConfigurationQualities_SegmentationConfigurationCultureTypeId",
                        column: x => x.SegmentationConfigurationCultureTypeId,
                        principalTable: "SegmentationConfigurationQualities",
                        principalColumn: "SegmentationConfigurationCultureTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScaleRanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SegmentationConfigurationCultureTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Minimum = table.Column<int>(type: "INTEGER", nullable: false),
                    Maximum = table.Column<int>(type: "INTEGER", nullable: false),
                    CropSeasonAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScaleRanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScaleRanges_SegmentationConfigurationScales_SegmentationConfigurationCultureTypeId",
                        column: x => x.SegmentationConfigurationCultureTypeId,
                        principalTable: "SegmentationConfigurationScales",
                        principalColumn: "SegmentationConfigurationCultureTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YieldRanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SegmentationConfigurationCultureTypeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Minimum = table.Column<int>(type: "INTEGER", nullable: false),
                    Maximum = table.Column<int>(type: "INTEGER", nullable: false),
                    CropSeasonAmount = table.Column<int>(type: "INTEGER", nullable: false),
                    Score = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YieldRanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YieldRanges_SegmentationConfigurationYields_SegmentationConfigurationCultureTypeId",
                        column: x => x.SegmentationConfigurationCultureTypeId,
                        principalTable: "SegmentationConfigurationYields",
                        principalColumn: "SegmentationConfigurationCultureTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EsgKpis_CropSeasonId",
                table: "EsgKpis",
                column: "CropSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_EsgKpis_CultureTypeCode",
                table: "EsgKpis",
                column: "CultureTypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_EsgKpis_FarmerId_CropSeasonId_CultureTypeCode",
                table: "EsgKpis",
                columns: new[] { "FarmerId", "CropSeasonId", "CultureTypeCode" },
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
                name: "IX_FinancialKpis_CultureTypeCode",
                table: "FinancialKpis",
                column: "CultureTypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialKpis_FarmerId_CropSeasonId_CultureTypeCode",
                table: "FinancialKpis",
                columns: new[] { "FarmerId", "CropSeasonId", "CultureTypeCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialSelfFundingRanges_SegmentationConfigurationCultureTypeId",
                table: "FinancialSelfFundingRanges",
                column: "SegmentationConfigurationCultureTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyHistoricalVolumeRanges_SegmentationConfigurationCultureTypeId",
                table: "LoyaltyHistoricalVolumeRanges",
                column: "SegmentationConfigurationCultureTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LoyaltyKpis_CropSeasonId",
                table: "LoyaltyKpis",
                column: "CropSeasonId");

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
                name: "IX_LoyaltySeasonQuantityRanges_SegmentationConfigurationCultureTypeId",
                table: "LoyaltySeasonQuantityRanges",
                column: "SegmentationConfigurationCultureTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityIqsRanges_SegmentationConfigurationCultureTypeId",
                table: "QualityIqsRanges",
                column: "SegmentationConfigurationCultureTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityKpis_CropSeasonId",
                table: "QualityKpis",
                column: "CropSeasonId");

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
                name: "IX_ScaleRanges_SegmentationConfigurationCultureTypeId",
                table: "ScaleRanges",
                column: "SegmentationConfigurationCultureTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SegmentationConfigurationCultureTypes_CultureTypeCode",
                table: "SegmentationConfigurationCultureTypes",
                column: "CultureTypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_SegmentationConfigurationCultureTypes_SegmentationConfigurationId_CultureTypeCode",
                table: "SegmentationConfigurationCultureTypes",
                columns: new[] { "SegmentationConfigurationId", "CultureTypeCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SegmentationConfigurationCultureTypeSegments_SegmentationConfigurationCultureTypeId_SegmentationSegmentId",
                table: "SegmentationConfigurationCultureTypeSegments",
                columns: new[] { "SegmentationConfigurationCultureTypeId", "SegmentationSegmentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SegmentationConfigurationCultureTypeSegments_SegmentationSegmentId",
                table: "SegmentationConfigurationCultureTypeSegments",
                column: "SegmentationSegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_SegmentationSegments_SegmentationConfigurationId",
                table: "SegmentationSegments",
                column: "SegmentationConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_SegmentationSimulationCropSeasons_CropSeasonId",
                table: "SegmentationSimulationCropSeasons",
                column: "CropSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_SegmentationSimulationCropSeasons_SegmentationSimulationId_CropSeasonId",
                table: "SegmentationSimulationCropSeasons",
                columns: new[] { "SegmentationSimulationId", "CropSeasonId" },
                unique: true);

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
                name: "IX_TechnologiesKpis_CultureTypeCode",
                table: "TechnologiesKpis",
                column: "CultureTypeCode");

            migrationBuilder.CreateIndex(
                name: "IX_TechnologiesKpis_FarmerId_CropSeasonId_CultureTypeCode",
                table: "TechnologiesKpis",
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

            migrationBuilder.CreateIndex(
                name: "IX_YieldRanges_SegmentationConfigurationCultureTypeId",
                table: "YieldRanges",
                column: "SegmentationConfigurationCultureTypeId");
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
                name: "FinancialSelfFundingRanges");

            migrationBuilder.DropTable(
                name: "LoyaltyHistoricalVolumeRanges");

            migrationBuilder.DropTable(
                name: "LoyaltyKpis");

            migrationBuilder.DropTable(
                name: "LoyaltySeasonQuantityRanges");

            migrationBuilder.DropTable(
                name: "QualityIqsRanges");

            migrationBuilder.DropTable(
                name: "QualityKpis");

            migrationBuilder.DropTable(
                name: "ScaleKpis");

            migrationBuilder.DropTable(
                name: "ScaleRanges");

            migrationBuilder.DropTable(
                name: "SegmentationConfigurationCultureTypeSegments");

            migrationBuilder.DropTable(
                name: "SegmentationConfigurationEsgs");

            migrationBuilder.DropTable(
                name: "SegmentationConfigurationTechnologies");

            migrationBuilder.DropTable(
                name: "SegmentationSimulationCropSeasons");

            migrationBuilder.DropTable(
                name: "SegmentationSimulationFarmers");

            migrationBuilder.DropTable(
                name: "TechnologiesKpis");

            migrationBuilder.DropTable(
                name: "YieldKpis");

            migrationBuilder.DropTable(
                name: "YieldRanges");

            migrationBuilder.DropTable(
                name: "FarmerClusters");

            migrationBuilder.DropTable(
                name: "SegmentationConfigurationFinancials");

            migrationBuilder.DropTable(
                name: "SegmentationConfigurationLoyalties");

            migrationBuilder.DropTable(
                name: "SegmentationConfigurationQualities");

            migrationBuilder.DropTable(
                name: "SegmentationConfigurationScales");

            migrationBuilder.DropTable(
                name: "SegmentationSegments");

            migrationBuilder.DropTable(
                name: "SegmentationSimulations");

            migrationBuilder.DropTable(
                name: "Farmers");

            migrationBuilder.DropTable(
                name: "SegmentationConfigurationYields");

            migrationBuilder.DropTable(
                name: "CropSeasons");

            migrationBuilder.DropTable(
                name: "SegmentationConfigurationCultureTypes");

            migrationBuilder.DropTable(
                name: "CultureTypes");

            migrationBuilder.DropTable(
                name: "SegmentationConfigurations");
        }
    }
}
