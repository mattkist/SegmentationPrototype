# Simulation scoring rules

This document describes how the prototype scores farmers when a simulation is run.

## Configuration model

- **Header** (`SegmentationConfiguration`): name and shared segment definitions (name, exclusivity, discounts).
- **Culture type block** (`SegmentationConfigurationCultureType`): one row per tobacco culture type with `MaximumScore`, KPI rule blocks (each with user-defined `MaxScore`), and per-segment `RangeMin` values.
- On save, the API requires:
  - Sum of KPI `MaxScore` values = culture-type `MaximumScore`.
  - Each KPI `MaxScore` equals the **derived cap** from its rules (range maxima, sum of positive technology scores, etc.).
- **Relevance** on each KPI block is stored as a fraction (`KpiMaxScore / MaximumScore`) for compatibility; the UI shows it as read-only percent.

Configurations are **crop-season-agnostic**. Season lists are chosen when starting a simulation.

## Simulation inputs

| Input | Meaning |
|--------|---------|
| `CropSeasonId` on `SegmentationSimulation` | Target season for the official snapshot when accepting results. |
| `SegmentationSimulationCropSeason` rows | Scope seasons for scoring. **Loyalty** and **Yield & Scale** evaluate across this full list. All other KPIs use **only the latest (highest year) season** in that list. |

## New farmer rule (important)

Before any scoring, for each farmer and culture type:

- Let **prior season** = simulation target crop season year **minus 1** (e.g. target 2027 → prior 2026).
- If there is **no** `LoyaltyKpi` **and** **no** `YieldAndScaleKpi` for that farmer/culture type in the prior season, the farmer is a **new farmer**:
  - All component scores and total score = **0**
  - **No segment** assigned (`IsNewFarmer = true`)
  - Excluded from segment distribution charts and ranking denominators

This gate uses the **target** season only, not the scope season list.

## Farmer culture type selection

Each farmer is scored with **one** culture type configuration:

- If the farmer has KPI rows for multiple culture types in the scope seasons, use the culture type with the **largest total `Scale`** in `YieldAndScaleKpi` across those seasons (sum per culture type).

## Loyalty (multi-season, consolidated delivery %)

Rules look like:

> Planting for company in at least **P** crop seasons  
> AND delivery amount between **min–max** in **D** crop seasons → **score**

Evaluation uses only seasons in the simulation scope list.

1. **Planting**: count scope seasons where the farmer has a loyalty KPI row. Must be `>= P`.
2. **Delivery window**: the **D** most recent scope seasons (by year) must all fall within min–max **delivered %**.
3. **Consolidated delivered %** (for historical volume bands and display): across selected scope seasons for the farmer’s tobacco type,  
   `sum(DeliveredAmountKg) / sum(ContractedAmountKg) * 100` — **not** an average of per-season percentages.
4. **Precedence**: when several rules match, the rule with the **highest `PlantingCropSeasonAmount`** wins.

## Point-in-time KPIs (latest scope season only)

For **Quality**, **Financial**, **Technology**, and **ESG**, the engine reads KPI facts from **`LatestScopeCropSeasonId`**: the highest crop-season year among the simulation scope list.

- **Quality**: IQS range match, plus `NtrmScore` / `MixtureScore` when flags are set on that season’s row.
- **Financial**: self-funding range on that season; add `DebtScore` when `HaveDebt` is true.
- **Technology**: dynamic catalog (`Technologies` table). `TechnologiesKpi` has one row per `(farmer, season, culture type, technology)`. Sum configured scores for technologies present in the latest scope season.
- **ESG**: `EsgKpi` supplies reforestation / native forest percentages (capped). `EsgIrregularityKpi` supplies irregularity types present; sum configured irregularity scores.

Legacy separate **Yield** and **Scale** configuration blocks may still exist in old configurations; KPI facts live in **`YieldAndScaleKpi`** only.

## Yield & Scale (multi-season, consolidated)

Rules look like:

> Planting for company in **N** crop seasons with yield & scale data  
> AND consolidated yield between **min–max**  
> AND average module (scale) between **min–max** → **score**

Across scope seasons where `YieldAndScaleKpi` exists for the culture type:

1. **Planting count** = number of those seasons; must be `>= YieldAndScaleCropSeasonAmount`.
2. **Average module** = arithmetic mean of `Scale` across those seasons.
3. **Consolidated yield** = `sum(ContractedAmountKg) / sum(Scale)` (integer division in prototype), **not** mean of per-season yield.
4. **Precedence**: highest `YieldAndScaleCropSeasonAmount` among matching ranges wins.
5. Block **MaxScore** (configured) must match the largest positive range score in rules.

## Rankings

Rankings between farmers are **disabled** in the prototype UI. Component and total scores are stored. The `Rank` column remains for schema compatibility.

## Segments and export

- Segment assignment compares total score to culture-type `RangeMin` thresholds (highest matching threshold first), respecting `OnlyExclusiveFarmer`. New farmers receive no segment.
- `GET /api/SegmentationSimulations/{id}/export.csv` exports all farmers with component scores, segment, `IsNewFarmer`, and non-exclusive flag.
- Detail DTO includes `OverallSegmentDistribution` and `SegmentDistributionByCultureType` (% of **scored** farmers per segment).
