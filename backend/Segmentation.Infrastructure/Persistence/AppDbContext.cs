using Microsoft.EntityFrameworkCore;
using Segmentation.Domain.Entities;

namespace Segmentation.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<CropSeason> CropSeasons => Set<CropSeason>();
    public DbSet<Farmer> Farmers => Set<Farmer>();
    public DbSet<FarmerCluster> FarmerClusters => Set<FarmerCluster>();
    public DbSet<FarmerClusterFarmer> FarmerClusterFarmers => Set<FarmerClusterFarmer>();
    public DbSet<LoyaltyKpi> LoyaltyKpis => Set<LoyaltyKpi>();
    public DbSet<QualityKpi> QualityKpis => Set<QualityKpi>();
    public DbSet<FinancialKpi> FinancialKpis => Set<FinancialKpi>();
    public DbSet<YieldKpi> YieldKpis => Set<YieldKpi>();
    public DbSet<ScaleKpi> ScaleKpis => Set<ScaleKpi>();
    public DbSet<TechnologiesKpi> TechnologiesKpis => Set<TechnologiesKpi>();
    public DbSet<EsgKpi> EsgKpis => Set<EsgKpi>();
    public DbSet<SegmentationConfiguration> SegmentationConfigurations => Set<SegmentationConfiguration>();
    public DbSet<SegmentationSegment> SegmentationSegments => Set<SegmentationSegment>();
    public DbSet<SegmentationConfigurationLoyalty> SegmentationConfigurationLoyalties => Set<SegmentationConfigurationLoyalty>();
    public DbSet<LoyaltySeasonQuantityRange> LoyaltySeasonQuantityRanges => Set<LoyaltySeasonQuantityRange>();
    public DbSet<LoyaltySeasonQuantityRangeSkippedCropSeason> LoyaltySeasonQuantityRangeSkippedCropSeasons =>
        Set<LoyaltySeasonQuantityRangeSkippedCropSeason>();
    public DbSet<LoyaltyHistoricalVolumeRange> LoyaltyHistoricalVolumeRanges => Set<LoyaltyHistoricalVolumeRange>();
    public DbSet<SegmentationConfigurationQuality> SegmentationConfigurationQualities => Set<SegmentationConfigurationQuality>();
    public DbSet<QualityIqsRange> QualityIqsRanges => Set<QualityIqsRange>();
    public DbSet<QualityIqsRangeSkippedCropSeason> QualityIqsRangeSkippedCropSeasons => Set<QualityIqsRangeSkippedCropSeason>();
    public DbSet<SegmentationConfigurationFinancial> SegmentationConfigurationFinancials => Set<SegmentationConfigurationFinancial>();
    public DbSet<FinancialSelfFundingRange> FinancialSelfFundingRanges => Set<FinancialSelfFundingRange>();
    public DbSet<FinancialSelfFundingRangeSkippedCropSeason> FinancialSelfFundingRangeSkippedCropSeasons =>
        Set<FinancialSelfFundingRangeSkippedCropSeason>();
    public DbSet<SegmentationConfigurationTechnology> SegmentationConfigurationTechnologies => Set<SegmentationConfigurationTechnology>();
    public DbSet<SegmentationConfigurationEsg> SegmentationConfigurationEsgs => Set<SegmentationConfigurationEsg>();
    public DbSet<SegmentationConfigurationYield> SegmentationConfigurationYields => Set<SegmentationConfigurationYield>();
    public DbSet<YieldRange> YieldRanges => Set<YieldRange>();
    public DbSet<YieldRangeSkippedCropSeason> YieldRangeSkippedCropSeasons => Set<YieldRangeSkippedCropSeason>();
    public DbSet<SegmentationConfigurationScale> SegmentationConfigurationScales => Set<SegmentationConfigurationScale>();
    public DbSet<ScaleRange> ScaleRanges => Set<ScaleRange>();
    public DbSet<ScaleRangeSkippedCropSeason> ScaleRangeSkippedCropSeasons => Set<ScaleRangeSkippedCropSeason>();
    public DbSet<SegmentationSimulation> SegmentationSimulations => Set<SegmentationSimulation>();
    public DbSet<SegmentationSimulationFarmer> SegmentationSimulationFarmers => Set<SegmentationSimulationFarmer>();
    public DbSet<FarmerSegmentation> FarmerSegmentations => Set<FarmerSegmentation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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

        modelBuilder.Entity<LoyaltyKpi>(e =>
        {
            e.ToTable("LoyaltyKpis");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Farmer).WithMany(x => x.LoyaltyKpis).HasForeignKey(x => x.FarmerId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.CropSeason).WithMany().HasForeignKey(x => x.CropSeasonId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.FarmerId, x.CropSeasonId }).IsUnique();
        });
        modelBuilder.Entity<QualityKpi>(e =>
        {
            e.ToTable("QualityKpis");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Farmer).WithMany(x => x.QualityKpis).HasForeignKey(x => x.FarmerId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.CropSeason).WithMany().HasForeignKey(x => x.CropSeasonId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.FarmerId, x.CropSeasonId }).IsUnique();
        });
        modelBuilder.Entity<FinancialKpi>(e =>
        {
            e.ToTable("FinancialKpis");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Farmer).WithMany(x => x.FinancialKpis).HasForeignKey(x => x.FarmerId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.CropSeason).WithMany().HasForeignKey(x => x.CropSeasonId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.FarmerId, x.CropSeasonId }).IsUnique();
        });
        modelBuilder.Entity<YieldKpi>(e =>
        {
            e.ToTable("YieldKpis");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Farmer).WithMany(x => x.YieldKpis).HasForeignKey(x => x.FarmerId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.CropSeason).WithMany().HasForeignKey(x => x.CropSeasonId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.FarmerId, x.CropSeasonId }).IsUnique();
        });
        modelBuilder.Entity<ScaleKpi>(e =>
        {
            e.ToTable("ScaleKpis");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Farmer).WithMany(x => x.ScaleKpis).HasForeignKey(x => x.FarmerId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.CropSeason).WithMany().HasForeignKey(x => x.CropSeasonId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.FarmerId, x.CropSeasonId }).IsUnique();
        });
        modelBuilder.Entity<TechnologiesKpi>(e =>
        {
            e.ToTable("TechnologiesKpis");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Farmer).WithMany(x => x.TechnologiesKpis).HasForeignKey(x => x.FarmerId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.CropSeason).WithMany().HasForeignKey(x => x.CropSeasonId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.FarmerId, x.CropSeasonId }).IsUnique();
        });
        modelBuilder.Entity<EsgKpi>(e =>
        {
            e.ToTable("EsgKpis");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Farmer).WithMany(x => x.EsgKpis).HasForeignKey(x => x.FarmerId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.CropSeason).WithMany().HasForeignKey(x => x.CropSeasonId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => new { x.FarmerId, x.CropSeasonId }).IsUnique();
        });

        modelBuilder.Entity<SegmentationConfiguration>(e =>
        {
            e.ToTable("SegmentationConfigurations");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(256).IsRequired();
            e.HasMany(x => x.Segments).WithOne(x => x.SegmentationConfiguration).HasForeignKey(x => x.SegmentationConfigurationId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Loyalty).WithOne(x => x.SegmentationConfiguration).HasForeignKey<SegmentationConfigurationLoyalty>(x => x.SegmentationConfigurationId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Quality).WithOne(x => x.SegmentationConfiguration).HasForeignKey<SegmentationConfigurationQuality>(x => x.SegmentationConfigurationId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Financial).WithOne(x => x.SegmentationConfiguration).HasForeignKey<SegmentationConfigurationFinancial>(x => x.SegmentationConfigurationId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Technology).WithOne(x => x.SegmentationConfiguration).HasForeignKey<SegmentationConfigurationTechnology>(x => x.SegmentationConfigurationId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Esg).WithOne(x => x.SegmentationConfiguration).HasForeignKey<SegmentationConfigurationEsg>(x => x.SegmentationConfigurationId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Yield).WithOne(x => x.SegmentationConfiguration).HasForeignKey<SegmentationConfigurationYield>(x => x.SegmentationConfigurationId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Scale).WithOne(x => x.SegmentationConfiguration).HasForeignKey<SegmentationConfigurationScale>(x => x.SegmentationConfigurationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SegmentationSegment>(e =>
        {
            e.ToTable("SegmentationSegments");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.SegmentName).HasMaxLength(128).IsRequired();
        });

        modelBuilder.Entity<SegmentationConfigurationLoyalty>(e =>
        {
            e.ToTable("SegmentationConfigurationLoyalties");
            e.HasKey(x => x.SegmentationConfigurationId);
            e.Property(x => x.Relevance).HasPrecision(9, 4);
            e.HasMany(x => x.SeasonQuantityRanges).WithOne(x => x.Loyalty).HasForeignKey(x => x.SegmentationConfigurationId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.HistoricalVolumeRanges).WithOne(x => x.Loyalty).HasForeignKey(x => x.SegmentationConfigurationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LoyaltySeasonQuantityRange>(e =>
        {
            e.ToTable("LoyaltySeasonQuantityRanges");
            e.HasKey(x => x.Id);
            e.HasMany(x => x.SkippedCropSeasons).WithOne(x => x.Range).HasForeignKey(x => x.LoyaltySeasonQuantityRangeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LoyaltySeasonQuantityRangeSkippedCropSeason>(e =>
        {
            e.ToTable("LoyaltySeasonQuantityRangeSkippedCropSeasons");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.CropSeason).WithMany().HasForeignKey(x => x.CropSeasonId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LoyaltyHistoricalVolumeRange>(e =>
        {
            e.ToTable("LoyaltyHistoricalVolumeRanges");
            e.HasKey(x => x.Id);
        });

        modelBuilder.Entity<SegmentationConfigurationQuality>(e =>
        {
            e.ToTable("SegmentationConfigurationQualities");
            e.HasKey(x => x.SegmentationConfigurationId);
            e.Property(x => x.Relevance).HasPrecision(9, 4);
            e.HasMany(x => x.IqsRanges).WithOne(x => x.Quality).HasForeignKey(x => x.SegmentationConfigurationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QualityIqsRange>(e =>
        {
            e.ToTable("QualityIqsRanges");
            e.HasKey(x => x.Id);
            e.HasMany(x => x.SkippedCropSeasons).WithOne(x => x.Range).HasForeignKey(x => x.QualityIqsRangeId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QualityIqsRangeSkippedCropSeason>(e =>
        {
            e.ToTable("QualityIqsRangeSkippedCropSeasons");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.CropSeason).WithMany().HasForeignKey(x => x.CropSeasonId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SegmentationConfigurationFinancial>(e =>
        {
            e.ToTable("SegmentationConfigurationFinancials");
            e.HasKey(x => x.SegmentationConfigurationId);
            e.Property(x => x.Relevance).HasPrecision(9, 4);
            e.HasMany(x => x.SelfFundingRanges).WithOne(x => x.Financial).HasForeignKey(x => x.SegmentationConfigurationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FinancialSelfFundingRange>(e =>
        {
            e.ToTable("FinancialSelfFundingRanges");
            e.HasKey(x => x.Id);
            e.HasMany(x => x.SkippedCropSeasons).WithOne(x => x.Range).HasForeignKey(x => x.FinancialSelfFundingRangeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FinancialSelfFundingRangeSkippedCropSeason>(e =>
        {
            e.ToTable("FinancialSelfFundingRangeSkippedCropSeasons");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.CropSeason).WithMany().HasForeignKey(x => x.CropSeasonId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SegmentationConfigurationTechnology>(e =>
        {
            e.ToTable("SegmentationConfigurationTechnologies");
            e.HasKey(x => x.SegmentationConfigurationId);
            e.Property(x => x.Relevance).HasPrecision(9, 4);
        });

        modelBuilder.Entity<SegmentationConfigurationEsg>(e =>
        {
            e.ToTable("SegmentationConfigurationEsgs");
            e.HasKey(x => x.SegmentationConfigurationId);
            e.Property(x => x.Relevance).HasPrecision(9, 4);
        });

        modelBuilder.Entity<SegmentationConfigurationYield>(e =>
        {
            e.ToTable("SegmentationConfigurationYields");
            e.HasKey(x => x.SegmentationConfigurationId);
            e.Property(x => x.Relevance).HasPrecision(9, 4);
            e.HasMany(x => x.Ranges).WithOne(x => x.Yield).HasForeignKey(x => x.SegmentationConfigurationId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<YieldRange>(e =>
        {
            e.ToTable("YieldRanges");
            e.HasKey(x => x.Id);
            e.HasMany(x => x.SkippedCropSeasons).WithOne(x => x.Range).HasForeignKey(x => x.YieldRangeId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<YieldRangeSkippedCropSeason>(e =>
        {
            e.ToTable("YieldRangeSkippedCropSeasons");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.CropSeason).WithMany().HasForeignKey(x => x.CropSeasonId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SegmentationConfigurationScale>(e =>
        {
            e.ToTable("SegmentationConfigurationScales");
            e.HasKey(x => x.SegmentationConfigurationId);
            e.Property(x => x.Relevance).HasPrecision(9, 4);
            e.HasMany(x => x.Ranges).WithOne(x => x.Scale).HasForeignKey(x => x.SegmentationConfigurationId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ScaleRange>(e =>
        {
            e.ToTable("ScaleRanges");
            e.HasKey(x => x.Id);
            e.HasMany(x => x.SkippedCropSeasons).WithOne(x => x.Range).HasForeignKey(x => x.ScaleRangeId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ScaleRangeSkippedCropSeason>(e =>
        {
            e.ToTable("ScaleRangeSkippedCropSeasons");
            e.HasKey(x => x.Id);
            e.HasOne(x => x.CropSeason).WithMany().HasForeignKey(x => x.CropSeasonId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<SegmentationSimulation>(e =>
        {
            e.ToTable("SegmentationSimulations");
            e.HasKey(x => x.Id);
            e.Property(x => x.Status).HasMaxLength(1).IsRequired();
            e.HasOne(x => x.SegmentationConfiguration).WithMany(x => x.Simulations).HasForeignKey(x => x.SegmentationConfigurationId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.CropSeason).WithMany().HasForeignKey(x => x.CropSeasonId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.Farmers).WithOne(x => x.SegmentationSimulation).HasForeignKey(x => x.SegmentationSimulationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SegmentationSimulationFarmer>(e =>
        {
            e.ToTable("SegmentationSimulationFarmers");
            e.HasKey(x => x.Id);
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
            e.HasOne(x => x.Segment).WithMany(x => x.FarmerSegmentations).HasForeignKey(x => x.SegmentationConfigurationSegmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

}
