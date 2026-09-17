import { createContext } from 'react'
import type { CurrentUser, LoginRequest } from '../types/auth'

export interface AuthContextValue {
  user: CurrentUser | null
  isLoading: boolean
  signIn: (credentials: LoginRequest) => Promise<void>
  signOut: () => void
}

export const AuthContext = createContext<AuthContextValue | undefined>(
  undefined,
)