export type UserRole = 'Admin' | 'Technician' | 'Customer'

export interface LoginRequest {
  email: string
  password: string
}

export interface LoginResponse {
  token: string
}

export interface CurrentUser {
  id: string
  fullName: string
  email: string
  role: UserRole
}