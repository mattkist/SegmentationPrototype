using Microsoft.EntityFrameworkCore;
using Segmentation.Domain.Entities;

namespace Segmentation.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<CultureType> CultureTypes => Set<CultureType>();
    public DbSet<CropSeason> CropSeasons => Set<CropSeason>();
    public DbSet<Farmer> Farmers => Set<Farmer>();
    public DbSet<FarmerCluster> FarmerClusters => Set<FarmerCluster>();
    public DbSet<FarmerClusterFarmer> FarmerClusterFarmers => Set<FarmerClusterFarmer>();
    public DbSet<FarmerContractKpi> FarmerContractKpis => Set<FarmerContractKpi>();
    public DbSet<TechnologyCatalog> Technologies => Set<TechnologyCatalog>();
    public DbSet<IrregularityTypeCatalog> IrregularityTypes => Set<IrregularityTypeCatalog>();
    public DbSet<TechnologiesKpi> TechnologiesKpis => Set<TechnologiesKpi>();
    public DbSet<EsgIrregularityKpi> EsgIrregularityKpis => Set<EsgIrregularityKpi>();
    public DbSet<SegmentationConfiguration> SegmentationConfigurations => Set<SegmentationConfiguration>();
    public DbSet<SegmentationSegment> SegmentationSegments => Set<SegmentationSegment>();
    public DbSet<SegmentationConfigurationCultureType> SegmentationConfigurationCultureTypes => Set<SegmentationConfigurationCultureType>();
    public DbSet<SegmentationConfigurationCultureTypeSegment> SegmentationConfigurationCultureTypeSegments =>
        Set<SegmentationConfigurationCultureTypeSegment>();
    public DbSet<SegmentationConfigurationLoyalty> SegmentationConfigurationLoyalties => Set<SegmentationConfigurationLoyalty>();
    public DbSet<LoyaltySeasonQuantityRange> LoyaltySeasonQuantityRanges => Set<LoyaltySeasonQuantityRange>();
    public DbSet<LoyaltyHistoricalVolumeRange> LoyaltyHistoricalVolumeRanges => Set<LoyaltyHistoricalVolumeRange>();
    public DbSet<SegmentationConfigurationQuality> SegmentationConfigurationQualities => Set<SegmentationConfigurationQuality>();
    public DbSet<QualityIqsRange> QualityIqsRanges => Set<QualityIqsRange>();
    public DbSet<SegmentationConfigurationFinancial> SegmentationConfigurationFinancials => Set<SegmentationConfigurationFinancial>();
    public DbSet<FinancialSelfFundingRange> FinancialSelfFundingRanges => Set<FinancialSelfFundingRange>();
    public DbSet<SegmentationConfigurationTechnology> SegmentationConfigurationTechnologies => Set<SegmentationConfigurationTechnology>();
    public DbSet<SegmentationConfigurationTechnologyScore> SegmentationConfigurationTechnologyScores =>
        Set<SegmentationConfigurationTechnologyScore>();
    public DbSet<SegmentationConfigurationEsg> SegmentationConfigurationEsgs => Set<SegmentationConfigurationEsg>();
    public DbSet<SegmentationConfigurationEsgIrregularityScore> SegmentationConfigurationEsgIrregularityScores =>
        Set<SegmentationConfigurationEsgIrregularityScore>();
    public DbSet<SegmentationConfigurationYield> SegmentationConfigurationYields => Set<SegmentationConfigurationYield>();
    public DbSet<YieldRange> YieldRanges => Set<YieldRange>();
    public DbSet<SegmentationConfigurationScale> SegmentationConfigurationScales => Set<SegmentationConfigurationScale>();
    public DbSet<ScaleRange> ScaleRanges => Set<ScaleRange>();
    public DbSet<SegmentationSimulation> SegmentationSimulations => Set<SegmentationSimulation>();
    public DbSet<SegmentationSimulationKpiScope> SegmentationSimulationKpiScopes => Set<SegmentationSimulationKpiScope>();
    public DbSet<SegmentationSimulationKpiScopeSeason> SegmentationSimulationKpiScopeSeasons =>
        Set<SegmentationSimulationKpiScopeSeason>();
    public DbSet<SegmentationSimulationFarmer> SegmentationSimulationFarmers => Set<SegmentationSimulationFarmer>();
    public DbSet<FarmerSegmentation> FarmerSegmentations => Set<FarmerSegmentation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CultureType>(e =>
        {
            e.ToTable("CultureTypes");
            e.HasKey(x => x.Code);
            e.Property(x => x.Code).HasMaxLength(8).IsRequired();
            e.Property(x => x.Name).HasMaxLength(64).IsRequired();
        });

        modelBuilder.Entity<CropSeason>(e =>
        {
            e.ToTable("CropSeasons");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Code).HasMaxLength(16).IsRequired();
        });

        modelBuilder.Entity<Farmer>(e =>
        {
            e.ToTable("Farmers");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(32).IsRequired();
            e.Property(x => x.Name).HasMaxLength(256).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<FarmerCluster>(e =>
        {
            e.ToTable("FarmerClusters");
            e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(32).IsRequired();
            e.Property(x => x.Name).HasMaxLength(256).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<FarmerClusterFarmer>(e =>
        {
            e.ToTable("FarmerClusterFarmers");
            e.HasKey(x => new { x.ClusterId, x.FarmerId });
            e.HasOne(x => x.Cluster).WithMany(x => x.Farmers).HasForeignKey(x => x.ClusterId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Farmer).WithMany(x => x.ClusterLinks).HasForeignKey(x => x.FarmerId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FarmerContractKpi>(e =>
        {
            e.ToTable("FarmerContractKpis");
            e.HasKey(x => new { x.FarmerId, x.CropSeasonId, x.CultureTypeCode });
            e.Property(x => x.CultureTypeCode).HasMaxLength(8).IsRequired();
            e.HasOne(x => x.Farmer).WithMany(x => x.ContractKpis).HasForeignKey(x => x.FarmerId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.CropSeason).WithMany().HasForeignKey(x => x.CropSeasonId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.CultureType).WithMany().HasForeignKey(x => x.CultureTypeCode).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TechnologyCatalog>(e =>
        {
            e.ToTable("Technologies");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Name).HasMaxLength(256).IsRequired();
        });

        modelBuilder.Entity<IrregularityTypeCatalog>(e =>
        {
            e.ToTable("IrregularityTypes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Name).HasMaxLength(256).IsRequired();
        });

        modelBuilder.Entity<TechnologiesKpi>(e =>
        {
            e.ToTable("TechnologiesKpis");
            e.HasKey(x => x.Id);
            e.Property(x => x.CultureTypeCode).HasMaxLength(8).IsRequired();
            e.HasOne(x => x.Farmer).WithMany(x => x.TechnologiesKpis).HasForeignKey(x => x.FarmerId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.CropSeason).WithMany().HasForeignKey(x => x.CropSeasonId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.CultureType).WithMany().HasForeignKey(x => x.CultureTypeCode).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Technology).WithMany(x => x.TechnologiesKpis).HasForeignKey(x => x.TechnologyId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.FarmerId, x.CropSeasonId, x.CultureTypeCode, x.TechnologyId }).IsUnique();
        });

        modelBuilder.Entity<EsgIrregularityKpi>(e =>
        {
            e.ToTable("EsgIrregularityKpis");
            e.HasKey(x => x.Id);
            e.Property(x => x.CultureTypeCode).HasMaxLength(8).IsRequired();
            e.HasOne(x => x.Farmer).WithMany(x => x.EsgIrregularityKpis).HasForeignKey(x => x.FarmerId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.CropSeason).WithMany().HasForeignKey(x => x.CropSeasonId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.CultureType).WithMany().HasForeignKey(x => x.CultureTypeCode).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.IrregularityType).WithMany(x => x.EsgIrregularityKpis).HasForeignKey(x => x.IrregularityTypeId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.FarmerId, x.CropSeasonId, x.CultureTypeCode, x.IrregularityTypeId }).IsUnique();
        });

        modelBuilder.Entity<SegmentationConfiguration>(e =>
        {
            e.ToTable("SegmentationConfigurations");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(256).IsRequired();
            e.HasMany(x => x.Segments).WithOne(x => x.SegmentationConfiguration).HasForeignKey(x => x.SegmentationConfigurationId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.CultureTypes).WithOne(x => x.SegmentationConfiguration).HasForeignKey(x => x.SegmentationConfigurationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SegmentationSegment>(e =>
        {
            e.ToTable("SegmentationSegments");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.SegmentName).HasMaxLength(128).IsRequired();
        });

        modelBuilder.Entity<SegmentationConfigurationCultureType>(e =>
        {
            e.ToTable("SegmentationConfigurationCultureTypes");
            e.HasKey(x => x.Id);
            e.Property(x => x.CultureTypeCode).HasMaxLength(8).IsRequired();
            e.HasOne(x => x.CultureType).WithMany().HasForeignKey(x => x.CultureTypeCode).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.SegmentationConfigurationId, x.CultureTypeCode }).IsUnique();
            e.HasMany(x => x.CultureTypeSegments).WithOne(x => x.CultureType).HasForeignKey(x => x.SegmentationConfigurationCultureTypeId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Loyalty).WithOne(x => x.CultureType).HasForeignKey<SegmentationConfigurationLoyalty>(x => x.SegmentationConfigurationCultureTypeId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Quality).WithOne(x => x.CultureType).HasForeignKey<SegmentationConfigurationQuality>(x => x.SegmentationConfigurationCultureTypeId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Financial).WithOne(x => x.CultureType).HasForeignKey<SegmentationConfigurationFinancial>(x => x.SegmentationConfigurationCultureTypeId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Technology).WithOne(x => x.CultureType).HasForeignKey<SegmentationConfigurationTechnology>(x => x.SegmentationConfigurationCultureTypeId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Esg).WithOne(x => x.CultureType).HasForeignKey<SegmentationConfigurationEsg>(x => x.SegmentationConfigurationCultureTypeId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Yield).WithOne(x => x.CultureType).HasForeignKey<SegmentationConfigurationYield>(x => x.SegmentationConfigurationCultureTypeId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Scale).WithOne(x => x.CultureType).HasForeignKey<SegmentationConfigurationScale>(x => x.SegmentationConfigurationCultureTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SegmentationConfigurationCultureTypeSegment>(e =>
        {
            e.ToTable("SegmentationConfigurationCultureTypeSegments");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.SegmentationConfigurationCultureTypeId, x.SegmentationSegmentId }).IsUnique();
            e.HasOne(x => x.Segment).WithMany(x => x.CultureTypeSegments).HasForeignKey(x => x.SegmentationSegmentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SegmentationConfigurationLoyalty>(e =>
        {
            e.ToTable("SegmentationConfigurationLoyalties");
            e.HasKey(x => x.SegmentationConfigurationCultureTypeId);
            e.Property(x => x.Relevance).HasPrecision(9, 4);
            e.HasMany(x => x.SeasonQuantityRanges).WithOne(x => x.Loyalty).HasForeignKey(x => x.SegmentationConfigurationCultureTypeId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.HistoricalVolumeRanges).WithOne(x => x.Loyalty).HasForeignKey(x => x.SegmentationConfigurationCultureTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LoyaltySeasonQuantityRange>(e =>
        {
            e.ToTable("LoyaltySeasonQuantityRanges");
            e.HasKey(x => x.Id);
        });

        modelBuilder.Entity<LoyaltyHistoricalVolumeRange>(e =>
        {
            e.ToTable("LoyaltyHistoricalVolumeRanges");
            e.HasKey(x => x.Id);
        });

        modelBuilder.Entity<SegmentationConfigurationQuality>(e =>
        {
            e.ToTable("SegmentationConfigurationQualities");
            e.HasKey(x => x.SegmentationConfigurationCultureTypeId);
            e.Property(x => x.Relevance).HasPrecision(9, 4);
            e.HasMany(x => x.IqsRanges).WithOne(x => x.Quality).HasForeignKey(x => x.SegmentationConfigurationCultureTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QualityIqsRange>(e =>
        {
            e.ToTable("QualityIqsRanges");
            e.HasKey(x => x.Id);
        });

        modelBuilder.Entity<SegmentationConfigurationFinancial>(e =>
        {
            e.ToTable("SegmentationConfigurationFinancials");
            e.HasKey(x => x.SegmentationConfigurationCultureTypeId);
            e.Property(x => x.Relevance).HasPrecision(9, 4);
            e.HasMany(x => x.SelfFundingRanges).WithOne(x => x.Financial).HasForeignKey(x => x.SegmentationConfigurationCultureTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FinancialSelfFundingRange>(e =>
        {
            e.ToTable("FinancialSelfFundingRanges");
            e.HasKey(x => x.Id);
        });

        modelBuilder.Entity<SegmentationConfigurationTechnology>(e =>
        {
            e.ToTable("SegmentationConfigurationTechnologies");
            e.HasKey(x => x.SegmentationConfigurationCultureTypeId);
            e.Property(x => x.Relevance).HasPrecision(9, 4);
            e.HasMany(x => x.TechnologyScores).WithOne(x => x.TechnologyBlock)
                .HasForeignKey(x => x.SegmentationConfigurationCultureTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SegmentationConfigurationTechnologyScore>(e =>
        {
            e.ToTable("SegmentationConfigurationTechnologyScores");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Technology).WithMany(x => x.ConfigurationScores).HasForeignKey(x => x.TechnologyId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.SegmentationConfigurationCultureTypeId, x.TechnologyId }).IsUnique();
        });

        modelBuilder.Entity<SegmentationConfigurationEsg>(e =>
        {
            e.ToTable("SegmentationConfigurationEsgs");
            e.HasKey(x => x.SegmentationConfigurationCultureTypeId);
            e.Property(x => x.Relevance).HasPrecision(9, 4);
            e.HasMany(x => x.IrregularityScores).WithOne(x => x.EsgBlock)
                .HasForeignKey(x => x.SegmentationConfigurationCultureTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SegmentationConfigurationEsgIrregularityScore>(e =>
        {
            e.ToTable("SegmentationConfigurationEsgIrregularityScores");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.IrregularityType).WithMany(x => x.ConfigurationScores).HasForeignKey(x => x.IrregularityTypeId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.SegmentationConfigurationCultureTypeId, x.IrregularityTypeId }).IsUnique();
        });

        modelBuilder.Entity<SegmentationConfigurationYield>(e =>
        {
            e.ToTable("SegmentationConfigurationYields");
            e.HasKey(x => x.SegmentationConfigurationCultureTypeId);
            e.Property(x => x.Relevance).HasPrecision(9, 4);
            e.HasMany(x => x.Ranges).WithOne(x => x.Yield).HasForeignKey(x => x.SegmentationConfigurationCultureTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<YieldRange>(e =>
        {
            e.ToTable("YieldRanges");
            e.HasKey(x => x.Id);
        });

        modelBuilder.Entity<SegmentationConfigurationScale>(e =>
        {
            e.ToTable("SegmentationConfigurationScales");
            e.HasKey(x => x.SegmentationConfigurationCultureTypeId);
            e.Property(x => x.Relevance).HasPrecision(9, 4);
            e.HasMany(x => x.Ranges).WithOne(x => x.Scale).HasForeignKey(x => x.SegmentationConfigurationCultureTypeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ScaleRange>(e =>
        {
            e.ToTable("ScaleRanges");
            e.HasKey(x => x.Id);
        });

        modelBuilder.Entity<SegmentationSimulation>(e =>
        {
            e.ToTable("SegmentationSimulations");
            e.HasKey(x => x.Id);
            e.Property(x => x.Status).HasMaxLength(1).IsRequired();
            e.HasOne(x => x.SegmentationConfiguration).WithMany(x => x.Simulations).HasForeignKey(x => x.SegmentationConfigurationId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.CropSeason).WithMany().HasForeignKey(x => x.CropSeasonId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.KpiScopes).WithOne(x => x.SegmentationSimulation).HasForeignKey(x => x.SegmentationSimulationId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Farmers).WithOne(x => x.SegmentationSimulation).HasForeignKey(x => x.SegmentationSimulationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SegmentationSimulationKpiScope>(e =>
        {
            e.ToTable("SegmentationSimulationKpiScopes");
            e.HasKey(x => x.Id);
            e.Property(x => x.KpiKind).HasMaxLength(32).IsRequired();
            e.Property(x => x.ValueAggregation).HasMaxLength(32);
            e.HasIndex(x => new { x.SegmentationSimulationId, x.KpiKind }).IsUnique();
            e.HasMany(x => x.Seasons).WithOne(x => x.KpiScope).HasForeignKey(x => x.SegmentationSimulationKpiScopeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SegmentationSimulationKpiScopeSeason>(e =>
        {
            e.ToTable("SegmentationSimulationKpiScopeSeasons");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.SegmentationSimulationKpiScopeId, x.CropSeasonId }).IsUnique();
            e.HasOne(x => x.CropSeason).WithMany().HasForeignKey(x => x.CropSeasonId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SegmentationSimulationFarmer>(e =>
        {
            e.ToTable("SegmentationSimulationFarmers");
            e.HasKey(x => x.Id);
            e.Property(x => x.CultureTypeCode).HasMaxLength(8).IsRequired();
            e.HasIndex(x => new { x.SegmentationSimulationId, x.FarmerId }).IsUnique();
            e.HasOne(x => x.Farmer).WithMany(x => x.SimulationResults).HasForeignKey(x => x.FarmerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Segment).WithMany(x => x.SimulationFarmers).HasForeignKey(x => x.SegmentationConfigurationSegmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FarmerSegmentation>(e =>
        {
            e.ToTable("FarmerSegmentations");
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.FarmerId, x.CropSeasonId }).IsUnique();
            e.HasOne(x => x.Farmer).WithMany(x => x.Segmentations).HasForeignKey(x => x.FarmerId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.CropSeason).WithMany().HasForeignKey(x => x.CropSeasonId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.SegmentationConfiguration).WithMany().HasForeignKey(x => x.SegmentationConfigurationId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Segment).WithMany(x => x.FarmerSegmentations).HasForeignKey(x => x.SegmentationConfigurationSegmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
