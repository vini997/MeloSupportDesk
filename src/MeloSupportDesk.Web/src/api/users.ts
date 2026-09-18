import { api } from './client'

export interface Technician {
  id: string
  fullName: string
  email: string
  role: 'Technician'
  isActive: boolean
  createdAtUtc: string
}

export async function getTechnicians(): Promise<Technician[]> {
  const response = await api.get<Technician[]>('/users/technicians')

  return response.data
}
