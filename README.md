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
| Farmers | GET | `/Farmers/{farmerId}?cropSeasonId=` | Detail + contract KPIs for season + official segmentation if any |
| Farmers | GET | `/Farmers/by-code/{farmerCode}?cropSeasonId=` | Same as above by farmer code |
| KPIs | GET | `/Kpis/farmer-contract?cropSeasonId=` | Unified contract KPI rows |
| KPIs | GET | `/Kpis/technologies?cropSeasonId=` | Technology rows (one per technology) |
| KPIs | GET | `/Kpis/esg-irregularities?cropSeasonId=` | ESG irregularity rows |
| KPIs | POST | `/Kpis/farmer-contract/import` | CSV upsert into `FarmerContractKpis` |
| KPIs | POST | `/Kpis/technologies/import` | CSV upsert |
| KPIs | POST | `/Kpis/esg-irregularities/import` | CSV upsert |
| Configurations | GET | `/SegmentationConfigurations` | List |
| Configurations | GET | `/SegmentationConfigurations/{id}` | Full graph (segments + KPI blocks per culture type) |
| Configurations | POST | `/SegmentationConfigurations` | Create (body: full save DTO) |
| Configurations | PUT | `/SegmentationConfigurations/{id}` | Replace graph; existing **segments must include `id`** in the payload (new segments may omit `id`) |
| Configurations | POST | `/SegmentationConfigurations/{id}/duplicate` | Optional body `{ "name": "..." }`; copies rules under a new id |
| Simulations | GET | `/SegmentationSimulations?cropSeasonId=` | Optional filter |
| Simulations | GET | `/SegmentationSimulations/{id}` | Detail + per-farmer scores, segment, culture type, `kpiScopes` |
| Simulations | POST | `/SegmentationSimulations` | Body: `{ "segmentationConfigurationId", "cropSeasonId", "kpiScopes": [{ "kpiKind", "cropSeasonIds", "valueAggregation?" }] }` — status **`S`** |
| Simulations | POST | `/SegmentationSimulations/{id}/accept-official` | Marks simulation **`O`**, replaces `FarmerSegmentation` for that season |
| Segmentation management | GET | `/SegmentationManagement?cropSeasonId=` | Official segmentations + available segments |
| Segmentation management | PUT | `/SegmentationManagement/{farmerId}/crop-seasons/{cropSeasonId}` | Manual segment change (submit for approval) |

### CSV import — farmer contract KPI

Headers are matched case-insensitively; underscores and spaces are stripped.

| Column | Required |
|--------|----------|
| `FarmerCode`, `CropSeasonId` | Yes |
| `CultureTypeCode` | Optional (default `FCV`) |
| `DeliveredPercentage`, `ContractedAmountKg` | Yes |
| `DeliveredAmountKg` | Optional (derived from percentage if omitted) |
| `Iqs`, `HadNtrm`, `HadQualityMixture` | Yes |
| `SelfFundingPercentage`, `HaveDebt` | Yes |
| `Yield`, `Scale` | Yes |
| `ReforestationPercentage`, `NativeForestPercentage` | Yes |

`NonExclusive` is taken from `Farmers.NonExclusiveFarmer` on upsert, not from the CSV.

Technologies and ESG irregularities use separate import endpoints (see previous prototype columns: `TechnologyId`, `IrregularityTypeId`).

Import response: `totalRows`, `insertedRows`, `updatedRows`, `errors[]` with `rowNumber` and `message`.

### Segmentation configuration validation

On create/update the API derives each KPI block’s **`MaxScore`** per **culture type** and requires:

**`Loyalty + Quality + Financial + Technology + ESG + Yield + Scale === MaximumScore`**

Derived rules (server-side, see `Segmentation.Domain/SegmentationConfigurationKpiMaxScores.cs`):

- **Loyalty:** max positive `Score` among season-quantity ranges **plus** max positive `Score` among historical volume ranges.
- **Quality / Financial / Yield / Scale:** max `Score` among their range rows (or 0 if no ranges).
- **Technology:** sum of positive technology **scores** in configuration.
- **ESG:** `ReforestationMaximumScore + NativeForestMaximumScore` (percentage ladders; irregularity scores are separate rule rows).

### Simulation scoring (high level)

See **`docs/SIMULATION_SCORING.md`** for per-KPI scopes, aggregation modes, and worked examples.

- Each simulation stores **per-KPI crop season lists** and optional **Average** / **LastActiveCropData** aggregation.
- Contract KPI facts are read from **`FarmerContractKpis`**; technologies and ESG irregularities from their own tables.
- Culture type for a farmer = largest total **Scale** in the simulation’s **Scale** scope.
- New farmers = no `FarmerContractKpi` in the season before the target season.

### OpenAPI

In Development, OpenAPI is mapped (see `Program.cs` for the exact endpoint).

## Frontend

The SPA is optimised for **discoverability**: Radix UI **tooltips** on headers, KPI import, configuration fields, simulation KPI scope forms, and management actions explain how rules relate to the backend engine.

### Run

```bash
cd frontend
npm install
npm run dev
```

Default: **http://localhost:5173**. The dev server proxies `/api` and `/health` to the backend (`vite.config.ts`); start the API on **http://localhost:5130** or set `VITE_API_PROXY_TARGET`.

### Pages

| Route | Purpose |
|-------|---------|
| `/kpis` | Farmer contract KPI, technologies, ESG irregularities |
| `/configurations` | Segmentation rules (no combined Yield & Scale block) |
| `/simulations` | Run simulations with per-KPI season scopes |
| `/segmentation-management` | View/change official segmentations (submit for approval) |

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
