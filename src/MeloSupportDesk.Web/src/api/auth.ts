import { api } from './client'
import type {
  CurrentUser,
  LoginRequest,
  LoginResponse,
} from '../types/auth'

export async function login(
  credentials: LoginRequest,
): Promise<LoginResponse> {
  const response = await api.post<LoginResponse>(
    '/auth/login',
    credentials,
  )

  return response.data
}

export async function getCurrentUser(): Promise<CurrentUser> {
  const response = await api.get<CurrentUser>('/auth/me')

  return response.data
}