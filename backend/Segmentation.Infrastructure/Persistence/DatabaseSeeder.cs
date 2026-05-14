using Microsoft.EntityFrameworkCore;
using Segmentation.Domain.Entities;

namespace Segmentation.Infrastructure.Persistence;

/// <summary>
/// Reference data for the prototype (farmers, clusters, crop seasons, KPI facts).
/// Safe to run after migrations; skips when farmers already exist.
/// </summary>
public static class DatabaseSeeder
{
    private static readonly Guid ClusterBat = Guid.Parse("70010100-0000-4000-8000-000000000001");
    private static readonly Guid ClusterVirginia = Guid.Parse("70010200-0000-4000-8000-000000000001");
    private static readonly Guid ClusterBurley = Guid.Parse("70010300-0000-4000-8000-000000000001");

    private static readonly Guid F900100 = Guid.Parse("90010000-0000-4000-8000-000000000001");
    private static readonly Guid F900101 = Guid.Parse("90010100-0000-4000-8000-000000000001");
    private static readonly Guid F900102 = Guid.Parse("90010200-0000-4000-8000-000000000001");
    private static readonly Guid F900201 = Guid.Parse("90020100-0000-4000-8000-000000000001");
    private static readonly Guid F900202 = Guid.Parse("90020200-0000-4000-8000-000000000001");
    private static readonly Guid F900203 = Guid.Parse("90020300-0000-4000-8000-000000000001");
    private static readonly Guid F900301 = Guid.Parse("90030100-0000-4000-8000-000000000001");
    private static readonly Guid F900302 = Guid.Parse("90030200-0000-4000-8000-000000000001");
    private static readonly Guid F900303 = Guid.Parse("90030300-0000-4000-8000-000000000001");

