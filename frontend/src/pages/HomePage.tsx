import { Link } from 'react-router-dom'
import { Hint } from '../components/Hint'
import { hints } from '../hints/en'

export function HomePage() {
  return (
    <div className="mx-auto max-w-3xl space-y-8">
      <div>
        <h1 className="font-display text-3xl font-bold tracking-tight text-ink">
          Farmer segmentation
        </h1>
        <p className="mt-2 flex items-start gap-2 text-ink-muted">
          <span>{hints.appTagline}</span>
          <Hint content={hints.homeWelcome} side="right" />
        </p>
      </div>

      <ol className="space-y-4 rounded-2xl border border-black/5 bg-surface-card p-6 shadow-card">
        <li className="flex gap-3">
          <span className="flex h-7 w-7 shrink-0 items-center justify-center rounded-full bg-leaf-soft text-sm font-bold text-leaf">
            1
          </span>
          <div>
            <div className="flex items-center gap-2 font-medium text-ink">
              Choose crop season
              <Hint content={hints.cropSeason} />
            </div>
            <p className="mt-1 text-sm text-ink-muted">
              Use the header selector. Most API calls need a season id.
            </p>
          </div>
        </li>
        <li className="flex gap-3">
          <span className="flex h-7 w-7 shrink-0 items-center justify-center rounded-full bg-accent-soft text-sm font-bold text-accent">
            2
          </span>
          <div>
            <div className="flex items-center gap-2 font-medium text-ink">
              Review KPI data
              <Hint content={hints.homeKpis} />
            </div>
            <p className="mt-1 text-sm text-ink-muted">
              <Link className="text-leaf underline-offset-2 hover:underline" to="/kpis">
                KPI data
              </Link>{' '}
              — import CSV or inspect rows per type.
            </p>
          </div>
        </li>
        <li className="flex gap-3">
          <span className="flex h-7 w-7 shrink-0 items-center justify-center rounded-full bg-surface-muted text-sm font-bold text-ink-muted">
            3
          </span>
          <div>
            <div className="flex items-center gap-2 font-medium text-ink">
              Configure segmentation
              <Hint content={hints.homeConfigs} />
            </div>
            <p className="mt-1 text-sm text-ink-muted">
              <Link className="text-leaf underline-offset-2 hover:underline" to="/configurations">
                Configurations
              </Link>{' '}
              — define segments and KPI rules; save checks cap totals.
            </p>
          </div>
        </li>
        <li className="flex gap-3">
          <span className="flex h-7 w-7 shrink-0 items-center justify-center rounded-full bg-surface-muted text-sm font-bold text-ink-muted">
            4
          </span>
          <div>
            <div className="flex items-center gap-2 font-medium text-ink">
              Run simulation & accept
              <Hint content={hints.homeSimulations} />
            </div>
            <p className="mt-1 text-sm text-ink-muted">
              <Link className="text-leaf underline-offset-2 hover:underline" to="/simulations">
                Simulations
              </Link>{' '}
              — score all farmers, then accept the official snapshot for the season.
            </p>
          </div>
        </li>
      </ol>
    </div>
  )
}
