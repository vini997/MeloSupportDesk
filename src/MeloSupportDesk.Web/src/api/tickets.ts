import { api } from './client'
import type {
  PaginatedTickets,
  Ticket,
  TicketPriority,
  TicketStatus,
} from '../types/ticket'

export interface TicketQuery {
  page?: number
  pageSize?: number
  status?: TicketStatus
  priority?: TicketPriority
  search?: string
}

export interface CreateTicketInput {
  title: string
  description: string
  category: string
  priority: TicketPriority
}

export async function getAdminTickets(
  query: TicketQuery = {},
): Promise<PaginatedTickets> {
  const response = await api.get<PaginatedTickets>('/admin/tickets', {
    params: {
      page: query.page ?? 1,
      pageSize: query.pageSize ?? 10,
      status: query.status,
      priority: query.priority,
      search: query.search,
    },
  })

  return response.data
}

export async function createTicket(
  input: CreateTicketInput,
): Promise<Ticket> {
  const response = await api.post<Ticket>('/tickets', input)
  return response.data
}

export async function assignTicket(
  ticketId: string,
  technicianId: string,
): Promise<Ticket> {
  const response = await api.patch<Ticket>(
    `/tickets/${ticketId}/assign`,
    { technicianId },
  )

  return response.data
}

export async function updateTicketStatus(
  ticketId: string,
  status: TicketStatus,
): Promise<Ticket> {
  const response = await api.patch<Ticket>(
    `/tickets/${ticketId}/status`,
    { status },
  )

  return response.data
}
export async function getAssignedTickets(): Promise<Ticket[]> {
  const response = await api.get<Ticket[]>('/tickets/assigned')
  return response.data
}
