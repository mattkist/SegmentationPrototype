# Simulation scoring rules

This document describes how the prototype scores farmers when a simulation is run.

## Configuration model

- **Header** (`SegmentationConfiguration`): name and shared segment definitions (name, exclusivity, discounts). No culture type, no maximum score, no `RangeMin` on header segments.
- **Culture type block** (`SegmentationConfigurationCultureType`): one row per tobacco culture type with `MaximumScore`, all KPI rule blocks, and per-segment `RangeMin` values (`SegmentationConfigurationCultureTypeSegment`).
- Configurations are **crop-season-agnostic**. Season lists are chosen when starting a simulation.

## Simulation inputs

| Input | Meaning |
|--------|---------|
| `CropSeasonId` on `SegmentationSimulation` | Target season for the official snapshot when accepting results. |
| `SegmentationSimulationCropSeason` rows | Scope seasons for scoring. Stored newest-first when loaded. **Loyalty** and **Yield & Scale** evaluate across this full list. All other KPIs use **only the latest (highest year) season** in that list. |

## Farmer culture type selection

Each farmer is scored with **one** culture type configuration:

- If the farmer has KPI rows for multiple culture types in the scope seasons, use the culture type with the **largest total `Scale` (hectares)** in `ScaleKpi` across those seasons (sum per culture type).
- Each farmer may have only **one** official segmentation per `(FarmerId, CropSeasonId)` when accepting a simulation.

## Loyalty season-quantity rules

Rules look like:

> Planting for company in at least **P** crop seasons  
> AND delivery amount between **min–max** in **D** crop seasons → **score**

Evaluation uses only seasons in the simulation scope list (not “starting from” or “skipped” lists).

1. **Planting**: count scope seasons where the farmer has a loyalty KPI row. Must be `>= P`.
2. **Delivery window**: the **D** most recent scope seasons (by year) must all have delivery % within min–max.
3. **Precedence**: when several rules match, the rule with the **highest `PlantingCropSeasonAmount`** wins (most specific).  
   Example: scope `{2026,2025,2024,2023}`, farmer has loyalty data in all four, delivery 0–90 in only one recent season:
   - Rule `P=1, D=1` would match loosely, but
   - Rule `P=4, D=1` also matches and wins → penalty score applies.

## Point-in-time KPIs (latest scope season only)

For **Quality**, **Financial**, **Technology**, **ESG**, **Yield**, and **Scale** (when configured separately), the engine reads KPI facts from **`LatestScopeCropSeasonId`**: the highest crop-season year among the simulation scope list (not the simulation target season unless it is also the latest in scope).

- **Quality**: IQS range match, plus `NtrmScore` / `MixtureScore` when flags are set on that season’s row.
- **Financial**: self-funding range on that season; add `DebtScore` when `HaveDebt` is true.
- **Technology**: sum scores for mulch, furnace, package adherence, and standard barn when the corresponding flags are true.
- **ESG**: reforestation / native forest percentages (capped), minor/major irregularity flags.
- **Yield** / **Scale** (separate blocks): first matching min–max range on that season’s value.

**Loyalty** and **Yield & Scale** are the only KPIs that consider multiple scope seasons.

### Example: scope includes a season with no KPI row

Scenario: the farmer planted for the company in **2023, 2024, and 2025**, did not plant in **2026**, and the simulation scope is `{2023, 2024, 2025, 2026}`.

- **`LatestScopeCropSeasonId`** is **2026** (highest year in scope).
- **Loyalty** and **Yield & Scale** still use seasons **2023–2025** where loyalty / yield+scale data exist.
- **All other KPIs** read only **2026**. With no KPI row for 2026, those components score **0** even though older seasons have data.

This is intentional: point-in-time KPIs reflect the latest scoped season; multi-season KPIs reflect planting and averages across the scoped seasons that have the required facts.

## Yield & Scale (combined, optional)

An optional culture-type block scores **existing** Yield and Scale KPI rows together (no separate import type).

Rules look like:

> Planting for company in **N** crop seasons  
> AND average yield between **min–max**  
> AND module (scale, hectares) between **min–max** → **score**

Evaluation uses **all seasons in the simulation scope** (same list as loyalty):

1. **Seasons with data**: scope seasons where the farmer has **both** a yield KPI row and a scale KPI row for the farmer’s culture type.
2. **Planting count**: number of those seasons must be `>= YieldAndScaleCropSeasonAmount`.
3. **Averages**: arithmetic mean of yield and of module across those seasons (rounded to integers for range checks).
4. **Match**: average yield and average module must fall inside the range bounds.
5. **Precedence**: when several rules match, the rule with the **highest `YieldAndScaleCropSeasonAmount`** wins (most specific planting requirement).
6. A farmer matches **at most one** range; configuration **MaxScore** for this block is the **largest positive range score** (or 0 if none).

You may configure **only** Yield & Scale (set separate Yield/Scale relevance to 0%), **only** separate Yield and Scale, or both (relevance shares must still sum to 100% when caps match maximum score).

## Rankings

Rankings between farmers are **disabled**. Component and total scores are still stored. The `Rank` column remains for schema compatibility and is always `0`.

## Segments

Segment assignment compares total score to culture-type `RangeMin` thresholds (highest matching threshold first), respecting `OnlyExclusiveFarmer` on the header segment definition.
