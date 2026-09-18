import type { Ticket } from '../types/ticket'

interface RecentTicketsProps {
  tickets: Ticket[]
  loading: boolean
  error: string
  onSelectTicket: (ticket: Ticket) => void
}

function formatStatus(status: Ticket['status']) {
  return status === 'InProgress' ? 'In progress' : status
}

export function RecentTickets({
  tickets,
  loading,
  error,
  onSelectTicket,
}: RecentTicketsProps) {
  return (
    <section className="tickets-panel">
      <div className="tickets-panel-header">
        <div>
          <p className="eyebrow">Recent activity</p>
          <h2>Latest tickets</h2>
        </div>
      </div>

      {loading && <p className="tickets-message">Loading tickets...</p>}

      {!loading && error && (
        <p className="tickets-message tickets-error">{error}</p>
      )}

      {!loading && !error && tickets.length === 0 && (
        <p className="tickets-message">No tickets found.</p>
      )}

      {!loading && !error && tickets.length > 0 && (
        <div className="tickets-table-wrapper">
          <table className="tickets-table">
            <thead>
              <tr>
                <th>Ticket</th>
                <th>Title</th>
                <th>Priority</th>
                <th>Status</th>
                <th>Assigned to</th>
              </tr>
            </thead>

            <tbody>
              {tickets.map((ticket) => (
                <tr
		  key={ticket.id}
 		  className="ticket-row"
		  onClick={() => onSelectTicket(ticket)}
	        >
                  <td>{ticket.ticketNumber}</td>
                  <td>{ticket.title}</td>
                  <td>
                    <span
                      className={`ticket-badge priority-${ticket.priority.toLowerCase()}`}
                    >
                      {ticket.priority}
                    </span>
                  </td>
                  <td>
                    <span
                      className={`ticket-badge status-${ticket.status.toLowerCase()}`}
                    >
                      {formatStatus(ticket.status)}
                    </span>
                  </td>
                  <td>{ticket.assignedToUserName ?? 'Unassigned'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  )
}