    public static async Task SeedAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.Farmers.AnyAsync(cancellationToken))
            return;

        db.CropSeasons.AddRange(
            new CropSeason { Id = 2024, Code = "2024" },
            new CropSeason { Id = 2025, Code = "2025" },
            new CropSeason { Id = 2026, Code = "2026" },
            new CropSeason { Id = 2027, Code = "2027" });

        db.FarmerClusters.AddRange(
            new FarmerCluster { Id = ClusterBat, Code = "700101", Name = "BAT" },
            new FarmerCluster { Id = ClusterVirginia, Code = "700102", Name = "Virginia Farm" },
            new FarmerCluster { Id = ClusterBurley, Code = "700103", Name = "Burley Farm" });

        db.Farmers.AddRange(
            new Farmer { Id = F900100, Code = "900100", Name = "Matheus Kist", NonExclusiveFarmer = false },
            new Farmer { Id = F900101, Code = "900101", Name = "Rafael Lasta", NonExclusiveFarmer = false },
            new Farmer { Id = F900102, Code = "900102", Name = "Samuel Limberger", NonExclusiveFarmer = false },
            new Farmer { Id = F900201, Code = "900201", Name = "Farmer 1 Virginia", NonExclusiveFarmer = false },
            new Farmer { Id = F900202, Code = "900202", Name = "Farmer 2 Virginia", NonExclusiveFarmer = false },
            new Farmer { Id = F900203, Code = "900203", Name = "Farmer Solo Virginia", NonExclusiveFarmer = true },
            new Farmer { Id = F900301, Code = "900301", Name = "Farmer 1 Burley", NonExclusiveFarmer = false },
            new Farmer { Id = F900302, Code = "900302", Name = "Farmer 2 Burley", NonExclusiveFarmer = false },
            new Farmer { Id = F900303, Code = "900303", Name = "Farmer Solo Burley", NonExclusiveFarmer = true });

        db.FarmerClusterFarmers.AddRange(
            new FarmerClusterFarmer { ClusterId = ClusterBat, FarmerId = F900101 },
            new FarmerClusterFarmer { ClusterId = ClusterBat, FarmerId = F900102 },
            new FarmerClusterFarmer { ClusterId = ClusterVirginia, FarmerId = F900201 },
            new FarmerClusterFarmer { ClusterId = ClusterVirginia, FarmerId = F900202 },
            new FarmerClusterFarmer { ClusterId = ClusterBurley, FarmerId = F900301 },
            new FarmerClusterFarmer { ClusterId = ClusterBurley, FarmerId = F900302 });

        await db.SaveChangesAsync(cancellationToken);

        var gid = new SequentialGuidGenerator("c0000000-0000-4000-8000-000000000000");

        foreach (var (farmerId, season, d) in LoyaltySeed())
            db.LoyaltyKpis.Add(new LoyaltyKpi { Id = gid.Next(), FarmerId = farmerId, CropSeasonId = season, DeliveredPercentage = d });

        foreach (var row in QualitySeed())
            db.QualityKpis.Add(new QualityKpi
            {
                Id = gid.Next(),
                FarmerId = row.FarmerId,
                CropSeasonId = row.Season,
                Iqs = row.Iqs,
                HadNtrm = row.Ntrm,
                HadQualityMixture = row.Mixture
            });

        foreach (var (farmerId, season, sf, debt) in FinancialSeed())
            db.FinancialKpis.Add(new FinancialKpi
            {
                Id = gid.Next(),
                FarmerId = farmerId,
                CropSeasonId = season,
                SelfFundingPercentage = sf,
                HaveDebt = debt
            });

        foreach (var (farmerId, season, y) in YieldSeed())
            db.YieldKpis.Add(new YieldKpi { Id = gid.Next(), FarmerId = farmerId, CropSeasonId = season, Yield = y });

        foreach (var (farmerId, season, s) in ScaleSeed())
            db.ScaleKpis.Add(new ScaleKpi { Id = gid.Next(), FarmerId = farmerId, CropSeasonId = season, Scale = s });

        foreach (var row in TechnologiesSeed())
            db.TechnologiesKpis.Add(new TechnologiesKpi
            {
                Id = gid.Next(),
                FarmerId = row.FarmerId,
                CropSeasonId = row.Season,
                HasLargeBaseRidgeWithMulch = row.Mulch,
                HasBroadGrateFurnace = row.Furnace,
                HasTechnologyPackageAdherence = row.Package
            });

        foreach (var row in EsgSeed())
            db.EsgKpis.Add(new EsgKpi
            {
                Id = gid.Next(),
                FarmerId = row.FarmerId,
                CropSeasonId = row.Season,
                ReforestationPercentage = row.Ref,
                NativeForestPercentage = row.Native,
                HasMinorIrregularity = row.Minor,
                HasMajorIrregularity = row.Major
            });

        await db.SaveChangesAsync(cancellationToken);
    }

    private static IEnumerable<(Guid FarmerId, int Season, int Delivered)> LoyaltySeed()
    {
        yield return (F900100, 2024, 92);
        yield return (F900101, 2024, 91);
        yield return (F900102, 2024, 93);

        foreach (var s in new[] { 2025, 2026 })
        {
            yield return (F900100, s, 92);
            yield return (F900101, s, 91);
            yield return (F900102, s, 93);
            yield return (F900201, s, 80);
            yield return (F900202, s, 85);
            yield return (F900203, s, 89);
            yield return (F900301, s, 91);
            yield return (F900302, s, 84);
            yield return (F900303, s, 95);
        }
    }

    private static IEnumerable<(Guid FarmerId, int Season, int Iqs, bool Ntrm, bool Mixture)> QualitySeed()
    {
        foreach (var s in new[] { 2024, 2025 })
        {
            yield return (F900100, s, 92, false, false);
            yield return (F900101, s, 91, false, false);
            yield return (F900102, s, 93, false, false);
        }

        foreach (var f in new[] { F900201, F900202, F900203, F900301, F900302, F900303 })
        {
            yield return (f, 2025, 70, false, false);
        }

        yield return (F900100, 2026, 92, false, false);
        yield return (F900101, 2026, 91, false, false);
        yield return (F900102, 2026, 93, false, false);
        yield return (F900201, 2026, 70, true, false);
        yield return (F900202, 2026, 70, true, false);
        yield return (F900203, 2026, 60, false, true);
        yield return (F900301, 2026, 70, true, false);
        yield return (F900302, 2026, 70, true, false);
        yield return (F900303, 2026, 60, false, true);
    }

    private static IEnumerable<(Guid FarmerId, int Season, int Sf, bool Debt)> FinancialSeed()
    {
        foreach (var s in new[] { 2024, 2025 })
        {
            yield return (F900100, s, 92, false);
            yield return (F900101, s, 91, false);
            yield return (F900102, s, 93, false);
        }

        foreach (var f in new[] { F900201, F900202, F900203, F900301, F900302, F900303 })
        {
            yield return (f, 2025, 70, false);
        }

        yield return (F900100, 2026, 92, false);
        yield return (F900101, 2026, 91, false);
        yield return (F900102, 2026, 93, false);
        yield return (F900201, 2026, 70, true);
        yield return (F900202, 2026, 70, true);
        yield return (F900203, 2026, 50, false);
        yield return (F900301, 2026, 70, true);
        yield return (F900302, 2026, 70, true);
        yield return (F900303, 2026, 50, false);
    }

    private static IEnumerable<(Guid FarmerId, int Season, int Yield)> YieldSeed()
    {
        foreach (var s in new[] { 2024, 2025, 2026 })
        {
            yield return (F900100, s, 3000);
            yield return (F900101, s, 3000);
            yield return (F900102, s, 3000);
        }

        foreach (var s in new[] { 2025, 2026 })
        {
            yield return (F900201, s, 2600);
            yield return (F900202, s, 2600);
            yield return (F900203, s, 2400);
            yield return (F900301, s, 2600);
            yield return (F900302, s, 2600);
            yield return (F900303, s, 2400);
        }
    }

    private static IEnumerable<(Guid FarmerId, int Season, int Scale)> ScaleSeed()
    {
        foreach (var s in new[] { 2024, 2025, 2026 })
        {
            yield return (F900100, s, 6);
            yield return (F900101, s, 6);
            yield return (F900102, s, 6);
        }

        foreach (var s in new[] { 2025, 2026 })
        {
            yield return (F900201, s, 4);
            yield return (F900202, s, 4);
            yield return (F900203, s, 2);
            yield return (F900301, s, 4);
            yield return (F900302, s, 4);
            yield return (F900303, s, 2);
        }
    }

    private static IEnumerable<(Guid FarmerId, int Season, bool Mulch, bool Furnace, bool Package)> TechnologiesSeed()
    {
        foreach (var s in new[] { 2024, 2025, 2026 })
        {
            yield return (F900100, s, true, true, true);
            yield return (F900101, s, true, true, true);
            yield return (F900102, s, true, true, true);
        }

        foreach (var s in new[] { 2025, 2026 })
        {
            yield return (F900201, s, true, true, false);
            yield return (F900202, s, true, true, false);
            yield return (F900203, s, false, false, false);
            yield return (F900301, s, true, false, false);
            yield return (F900302, s, true, false, false);
            yield return (F900303, s, false, false, false);
        }
    }

    private static IEnumerable<(Guid FarmerId, int Season, int Ref, int Native, bool Minor, bool Major)> EsgSeed()
    {
        foreach (var s in new[] { 2024, 2025, 2026 })
        {
            yield return (F900100, s, 20, 18, false, false);
            yield return (F900101, s, 20, 18, false, false);
            yield return (F900102, s, 20, 18, false, false);
        }

        foreach (var s in new[] { 2025, 2026 })
        {
            yield return (F900201, s, 10, 8, true, false);
            yield return (F900202, s, 10, 8, true, false);
            yield return (F900203, s, 5, 4, false, true);
            yield return (F900301, s, 10, 8, true, false);
            yield return (F900302, s, 10, 8, true, false);
            yield return (F900303, s, 5, 4, false, true);
        }
    }

    /// <summary>Deterministic GUID sequence for seed rows.</summary>
    private sealed class SequentialGuidGenerator
    {
        private Guid _current;

        public SequentialGuidGenerator(string baseGuid) => _current = Guid.Parse(baseGuid);

        public Guid Next()
        {
            var bytes = _current.ToByteArray();
            for (var i = bytes.Length - 1; i >= 0; i--)
            {
                if (bytes[i] < byte.MaxValue)
                {
                    bytes[i]++;
                    break;
                }

                bytes[i] = 0;
            }

            _current = new Guid(bytes);
            return _current;
        }
    }
}
