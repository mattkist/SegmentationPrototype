import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { useQuery } from '@tanstack/react-query'
import { apiGet } from '../api/client'
import type { CropSeasonDto } from '../api/types'

type CropSeasonContextValue = {
  seasons: CropSeasonDto[]
  seasonId: number | null
  setSeasonId: (id: number) => void
  isLoading: boolean
}

const CropSeasonContext = createContext<CropSeasonContextValue | null>(null)

export function CropSeasonProvider({ children }: { children: ReactNode }) {
  const { data, isLoading } = useQuery({
    queryKey: ['crop-seasons'],
    queryFn: () => apiGet<CropSeasonDto[]>('/api/CropSeasons'),
  })

  const seasons = data ?? []
  const [seasonId, setSeasonIdState] = useState<number | null>(null)

  useEffect(() => {
    if (seasons.length === 0) return
    setSeasonIdState((prev) => {
      if (prev !== null && seasons.some((s) => s.id === prev)) return prev
      return seasons[seasons.length - 1]?.id ?? null
    })
  }, [seasons])

  const value = useMemo(
    () => ({
      seasons,
      seasonId,
      setSeasonId: setSeasonIdState,
      isLoading,
    }),
    [seasons, seasonId, isLoading],
  )

  return <CropSeasonContext.Provider value={value}>{children}</CropSeasonContext.Provider>
}

export function useCropSeason() {
  const ctx = useContext(CropSeasonContext)
  if (!ctx) throw new Error('useCropSeason must be used within CropSeasonProvider')
  return ctx
}
