import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { AppLayout } from './components/AppLayout'
import { CropSeasonProvider } from './context/CropSeasonContext'
import { HomePage } from './pages/HomePage'
import { FarmersPage } from './pages/FarmersPage'
import { FarmerDetailPage } from './pages/FarmerDetailPage'
import { KpisPage } from './pages/KpisPage'
import { ConfigurationsPage } from './pages/ConfigurationsPage'
import { ConfigurationEditorPage } from './pages/ConfigurationEditorPage'
import { SimulationsPage } from './pages/SimulationsPage'
import { SimulationDetailPage } from './pages/SimulationDetailPage'

export default function App() {
  return (
    <BrowserRouter>
      <CropSeasonProvider>
        <Routes>
          <Route element={<AppLayout />}>
            <Route index element={<HomePage />} />
            <Route path="farmers" element={<FarmersPage />} />
            <Route path="farmers/:farmerId" element={<FarmerDetailPage />} />
            <Route path="kpis" element={<KpisPage />} />
            <Route path="configurations" element={<ConfigurationsPage />} />
            <Route path="configurations/:id" element={<ConfigurationEditorPage />} />
            <Route path="simulations" element={<SimulationsPage />} />
            <Route path="simulations/:id" element={<SimulationDetailPage />} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Route>
        </Routes>
      </CropSeasonProvider>
    </BrowserRouter>
  )
}
