using Segmentation.Application.Dtos;
using Segmentation.Domain.Entities;
using Segmentation.Domain.Scoring;

namespace Segmentation.Infrastructure.Services;

internal static class SimulationKpiScopeMapper
{
    public static SimulationKpiScopeSet ToScopeSet(IReadOnlyList<SimulationKpiScopeInputDto> inputs)
    {
        var byKind = inputs.ToDictionary(i => i.KpiKind, StringComparer.OrdinalIgnoreCase);
        return new SimulationKpiScopeSet
        {
            Loyalty = ToScope(byKind, SimulationKpiKind.Loyalty, null),
            Quality = ToScope(byKind, SimulationKpiKind.Quality, KpiValueAggregation.Average),
            Financial = ToScope(byKind, SimulationKpiKind.Financial, KpiValueAggregation.LastActiveCropData),
            Esg = ToScope(byKind, SimulationKpiKind.Esg, null),
            Technologies = ToScope(byKind, SimulationKpiKind.Technologies, null),
            Yield = ToScope(byKind, SimulationKpiKind.Yield, KpiValueAggregation.Average),
            Scale = ToScope(byKind, SimulationKpiKind.Scale, KpiValueAggregation.LastActiveCropData)
        };
    }

    public static IReadOnlyList<SimulationKpiScopeInputDto> ToInputs(
        IEnumerable<SegmentationSimulationKpiScope> scopes) =>
        scopes.Select(s => new SimulationKpiScopeInputDto
        {
            KpiKind = s.KpiKind,
            CropSeasonIds = s.Seasons.Select(x => x.CropSeasonId).OrderByDescending(x => x).ToList(),
            ValueAggregation = s.ValueAggregation
        }).ToList();

    public static List<SegmentationSimulationKpiScope> ToEntities(
        Guid simulationId,
        IReadOnlyList<SimulationKpiScopeInputDto> inputs)
    {
        return inputs.Select(dto => new SegmentationSimulationKpiScope
        {
            Id = Guid.NewGuid(),
            SegmentationSimulationId = simulationId,
            KpiKind = dto.KpiKind,
            ValueAggregation = dto.ValueAggregation,
            Seasons = dto.CropSeasonIds.Distinct().Select(cs => new SegmentationSimulationKpiScopeSeason
            {
                Id = Guid.NewGuid(),
                CropSeasonId = cs
            }).ToList()
        }).ToList();
    }

    private static KpiScope ToScope(
        Dictionary<string, SimulationKpiScopeInputDto> byKind,
        string kind,
        string? defaultAggregation)
    {
        if (!byKind.TryGetValue(kind, out var input) || input.CropSeasonIds.Count == 0)
            return new KpiScope { CropSeasonIdsDescending = [], ValueAggregation = defaultAggregation };

        return new KpiScope
        {
            CropSeasonIdsDescending = input.CropSeasonIds.Distinct().OrderByDescending(x => x).ToList(),
            ValueAggregation = input.ValueAggregation ?? defaultAggregation
        };
    }
}
