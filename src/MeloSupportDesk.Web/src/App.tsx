import { Navigate, Route, Routes } from 'react-router-dom'
import { useAuth } from './auth/useAuth'
import { DashboardPage } from './pages/DashboardPage'
import { LoginPage } from './pages/LoginPage'
import { TechnicianDashboardPage } from './pages/TechnicianDashboardPage'
import './App.css'

function getHomePath(role?: string) {
  return role === 'Technician' ? '/technician' : '/dashboard'
}

function App() {
  const { user, isLoading } = useAuth()

  if (isLoading) {
    return (
      <main className="loading-page">
        <div className="loading-spinner" />
        <p>Loading your workspace...</p>
      </main>
    )
  }

  const homePath = getHomePath(user?.role)

  return (
    <Routes>
      <Route
        path="/login"
        element={
          user
            ? <Navigate to={homePath} replace />
            : <LoginPage />
        }
      />

      <Route
        path="/dashboard"
        element={
          user?.role === 'Admin'
            ? <DashboardPage />
            : <Navigate to={user ? homePath : '/login'} replace />
        }
      />

      <Route
        path="/technician"
        element={
          user?.role === 'Technician'
            ? <TechnicianDashboardPage />
            : <Navigate to={user ? homePath : '/login'} replace />
        }
      />

      <Route
        path="*"
        element={
          <Navigate
            to={user ? homePath : '/login'}
            replace
          />
        }
      />
    </Routes>
  )
}

export default App
