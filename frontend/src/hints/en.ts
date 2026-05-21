/**
 * Long-form UI hints (English) for tooltips and inline guidance.
 * Keep paragraphs readable in narrow tooltip panels (pre-wrap + scroll).
 */

export const hints = {
  appTagline:
    'Prototype console for KPI-driven farmer segmentation: load facts per crop season, define scoring rules, run simulations, then accept one result as the official segmentation for that season.',

  cropSeason:
    'Crop seasons are the time axis for KPI grids and official segmentation snapshots. Season ids in the seed database are years (e.g. 2024–2027). When you create a simulation, the season you pick is stored with the run and used on accept-official; scoring windows use anchors from the configuration, not that picker alone.',

  homeWelcome:
    'Use the sidebar to move between areas. Choose a crop season for KPI tables and official results context; configuration rules carry their own season anchors for scoring.',

  homeKpis:
    'Each KPI type has its own grid for the selected season. You can bulk-load or update values with CSV import; rows are upserted by farmer code and crop season id.',

  homeConfigs:
    'A segmentation configuration bundles segment thresholds (commercial tiers) with scoring rules for Loyalty, Quality, Finance, Technology, ESG, Yield, Scale, and optional combined Yield & Scale. Saving enforces that the sum of derived KPI maximum scores equals the configuration Maximum score.',

  homeSimulations:
    'A simulation applies one configuration to every farmer, stores component scores and a chosen crop season id on the run (for history and accept-official), assigns a segment from total score, and ranks farmers with competition ranking (ties share rank). Rule anchors come from the configuration, not from that stored season. Accepting replaces the official segmentation snapshot for the run’s crop season.',

  farmersList:
    'All farmers are listed. Official total, rank, and segment (if any) refer to the accepted segmentation for the selected crop season—not the latest simulation unless you have accepted it.',

  farmerDetail:
    'Official segmentation shows the persisted snapshot after you accepted a simulation for this season. KPI cards show raw facts for the same season when data exists.',

  kpisOverview:
    'Each tab loads read-only rows for the active crop season. Use Import CSV to upsert; invalid farmer codes or unknown seasons appear in the import error list.',

  csvLoyalty:
    'Columns: FarmerCode, CropSeasonId, DeliveredPercentage (0–100 in the prototype). One row per farmer per season.',

  csvQuality:
    'Columns: FarmerCode, CropSeasonId, Iqs, HadNtrm, HadQualityMixture (booleans).',

  csvFinancial:
    'Columns: FarmerCode, CropSeasonId, SelfFundingPercentage, HaveDebt.',

  csvYield: 'Columns: FarmerCode, CropSeasonId, Yield.',
  csvScale: 'Columns: FarmerCode, CropSeasonId, Scale.',

  csvTechnologies:
    'Columns: FarmerCode, CropSeasonId, CultureTypeCode (optional), HasLargeBaseRidgeWithMulch, HasBroadGrateFurnace, HasTechnologyPackageAdherence, HasStandardBarn (optional, defaults to false).',

  csvEsg:
    'Columns: FarmerCode, CropSeasonId, ReforestationPercentage, NativeForestPercentage, HasMinorIrregularity, HasMajorIrregularity.',

  configMaximumScore:
    'Target total of all KPI caps after rules are interpreted. The server recomputes each block’s MaxScore from your ranges and caps, then requires: Loyalty + Quality + Financial + Technology + ESG + Yield + Scale + Yield & Scale = Maximum score. The panel below shows the live breakdown while you edit.',

  configRelevance:
    'Relevance is **derived max for that KPI ÷ configuration maximum score** (shown as %). While the sum of derived caps equals the maximum score, changing one % **rescales all score fields** in every KPI block so caps stay married to those shares. Simulation totals still use the rescaled rule scores.',

  configSegments:
    'Segments are named tiers (e.g. Diamond, Gold). Range min is the minimum **total simulation score** to qualify; it may be **negative** if your rules produce negative totals. Segments with higher Range min are evaluated first. Empty Range min is a catch-all after other thresholds. OnlyExclusiveFarmer excludes non-exclusive farmers from that tier during assignment.',

  loyaltyHistorical:
    'When you have season-quantity rows, historical volume bands use the delivered % at that row’s Crop season start (the configured anchor). With no season-quantity rows, any season’s delivered % in history can match a historical band. Loyalty KPI cap uses two independent maxima (see server docs).',

  loyaltySeasonQty:
    'Each row’s Crop season start is the anchor year (e.g. 2025). Planting: need enough prior seasons with loyalty data, ending at that anchor (skips excluded). Delivery: last N seasons ending at the same anchor must fall within min/max delivered %. The simulation’s crop season is not used for this math.',

  qualityIqs:
    'Each IQS range uses its own Crop season start as the right end of the look-back window (Crop season amount seasons, skips excluded). NTRM/mixture bonuses read the quality row at the latest IQS anchor among your ranges, compared to Ntrm/Mixture crop season start fields.',

  financialSf:
    'Self-funding ranges use each row’s Crop season start as the window end. Debt uses the self-funding row with the latest configured anchor to read HaveDebt, then compares that anchor to Debt crop season.',

  technology:
    'Each technology field stores the crop season id whose TechnologiesKpi row is read for that lever (mulch, furnace, package). If the flag is true in that row, the score is added. Technology KPI cap on save sums positive configured scores.',

  esg:
    'Reforestation uses the KPI row at Reforestation crop season; native forest uses Native forest crop season; irregularities use their respective crop season rows. Caps apply as configured. ESG KPI cap on save is reforestation max + native max.',

  yieldScale:
    'Each range uses its Crop season start as the right end of the look-back window; every season in the window (except skipped) must fall inside min/max. Best matching row score wins.',

  yieldAndScale:
    'Optional combined KPI using existing Yield and Scale facts (no new KPI import). Counts scope seasons where both values exist, compares planting count, average yield, and average module (hectares) to one range. A farmer matches at most one range; cap is the largest positive range score. Set relevance to 0% here and use separate Yield/Scale tabs, or the opposite.',

  simulationCreate:
    'Creates a new run for all farmers. Scores use only seasons and anchors from the configuration plus KPI history; the crop season you pick is stored on the simulation and used when you accept as official (FarmerSegmentation for that season). Status S until accepted.',

  simulationRank:
    'Rank uses competition ranking: 1 plus the number of farmers with strictly higher total score. Equal totals share the same rank (e.g. 1,1,1,4).',

  simulationAccept:
    'Accepting marks this run as official (O) for its crop season, demotes any other official run for that season back to S, deletes existing FarmerSegmentation rows for the season, and inserts new rows from this simulation. This cannot be undone from the UI—run another simulation and accept it if you need to change official results.',

  simulationSegment:
    'Segment assignment walks segment Range min from highest to lowest; the first segment the farmer qualifies for wins. Farmers flagged non-exclusive skip segments marked Only exclusive farmer. Segments without Range min are tried after all thresholds.',
} as const
