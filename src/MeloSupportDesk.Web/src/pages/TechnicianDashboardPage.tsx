import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { getAssignedTickets, updateTicketStatus } from '../api/tickets'
import { useAuth } from '../auth/useAuth'
import type { Ticket, TicketStatus } from '../types/ticket'

export function TechnicianDashboardPage() {
  const navigate = useNavigate()
  const { user, signOut } = useAuth()
  const [tickets, setTickets] = useState<Ticket[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    async function loadTickets() {
      try {
        const assignedTickets = await getAssignedTickets()
        setTickets(assignedTickets)
      } catch {
        setError('Unable to load your assigned tickets.')
      } finally {
        setLoading(false)
      }
    }

    void loadTickets()
  }, [])

  function handleSignOut() {
    signOut()
    navigate('/login')
  }

  async function handleStatusChange(
    ticket: Ticket,
    status: TicketStatus,
  ) {
    try {
      const updatedTicket = await updateTicketStatus(ticket.id, status)

      setTickets((currentTickets) =>
        currentTickets.map((currentTicket) =>
          currentTicket.id === updatedTicket.id
            ? updatedTicket
            : currentTicket,
        ),
      )
    } catch {
      setError('Unable to update the ticket status.')
    }
  }

  const inProgress = tickets.filter(
    (ticket) => ticket.status === 'InProgress',
  ).length

  const resolved = tickets.filter(
    (ticket) => ticket.status === 'Resolved',
  ).length

  return (
    <main className="dashboard-page">
      <header className="dashboard-header">
        <div className="dashboard-brand">
          <div className="brand-mark brand-mark-small">M</div>

          <div>
            <strong>Melo Support Desk</strong>
            <span>Technician workspace</span>
          </div>
        </div>

        <div className="user-menu">
          <div>
            <strong>{user?.fullName}</strong>
            <span>{user?.role}</span>
          </div>

          <button
            className="secondary-button"
            type="button"
            onClick={handleSignOut}
          >
            Sign out
          </button>
        </div>
      </header>

      <section className="dashboard-content">
        <div className="welcome-row">
          <div>
            <p className="eyebrow">Technician dashboard</p>
            <h1>
              Welcome, {user?.fullName?.split(' ')[0]}.
            </h1>
            <p>Manage the support tickets assigned to you.</p>
          </div>
        </div>

        <div className="metric-grid">
          <article className="metric-card">
            <span>Assigned tickets</span>
            <strong>{loading ? '–' : tickets.length}</strong>
            <p>Your current workload</p>
          </article>

          <article className="metric-card">
            <span>In progress</span>
            <strong>{loading ? '–' : inProgress}</strong>
            <p>Currently being handled</p>
          </article>

          <article className="metric-card">
            <span>Resolved</span>
            <strong>{loading ? '–' : resolved}</strong>
            <p>Successfully completed</p>
          </article>
        </div>

        <section className="tickets-panel">
          <div className="tickets-panel-header">
            <div>
              <p className="eyebrow">My workload</p>
              <h2>Assigned tickets</h2>
            </div>
          </div>

          {loading && (
            <p className="tickets-message">Loading tickets...</p>
          )}

          {!loading && error && (
            <p className="tickets-message tickets-error">{error}</p>
          )}

          {!loading && !error && tickets.length === 0 && (
            <p className="tickets-message">
              No tickets are currently assigned to you.
            </p>
          )}

          {!loading && !error && tickets.length > 0 && (
            <div className="tickets-table-wrapper">
              <table className="tickets-table">
                <thead>
                  <tr>
                    <th>Ticket</th>
                    <th>Title</th>
                    <th>Priority</th>
                    <th>Customer</th>
                    <th>Status</th>
                  </tr>
                </thead>

                <tbody>
                  {tickets.map((ticket) => (
                    <tr key={ticket.id}>
                      <td>{ticket.ticketNumber}</td>
                      <td>{ticket.title}</td>
                      <td>{ticket.priority}</td>
                      <td>{ticket.createdByUserName}</td>
                      <td>
                        <select
                          value={ticket.status}
                          onChange={(event) =>
                            void handleStatusChange(
                              ticket,
                              event.target.value as TicketStatus,
                            )
                          }
                        >
                          <option value="InProgress">In progress</option>
                          <option value="Resolved">Resolved</option>
                        </select>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </section>
      </section>
    </main>
  )
}

