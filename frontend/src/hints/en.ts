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
    'A segmentation configuration bundles segment thresholds with scoring rules per KPI. You set the culture-type maximum score and each KPI maximum score manually; relevance % is read-only (KPI max ÷ culture max). Save is allowed only when KPI max scores sum to the culture maximum and each KPI max matches the cap implied by its rules (sum of positive technology scores, range maxima, etc.).',

  homeSimulations:
    'A simulation applies one configuration to every farmer for a target crop season. Multi-season KPIs (Loyalty, Yield & Scale) use the scope season list. Farmers without Loyalty and Yield & Scale data in the season before the target are treated as new (score 0, no segment). Results can be exported as CSV; segment share charts exclude new farmers.',

  farmersList:
    'All farmers are listed. Official total, rank, and segment (if any) refer to the accepted segmentation for the selected crop season—not the latest simulation unless you have accepted it.',

  farmerDetail:
    'Official segmentation shows the persisted snapshot after you accepted a simulation for this season. KPI cards show raw facts for the same season when data exists.',

  kpisOverview:
    'Each tab loads read-only rows for the active crop season. Use Import CSV to upsert; invalid farmer codes or unknown seasons appear in the import error list.',

  csvLoyalty:
    'Columns: FarmerCode, CropSeasonId, CultureTypeCode, DeliveredPercentage, DeliveredAmountKg, ContractedAmountKg. One row per farmer per culture type per season.',

  csvQuality:
    'Columns: FarmerCode, CropSeasonId, CultureTypeCode, Iqs, HadNtrm, HadQualityMixture (booleans).',

  csvFinancial:
    'Columns: FarmerCode, CropSeasonId, CultureTypeCode, SelfFundingPercentage, HaveDebt.',

  csvYieldAndScale:
    'Columns: FarmerCode, CropSeasonId, CultureTypeCode, Yield, Scale, ContractedAmountKg.',

  csvTechnologies:
    'Columns: FarmerCode, CropSeasonId, CultureTypeCode, TechnologyId. One row per technology adopted that season (catalog at GET /api/ReferenceData/technologies).',

  csvEsg:
    'Columns: FarmerCode, CropSeasonId, CultureTypeCode, ReforestationPercentage, NativeForestPercentage.',

  csvEsgIrregularities:
    'Columns: FarmerCode, CropSeasonId, CultureTypeCode, IrregularityTypeId (catalog at GET /api/ReferenceData/irregularity-types).',

  configMaximumScore:
    'Culture-type maximum score is the segmentation total you want (e.g. 100). Each KPI has its own configured maximum; their sum must equal this value before save. The “Derived from rules” column shows what your current rules allow; it must match the configured KPI max (mismatch only blocks save, never blocks editing).',

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
    'Pick technologies from the catalog and assign a score for each. At simulation time, the latest scope season’s Technologies KPI rows are matched; points for each configured technology present are summed. Technology KPI maximum must equal the sum of all positive technology scores in this block.',

  esg:
    'Reforestation and native forest percentages come from the ESG KPI row at the latest scope season (capped by your per-point rules). Irregularity scores use ESG irregularity KPI rows for that season. ESG KPI maximum must equal reforestation max + native forest max + sum of positive irregularity scores.',

  yieldAndScale:
    'Uses YieldAndScale KPI rows across all scope seasons. Consolidated delivered % for loyalty is sum(delivered kg)/sum(contracted kg); here average module is the mean of scale across seasons with data, and consolidated yield is sum(contracted kg)/sum(scale). Cap is the largest positive matching range score.',

  simulationCreate:
    'Target season (header) is stored on the run and used when accepting as official. Scope seasons drive Loyalty and Yield & Scale. Other KPIs read only the latest year in the scope list.',

  simulationNewFarmer:
    'Before scoring, the engine checks the crop season immediately before the simulation target (target year minus 1). If the farmer has no Loyalty KPI and no Yield & Scale KPI for that prior season for their culture type, they are treated as a new farmer: total score 0, no segment, excluded from segment distribution charts. Scope seasons do not change this gate.',

  simulationNewFarmerShort:
    'No contract/KPI baseline in the season before the target → score 0 and no segment (new farmer).',

  simulationRank:
    'Rank uses competition ranking: 1 plus the number of farmers with strictly higher total score. Equal totals share the same rank (e.g. 1,1,1,4).',

  simulationAccept:
    'Accepting marks this run as official (O) for its crop season, demotes any other official run for that season back to S, deletes existing FarmerSegmentation rows for the season, and inserts new rows from this simulation. This cannot be undone from the UI—run another simulation and accept it if you need to change official results.',

  simulationSegment:
    'Segment assignment walks segment Range min from highest to lowest; the first segment the farmer qualifies for wins. Farmers flagged non-exclusive skip segments marked Only exclusive farmer. Segments without Range min are tried after all thresholds. New farmers never receive a segment.',

  simulationExportCsv:
    'Downloads all farmers in this run including component scores, segment name, new-farmer flag, and non-exclusive flag.',
} as const
