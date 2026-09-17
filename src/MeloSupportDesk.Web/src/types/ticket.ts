export type TicketStatus =
  | 'Open'
  | 'InProgress'
  | 'Resolved'
  | 'Closed'

export type TicketPriority =
  | 'Low'
  | 'Medium'
  | 'High'

export interface Ticket {
  id: string
  ticketNumber: string
  title: string
  description: string
  category: string
  priority: TicketPriority
  status: TicketStatus
  createdByUserId: string
  createdByUserName: string
  assignedToUserId: string | null
  assignedToUserName: string | null
  createdAtUtc: string
  updatedAtUtc: string
  resolvedAtUtc: string | null
  closedAtUtc: string | null
}

export interface PaginatedTickets {
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  items: Ticket[]
}
