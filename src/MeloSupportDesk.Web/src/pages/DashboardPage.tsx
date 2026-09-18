import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { getAdminTickets } from '../api/tickets'
import { useAuth } from '../auth/useAuth'
import { CreateTicketModal } from '../components/CreateTicketModal'
import { RecentTickets } from '../components/RecentTickets'
import type { Ticket } from '../types/ticket'
import { TicketDetailsModal } from '../components/TicketDetailsModal'

interface TicketMetrics {
  open: number
  inProgress: number
  resolved: number
}

export function DashboardPage() {
  const navigate = useNavigate()
  const { user, signOut } = useAuth()

  const [metrics, setMetrics] = useState<TicketMetrics>({
    open: 0,
    inProgress: 0,
    resolved: 0,
  })
  const [tickets, setTickets] = useState<Ticket[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [createModalOpen, setCreateModalOpen] = useState(false)
const [selectedTicket, setSelectedTicket] =
  useState<Ticket | null>(null)

  useEffect(() => {
    let active = true

    async function loadDashboard() {
      try {
        const [open, inProgress, resolved, recent] = await Promise.all([
          getAdminTickets({ status: 'Open', pageSize: 1 }),
          getAdminTickets({ status: 'InProgress', pageSize: 1 }),
          getAdminTickets({ status: 'Resolved', pageSize: 1 }),
          getAdminTickets({ page: 1, pageSize: 5 }),
        ])

        if (active) {
          setMetrics({
            open: open.totalCount,
            inProgress: inProgress.totalCount,
            resolved: resolved.totalCount,
          })
          setTickets(recent.items)
        }
      } catch {
        if (active) {
          setError('Unable to load ticket information.')
        }
      } finally {
        if (active) {
          setLoading(false)
        }
      }
    }

    void loadDashboard()

    return () => {
      active = false
    }
  }, [])

  function handleSignOut() {
    signOut()
    navigate('/login')
  }

  function handleTicketCreated(ticket: Ticket) {
    setTickets((currentTickets) => [
      ticket,
      ...currentTickets.filter(
        (currentTicket) => currentTicket.id !== ticket.id,
      ),
    ].slice(0, 5))

    setMetrics((currentMetrics) => ({
      ...currentMetrics,
      open: currentMetrics.open + 1,
    }))

    setCreateModalOpen(false)
  }
function handleTicketUpdated(updatedTicket: Ticket) {
  const previousStatus = selectedTicket?.status

  setTickets((currentTickets) =>
    currentTickets.map((currentTicket) =>
      currentTicket.id === updatedTicket.id
        ? updatedTicket
        : currentTicket,
    ),
  )

  setSelectedTicket(updatedTicket)

  if (previousStatus === updatedTicket.status) {
    return
  }

  setMetrics((currentMetrics) => {
    const nextMetrics = { ...currentMetrics }

    if (previousStatus === 'Open') {
      nextMetrics.open = Math.max(0, nextMetrics.open - 1)
    }

    if (previousStatus === 'InProgress') {
      nextMetrics.inProgress = Math.max(
        0,
        nextMetrics.inProgress - 1,
      )
    }

    if (previousStatus === 'Resolved') {
      nextMetrics.resolved = Math.max(
        0,
        nextMetrics.resolved - 1,
      )
    }

    if (updatedTicket.status === 'Open') {
      nextMetrics.open += 1
    }

    if (updatedTicket.status === 'InProgress') {
      nextMetrics.inProgress += 1
    }

    if (updatedTicket.status === 'Resolved') {
      nextMetrics.resolved += 1
    }

    return nextMetrics
  })
}
  return (
    <main className="dashboard-page">
      <header className="dashboard-header">
        <div className="dashboard-brand">
          <div className="brand-mark brand-mark-small">M</div>

          <div>
            <strong>Melo Support Desk</strong>
            <span>Support workspace</span>
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
            <p className="eyebrow">Dashboard</p>
            <h1>Welcome, {user?.fullName?.split(' ')[0]}.</h1>
            <p>Here is an overview of your support workspace.</p>
          </div>

          <button
            type="button"
            onClick={() => setCreateModalOpen(true)}
          >
            Create ticket
          </button>
        </div>

        <div className="metric-grid">
          <article className="metric-card">
            <span>Open tickets</span>
            <strong>{loading ? '–' : metrics.open}</strong>
            <p>Waiting for attention</p>
          </article>

          <article className="metric-card">
            <span>In progress</span>
            <strong>{loading ? '–' : metrics.inProgress}</strong>
            <p>Currently being handled</p>
          </article>

          <article className="metric-card">
            <span>Resolved</span>
            <strong>{loading ? '–' : metrics.resolved}</strong>
            <p>Successfully completed</p>
          </article>
        </div>

        <RecentTickets
          tickets={tickets}
          loading={loading}
          error={error}
	  onSelectTicket={setSelectedTicket}
        />
      </section>

      <CreateTicketModal
        open={createModalOpen}
        onClose={() => setCreateModalOpen(false)}
        onCreated={handleTicketCreated}
      />
{selectedTicket && (
  <TicketDetailsModal
    ticket={selectedTicket}
    onClose={() => setSelectedTicket(null)}
    onUpdated={handleTicketUpdated}
  />
)}
    </main>
  )
}
