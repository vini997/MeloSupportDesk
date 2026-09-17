import { Navigate, Route, Routes } from 'react-router-dom'
import { useAuth } from './auth/useAuth'
import { DashboardPage } from './pages/DashboardPage'
import { LoginPage } from './pages/LoginPage'
import './App.css'

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

  return (
    <Routes>
      <Route
        path="/login"
        element={
          user ? <Navigate to="/dashboard" replace /> : <LoginPage />
        }
      />

      <Route
        path="/dashboard"
        element={
          user ? <DashboardPage /> : <Navigate to="/login" replace />
        }
      />

      <Route
        path="*"
        element={
          <Navigate
            to={user ? '/dashboard' : '/login'}
            replace
          />
        }
      />
    </Routes>
  )
}

export default App