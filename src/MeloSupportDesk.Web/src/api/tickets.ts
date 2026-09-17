import { api } from './client'
import type {
  PaginatedTickets,
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
