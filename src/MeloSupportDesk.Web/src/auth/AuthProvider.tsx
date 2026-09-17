import { useEffect, useState } from 'react'
import type { ReactNode } from 'react'
import { getCurrentUser, login } from '../api/auth'
import type { CurrentUser, LoginRequest } from '../types/auth'
import { AuthContext } from './AuthContext'

interface AuthProviderProps {
  children: ReactNode
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<CurrentUser | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    async function restoreSession() {
      const token = localStorage.getItem('accessToken')

      if (!token) {
        setIsLoading(false)
        return
      }

      try {
        const currentUser = await getCurrentUser()
        setUser(currentUser)
      } catch {
        localStorage.removeItem('accessToken')
      } finally {
        setIsLoading(false)
      }
    }

    void restoreSession()
  }, [])

  async function signIn(credentials: LoginRequest) {
    const response = await login(credentials)

    localStorage.setItem('accessToken', response.token)

    const currentUser = await getCurrentUser()
    setUser(currentUser)
  }

  function signOut() {
    localStorage.removeItem('accessToken')
    setUser(null)
  }

  return (
    <AuthContext.Provider
      value={{
        user,
        isLoading,
        signIn,
        signOut,
      }}
    >
      {children}
    </AuthContext.Provider>
  )
}