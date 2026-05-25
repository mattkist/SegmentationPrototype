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

    private static readonly HashSet<Guid> BurleyFarmerIds = new() { F900301, F900302, F900303 };

    public static async Task SeedAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        await SeedCatalogsAsync(db, cancellationToken);

        if (await db.Farmers.AnyAsync(cancellationToken))
            return;

        db.CultureTypes.AddRange(
            new CultureType { Code = "FCV", Name = "Virginia" },
            new CultureType { Code = "BLY", Name = "Burley" },
            new CultureType { Code = "CO", Name = "Comum" });

        db.CropSeasons.AddRange(
            new CropSeason { Id = 2023, Code = "2023" },
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

        foreach (var row in LoyaltySeed())
        {
            db.LoyaltyKpis.Add(new LoyaltyKpi
            {
                Id = gid.Next(),
                FarmerId = row.FarmerId,
                CropSeasonId = row.Season,
                CultureTypeCode = CultureFor(row.FarmerId),
                DeliveredPercentage = row.Pct,
                DeliveredAmountKg = row.Delivered,
                ContractedAmountKg = row.Contracted
            });
        }

        foreach (var row in QualitySeed())
        {
            db.QualityKpis.Add(new QualityKpi
            {
                Id = gid.Next(),
                FarmerId = row.FarmerId,
                CropSeasonId = row.Season,
                CultureTypeCode = CultureFor(row.FarmerId),
                Iqs = row.Iqs,
                HadNtrm = row.Ntrm,
                HadQualityMixture = row.Mixture
            });
        }

        foreach (var (farmerId, season, sf, debt) in FinancialSeed())
        {
            db.FinancialKpis.Add(new FinancialKpi
            {
                Id = gid.Next(),
                FarmerId = farmerId,
                CropSeasonId = season,
                CultureTypeCode = CultureFor(farmerId),
                SelfFundingPercentage = sf,
                HaveDebt = debt
            });
        }

        foreach (var row in YieldAndScaleSeed())
        {
            db.YieldAndScaleKpis.Add(new YieldAndScaleKpi
            {
                Id = gid.Next(),
                FarmerId = row.FarmerId,
                CropSeasonId = row.Season,
                CultureTypeCode = CultureFor(row.FarmerId),
                Yield = row.Yield,
                Scale = row.Scale,
                ContractedAmountKg = row.Contracted
            });
        }

        foreach (var row in TechnologiesSeed())
        {
            db.TechnologiesKpis.Add(new TechnologiesKpi
            {
                Id = gid.Next(),
                FarmerId = row.FarmerId,
                CropSeasonId = row.Season,
                CultureTypeCode = CultureFor(row.FarmerId),
                TechnologyId = row.TechnologyId
            });
        }

        foreach (var row in EsgSeed())
        {
            db.EsgKpis.Add(new EsgKpi
            {
                Id = gid.Next(),
                FarmerId = row.FarmerId,
                CropSeasonId = row.Season,
                CultureTypeCode = CultureFor(row.FarmerId),
                ReforestationPercentage = row.Ref,
                NativeForestPercentage = row.Native
            });
        }

        foreach (var row in EsgIrregularitySeed())
        {
            db.EsgIrregularityKpis.Add(new EsgIrregularityKpi
            {
                Id = gid.Next(),
                FarmerId = row.FarmerId,
                CropSeasonId = row.Season,
                CultureTypeCode = CultureFor(row.FarmerId),
                IrregularityTypeId = row.IrregularityTypeId
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedCatalogsAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        if (!await db.Technologies.AnyAsync(cancellationToken))
        {
            db.Technologies.AddRange(
                new TechnologyCatalog { Id = 1, Name = "Large Base Ridge With Mulch" },
                new TechnologyCatalog { Id = 2, Name = "Broad Grate Furnace" },
                new TechnologyCatalog { Id = 3, Name = "Tecknology Package Adderence" },
                new TechnologyCatalog { Id = 4, Name = "Standard Barn" });
        }

        if (!await db.IrregularityTypes.AnyAsync(cancellationToken))
        {
            db.IrregularityTypes.AddRange(
                new IrregularityTypeCatalog { Id = 1, Name = "Minor Irregularity" },
                new IrregularityTypeCatalog { Id = 2, Name = "Major Irregularity" });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static IEnumerable<(Guid FarmerId, int Season, int Pct, int Delivered, int Contracted)> LoyaltySeed()
    {
        yield return (F900100, 2023, 92, 9200, 10000);
        yield return (F900101, 2023, 91, 9100, 10000);
        yield return (F900102, 2023, 93, 9300, 10000);
        yield return (F900100, 2024, 92, 9200, 10000);
        yield return (F900101, 2024, 91, 9100, 10000);
        yield return (F900102, 2024, 93, 9300, 10000);

        foreach (var s in new[] { 2025, 2026 })
        {
            yield return (F900100, s, 92, 9200, 10000);
            yield return (F900101, s, 91, 9100, 10000);
            yield return (F900102, s, 93, 9300, 10000);
            yield return (F900201, s, 80, 8000, 10000);
            yield return (F900202, s, 85, 8500, 10000);
            yield return (F900203, s, 89, 8900, 10000);
            yield return (F900301, s, 91, 9100, 10000);
            yield return (F900302, s, 84, 8400, 10000);
            yield return (F900303, s, 95, 9500, 10000);
        }
    }

    private static IEnumerable<(Guid FarmerId, int Season, int Iqs, bool Ntrm, bool Mixture)> QualitySeed()
    {
        foreach (var s in new[] { 2023, 2024, 2025 })
        {
            yield return (F900100, s, 92, false, false);
            yield return (F900101, s, 91, false, false);
            yield return (F900102, s, 93, false, false);
        }

        foreach (var f in new[] { F900201, F900202, F900203, F900301, F900302, F900303 })
            yield return (f, 2025, 70, false, false);

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
        foreach (var s in new[] { 2023, 2024, 2025 })
        {
            yield return (F900100, s, 92, false);
            yield return (F900101, s, 91, false);
            yield return (F900102, s, 93, false);
        }

        foreach (var f in new[] { F900201, F900202, F900203, F900301, F900302, F900303 })
            yield return (f, 2025, 70, false);

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

    private static IEnumerable<(Guid FarmerId, int Season, int Yield, int Scale, int Contracted)> YieldAndScaleSeed()
    {
        foreach (var s in new[] { 2023, 2024, 2025, 2026 })
        {
            yield return (F900100, s, 3000, 6, 18000);
            yield return (F900101, s, 3000, 6, 18000);
            yield return (F900102, s, 3000, 6, 18000);
        }

        foreach (var s in new[] { 2025, 2026 })
        {
            yield return (F900201, s, 2600, 4, 10400);
            yield return (F900202, s, 2600, 4, 10400);
            yield return (F900203, s, 2400, 2, 4800);
            yield return (F900301, s, 2600, 4, 10400);
            yield return (F900302, s, 2600, 4, 10400);
            yield return (F900303, s, 2400, 2, 4800);
        }
    }

    private static IEnumerable<(Guid FarmerId, int Season, int TechnologyId)> TechnologiesSeed()
    {
        foreach (var s in new[] { 2023, 2024, 2025, 2026 })
        {
            foreach (var f in new[] { F900100, F900101, F900102 })
            {
                yield return (f, s, 1);
                yield return (f, s, 2);
                yield return (f, s, 3);
            }
        }

        foreach (var s in new[] { 2025, 2026 })
        {
            yield return (F900201, s, 1);
            yield return (F900201, s, 2);
            yield return (F900202, s, 1);
            yield return (F900202, s, 2);
            yield return (F900301, s, 1);
            yield return (F900301, s, 4);
            yield return (F900302, s, 1);
            yield return (F900302, s, 4);
        }
    }

    private static IEnumerable<(Guid FarmerId, int Season, int Ref, int Native)> EsgSeed()
    {
        foreach (var s in new[] { 2023, 2024, 2025, 2026 })
        {
            yield return (F900100, s, 20, 18);
            yield return (F900101, s, 20, 18);
            yield return (F900102, s, 20, 18);
        }

        foreach (var s in new[] { 2025, 2026 })
        {
            yield return (F900201, s, 10, 8);
            yield return (F900202, s, 10, 8);
            yield return (F900203, s, 5, 4);
            yield return (F900301, s, 10, 8);
            yield return (F900302, s, 10, 8);
            yield return (F900303, s, 5, 4);
        }
    }

    private static IEnumerable<(Guid FarmerId, int Season, int IrregularityTypeId)> EsgIrregularitySeed()
    {
        foreach (var s in new[] { 2025, 2026 })
        {
            yield return (F900201, s, 1);
            yield return (F900202, s, 1);
            yield return (F900301, s, 1);
            yield return (F900302, s, 1);
        }

        yield return (F900203, 2025, 2);
        yield return (F900303, 2025, 2);
        yield return (F900203, 2026, 2);
        yield return (F900303, 2026, 2);
    }

    private static string CultureFor(Guid farmerId) =>
        BurleyFarmerIds.Contains(farmerId) ? "BLY" : "FCV";

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
