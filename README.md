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
| Crop seasons | GET | `/CropSeasons` | List seasons (ids are explicit years, e.g. 2024–2027) |
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
| Simulations | GET | `/SegmentationSimulations/{id}` | Detail + per-farmer scores, segment assignment, rank |
| Simulations | POST | `/SegmentationSimulations` | Body: `{ "segmentationConfigurationId", "cropSeasonId" }` — scores **all** farmers from KPI history using **only calendar seasons defined in the configuration**; `cropSeasonId` is stored on the simulation and used when accepting as official for `FarmerSegmentation` for that season; status **`S`** |
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

On create/update the API derives each KPI block’s **`MaxScore`** from its rules, then requires:

**`Loyalty.MaxScore + Quality.MaxScore + Financial.MaxScore + Technology.MaxScore + Esg.MaxScore + Yield.MaxScore + Scale.MaxScore === MaximumScore`**

Derived rules (server-side, see `Segmentation.Domain/SegmentationConfigurationKpiMaxScores.cs`):

- **Loyalty:** max positive `Score` among season-quantity ranges **plus** max positive `Score` among historical volume ranges (two independent maxima, then summed).
- **Quality / Financial / Yield / Scale:** max `Score` among their range rows (or 0 if no ranges).
- **Technology:** sum of the three technology **scores** where the configured value is **positive** (zero or negative contributions are ignored in the sum).
- **ESG:** `ReforestationMaximumScore + NativeForestMaximumScore` (caps for the two percentage ladders; irregularity scores are not part of this derived cap in the current model).

### Simulation scoring (high level)

When you **POST** a simulation, the engine loads the configuration and each farmer’s KPI history **across all seasons present in the database**. **Scoring does not use the simulation’s `cropSeasonId` as a math anchor**: each rule row carries its own anchor (e.g. `CropSeasonStart` on loyalty / IQS / self-funding / yield / scale ranges). Technology and ESG read the farmer’s KPI row at the **crop season id stored on each configuration field** (e.g. `HasLargeBaseRidgeWithMulchCropSeason`, `ReforestationCropSeason`). The simulation’s **`cropSeasonId`** is only persisted with the run and used when you **accept as official** to attach `FarmerSegmentation` rows to that season. See `Segmentation.Domain/Scoring/SimulationFarmerScoring.cs`.

- **Total score** is the sum of the seven KPI component scores stored on `SegmentationSimulationFarmer`.
- **Rank** uses **competition ranking**: `rank = 1 +` count of farmers with **strictly greater** total score (ties share the same rank, e.g. 1, 1, 1, 4).
- **Segment** is chosen from configuration segments using **`RangeMin`** (highest threshold not exceeding total score, respecting **`OnlyExclusiveFarmer`**), then segments without `RangeMin` as fallback.

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
