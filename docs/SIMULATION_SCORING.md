# Simulation scoring rules

This document describes how the prototype scores farmers when a simulation is run.

## Configuration model

- **Header** (`SegmentationConfiguration`): name and shared segment definitions (name, exclusivity, discounts).
- **Culture type block** (`SegmentationConfigurationCultureType`): one row per tobacco culture type with `MaximumScore`, KPI rule blocks (each with user-defined `MaxScore`), and per-segment `RangeMin` values.
- On save, the API requires:
  - Sum of KPI `MaxScore` values = culture-type `MaximumScore`.
  - Each KPI `MaxScore` equals the **derived cap** from its rules (range maxima, sum of positive technology scores, etc.).
- **Relevance** on each KPI block is stored as a fraction (`KpiMaxScore / MaximumScore`) for compatibility; the UI shows it as read-only percent.

Configurations are **crop-season-agnostic**. Per-KPI crop season lists and aggregation rules are chosen when starting a simulation.

## Simulation inputs

| Input | Meaning |
|--------|---------|
| `CropSeasonId` on `SegmentationSimulation` | Target season for the official snapshot when accepting results. |
| `SegmentationSimulationKpiScope` rows | One row per KPI kind (`Loyalty`, `Quality`, `Financial`, `Esg`, `Technologies`, `Yield`, `Scale`) with selected crop seasons and optional `ValueAggregation`. |
| `SegmentationSimulationKpiScopeSeason` rows | Crop seasons linked to each KPI scope. |

### Default aggregation (UI)

| KPI | Value aggregation | Notes |
|-----|-------------------|--------|
| Loyalty | — | Each selected season is evaluated individually (planting count, delivery windows, consolidated volume). |
| Quality | `Average` (default) or `LastActiveCropData` | **IQS** uses the selected rule. **HadNTRM** and **HadQualityMixture** always use **last active crop data** within the Quality scope (highest year where the farmer has a row), regardless of the IQS radio. |
| Financial | `LastActiveCropData` (default) or `Average` | **Self funding** uses the selected rule. **Have debt** always uses **last active crop data** within the Financial scope. |
| ESG | — | Always **last active crop data** within the ESG scope. |
| Technologies | — | Always **last active crop data** within the Technologies scope. |
| Yield | `Average` (default) or `LastActiveCropData` | Range match uses the derived yield value. |
| Scale | `LastActiveCropData` (default) or `Average` | Range match uses the derived scale value (ha). |

### Average vs last active crop data

For KPIs that support aggregation, only seasons **where the farmer has data** within the selected scope are considered.

- **`Average`**: arithmetic mean of the metric across those seasons (e.g. IQS, self funding %, yield, or scale).
- **`LastActiveCropData`**: value from the **highest crop-season year** in the scope where the farmer has a row for that metric.

**Example (Scale):** Farmer X planted in 2025, 2026, and 2027 with scale 4 ha, 4 ha, and 5 ha. The simulation Scale scope selects seasons **2025, 2026, 2027**. The farmer has no row in 2027.

- **Consider Average**: uses 2025 and 2026 → mean scale = (4 + 5) / 2 = **4.5 ha** for range matching.
- **Consider Last Active Crop Data**: uses 2026 → **5 ha** for range matching.

The same logic applies to Quality (IQS only), Financial (self funding % only), and Yield.

## KPI data model

Contract-level facts live in **`FarmerContractKpis`** (one row per farmer, crop season, culture type): loyalty delivery, quality, financial, yield, scale, ESG percentages, and a snapshot of `NonExclusive` from the farmer at import time.

**Technologies** and **ESG irregularities** remain in separate tables (`TechnologiesKpis`, `EsgIrregularityKpis`).

## New farmer rule (important)

Before any scoring, for each farmer and culture type:

- Let **prior season** = simulation target crop season year **minus 1** (e.g. target 2027 → prior 2026).
- If there is **no** `FarmerContractKpi` for that farmer/culture type in the prior season, the farmer is a **new farmer**:
  - All component scores and total score = **0**
  - **No segment** assigned (`IsNewFarmer = true`)
  - Excluded from segment distribution charts

This gate uses the **target** season only, not the per-KPI scope lists.

## Farmer culture type selection

Each farmer is scored with **one** culture type configuration:

- Among culture types configured, use the culture type with the **largest total `Scale`** in the simulation **Scale** scope seasons (sum per culture type).

## Loyalty (multi-season, consolidated delivery %)

Rules look like:

> Planting for company in at least **P** crop seasons  
> AND delivery amount between **min–max** in **D** crop seasons → **score**

Evaluation uses only seasons in the **Loyalty** scope list.

1. **Planting**: count Loyalty-scope seasons where the farmer has a contract KPI row. Must be `>= P`.
2. **Delivery window**: the **D** most recent Loyalty-scope seasons (by year) must all fall within min–max **delivered %**.
3. **Consolidated delivered %** (for historical volume bands): across Loyalty-scope seasons,  
   `sum(DeliveredAmountKg) / sum(ContractedAmountKg) * 100` — **not** an average of per-season percentages.
4. **Precedence**: when several rules match, the rule with the **highest `PlantingCropSeasonAmount`** wins.

## Quality, financial, yield, scale, technology, ESG

- **Quality / Financial / Yield / Scale**: numeric range rules use the aggregated value from the scope and aggregation mode (see table above). Flag-based add-ons (NTRM, mixture, debt) follow the fixed rules in the table.
- **Technology**: sum configured scores for technologies present in the **latest** Technologies-scope season.
- **ESG**: reforestation and native forest percentages from the **latest** ESG-scope season; plus irregularity scores from `EsgIrregularityKpi` for that season.

## Rankings

Rankings between farmers are **disabled** in the prototype UI. Component and total scores are stored. The `Rank` column remains for schema compatibility.

## Segments and export

- Segment assignment compares total score to culture-type `RangeMin` thresholds (highest matching threshold first), respecting `OnlyExclusiveFarmer`. New farmers receive no segment.
- `GET /api/SegmentationSimulations/{id}/export.csv` exports farmers with component scores (no combined yield-and-scale column), segment, `IsNewFarmer`, and non-exclusive flag.
- Detail DTO includes `KpiScopes` and segment distribution for scored farmers.

## Official segmentation and manual changes

Accepting a simulation (`POST .../accept-official`) writes **`FarmerSegmentations`** for the target season, including `SegmentationConfigurationId` from the simulation.

**Segmentation Management** (`GET /api/SegmentationManagement`) lists official segmentations; manual segment changes use **Submit for approval** (`PUT .../crop-seasons/{cropSeasonId}`) and only allow segments defined on the originating configuration.
