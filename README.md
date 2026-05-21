# Segmentation prototype

Prototype for **farmer segmentation by KPIs**: layered **ASP.NET Core** backend with **SQLite** and **EF Core**, plus a **React (TypeScript)** web UI. Product copy in the repository (UI labels, API messages, README) is in **English**.

## Repository layout

| Path | Description |
|------|-------------|
| `backend/Segmentation.sln` | .NET solution (Domain, Application, Infrastructure, Api) |
| `frontend/` | Vite + React + TypeScript SPA |

## Prerequisites

- [.NET SDK 10](https://dotnet.microsoft.com/download) (or the SDK version targeted by the projects)
- [Node.js 20+](https://nodejs.org/) and npm

## Backend

### Run

```bash
cd backend
dotnet run --project Segmentation.Api --launch-profile http
```

Default HTTP URL: **http://localhost:5130** (see `Segmentation.Api/Properties/launchSettings.json`).

On startup the API applies EF Core migrations, seeds reference data when the database is empty, and serves JSON with **camelCase** property names.

### Database

SQLite file: `backend/Segmentation.Api/segmentation.db` (ignored by git). Delete it to reset data (migrations re-run on next start).

### Health

- `GET /health` — liveness payload `{ "status": "healthy" }`

### CORS

Development CORS allows the Vite dev server origins `http://localhost:5173` and `http://localhost:3000`.

### Main API surface

Base path: `/api`

| Area | Method | Route | Notes |
|------|--------|-------|--------|
| Crop seasons | GET | `/CropSeasons` | List seasons (ids are explicit years, e.g. 2023–2027) |
| Farmers | GET | `/Farmers?cropSeasonId=` | List all farmers with optional official segmentation for the season |
| Farmers | GET | `/Farmers/{farmerId}?cropSeasonId=` | Detail + KPIs for season + official segmentation if any |
| Farmers | GET | `/Farmers/by-code/{farmerCode}?cropSeasonId=` | Same as above by farmer code |
| KPIs | GET | `/Kpis/{loyalty\|quality\|financial\|yield\|scale\|technologies\|esg}?cropSeasonId=` | Rows for that KPI type |
| KPIs | POST | `/Kpis/{...}/import` | `multipart/form-data` field **`file`**: CSV upsert by `FarmerCode` + `CropSeasonId` |
| Configurations | GET | `/SegmentationConfigurations` | List |
| Configurations | GET | `/SegmentationConfigurations/{id}` | Full graph (segments + all KPI blocks) |
| Configurations | POST | `/SegmentationConfigurations` | Create (body: full save DTO) |
| Configurations | PUT | `/SegmentationConfigurations/{id}` | Replace graph; existing **segments must include `id`** in the payload (new segments may omit `id`) |
| Configurations | POST | `/SegmentationConfigurations/{id}/duplicate` | Optional body `{ "name": "..." }`; copies rules under a new id |
| Simulations | GET | `/SegmentationSimulations?cropSeasonId=` | Optional filter |
| Simulations | GET | `/SegmentationSimulations/{id}` | Detail + per-farmer scores, segment assignment, culture type used |
| Simulations | POST | `/SegmentationSimulations` | Body: `{ "segmentationConfigurationId", "cropSeasonId", "scopeCropSeasonIds": [2026,2025,...] }` — `cropSeasonId` is the **target** season (official snapshot + point-in-time KPIs); `scopeCropSeasonIds` is the scoring window for multi-season rules; status **`S`** |
| Simulations | POST | `/SegmentationSimulations/{id}/accept-official` | Transaction: marks simulation **`O`**, demotes other **`O`** for the same season to **`S`**, replaces `FarmerSegmentation` rows for that season |

### CSV import columns

Headers are matched case-insensitively; underscores and spaces are stripped (e.g. `FarmerCode` or `farmer_code`).

| KPI | Required columns |
|-----|------------------|
| Loyalty | `FarmerCode`, `CropSeasonId`, `DeliveredPercentage` |
| Quality | `FarmerCode`, `CropSeasonId`, `Iqs`, `HadNtrm`, `HadQualityMixture` |
| Financial | `FarmerCode`, `CropSeasonId`, `SelfFundingPercentage`, `HaveDebt` |
| Yield | `FarmerCode`, `CropSeasonId`, `Yield` |
| Scale | `FarmerCode`, `CropSeasonId`, `Scale` |
| Technologies | `FarmerCode`, `CropSeasonId`, `HasLargeBaseRidgeWithMulch`, `HasBroadGrateFurnace`, `HasTechnologyPackageAdherence` |
| ESG | `FarmerCode`, `CropSeasonId`, `ReforestationPercentage`, `NativeForestPercentage`, `HasMinorIrregularity`, `HasMajorIrregularity` |

Boolean values accept common true/false spellings.

Import response: `totalRows`, `insertedRows`, `updatedRows`, `errors[]` with `rowNumber` and `message`.

### Segmentation configuration validation

On create/update the API derives each KPI block’s **`MaxScore`** per **culture type** and requires, for every `SegmentationConfigurationCultureType`:

**`Loyalty.MaxScore + … + Scale.MaxScore + YieldAndScale.MaxScore === CultureType.MaximumScore`**

Configurations are **crop-season-agnostic** (no `CropSeasonStart` / skipped seasons on rules). See `docs/SIMULATION_SCORING.md`.

Derived rules (server-side, see `Segmentation.Domain/SegmentationConfigurationKpiMaxScores.cs`):

- **Loyalty:** max positive `Score` among season-quantity ranges **plus** max positive `Score` among historical volume ranges (two independent maxima, then summed).
- **Quality / Financial / Yield / Scale:** max `Score` among their range rows (or 0 if no ranges).
- **Yield & Scale (optional combined block):** max **positive** `Score` among combined ranges (farmer matches at most one range).
- **Technology:** sum of the three technology **scores** where the configured value is **positive** (zero or negative contributions are ignored in the sum).
- **ESG:** `ReforestationMaximumScore + NativeForestMaximumScore` (caps for the two percentage ladders; irregularity scores are not part of this derived cap in the current model).

### Simulation scoring (high level)

See **`docs/SIMULATION_SCORING.md`** for full rules (scope seasons, loyalty precedence, culture-type choice).

- One **header** configuration holds shared segment definitions; each **culture type** has its own KPI rules, `MaximumScore`, and per-segment **`RangeMin`** thresholds.
- **POST** body includes **`scopeCropSeasonIds`** (multi-season window) and **`cropSeasonId`** (target season for official snapshot and point-in-time KPIs).
- Each farmer is scored with the culture type that has the **largest total Scale** in scope seasons (among types configured).
- **No ranking** between farmers (scores are still stored). **Segment** uses culture-type `RangeMin` thresholds.

### OpenAPI

In Development, OpenAPI is mapped (see `Program.cs` for the exact endpoint).

## Frontend

The SPA is optimised for **discoverability**: Radix UI **tooltips** on headers, KPI import, configuration fields (segments, windows, skipped seasons, technology thresholds, ESG caps), and simulation actions explain how rules relate to the backend engine.

### Run

```bash
cd frontend
npm install
npm run dev
```

Default: **http://localhost:5173**. The dev server proxies `/api` and `/health` to the backend (`vite.config.ts`); start the API on **http://localhost:5130** or set `VITE_API_PROXY_TARGET`.

### Environment

Optional `frontend/.env` (see `frontend/.env.example`):

```env
VITE_API_PROXY_TARGET=http://localhost:5130
```

### Production build

```bash
cd frontend
npm run build
```

Static output is written to `frontend/dist/` (serve behind your gateway or `vite preview`).

## License

Internal prototype — add a license if you open-source the project.
