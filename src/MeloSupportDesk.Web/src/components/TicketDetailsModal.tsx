import { useEffect, useState } from 'react'
import {
  assignTicket,
  updateTicketStatus,
} from '../api/tickets'
import {
  getTechnicians,
  type Technician,
} from '../api/users'
import type {
  Ticket,
  TicketStatus,
} from '../types/ticket'

interface TicketDetailsModalProps {
  ticket: Ticket
  onClose: () => void
  onUpdated: (ticket: Ticket) => void
}

export function TicketDetailsModal({
  ticket,
  onClose,
  onUpdated,
}: TicketDetailsModalProps) {
  const [technicians, setTechnicians] = useState<Technician[]>([])
  const [technicianId, setTechnicianId] = useState(
    ticket.assignedToUserId ?? '',
  )
  const [status, setStatus] = useState<TicketStatus>(ticket.status)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    async function loadTechnicians() {
      try {
        const data = await getTechnicians()
        setTechnicians(data)
      } catch {
        setError('Unable to load technicians.')
      }
    }

    void loadTechnicians()
  }, [])

  async function handleAssign() {
    if (!technicianId) {
      setError('Select a technician.')
      return
    }

    try {
      setSaving(true)
      setError('')

      const updatedTicket = await assignTicket(
        ticket.id,
        technicianId,
      )

      setStatus(updatedTicket.status)
      onUpdated(updatedTicket)
    } catch {
      setError('Unable to assign this ticket.')
    } finally {
      setSaving(false)
    }
  }

  async function handleStatusUpdate() {
    try {
      setSaving(true)
      setError('')

      const updatedTicket = await updateTicketStatus(
        ticket.id,
        status,
      )

      onUpdated(updatedTicket)
    } catch {
      setError('Unable to update the ticket status.')
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="modal-backdrop" onMouseDown={onClose}>
      <section
        className="ticket-details-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="ticket-details-title"
        onMouseDown={(event) => event.stopPropagation()}
      >
        <header className="modal-header">
          <div>
            <p className="eyebrow">Ticket details</p>
            <h2 id="ticket-details-title">{ticket.title}</h2>
            <span>{ticket.ticketNumber}</span>
          </div>

          <button
            className="modal-close"
            type="button"
            aria-label="Close"
            onClick={onClose}
          >
            ×
          </button>
        </header>

        <div className="ticket-details-grid">
          <div>
            <span>Category</span>
            <strong>{ticket.category}</strong>
          </div>

          <div>
            <span>Priority</span>
            <strong>{ticket.priority}</strong>
          </div>

          <div>
            <span>Created by</span>
            <strong>{ticket.createdByUserName}</strong>
          </div>

          <div>
            <span>Assigned to</span>
            <strong>
              {ticket.assignedToUserName ?? 'Unassigned'}
            </strong>
          </div>
        </div>

        <div className="ticket-description">
          <span>Description</span>
          <p>{ticket.description}</p>
        </div>

        <div className="ticket-management">
          <label>
            Technician
            <select
              value={technicianId}
              disabled={saving || ticket.status === 'Closed'}
              onChange={(event) =>
                setTechnicianId(event.target.value)
              }
            >
              <option value="">Select technician</option>

              {technicians.map((technician) => (
                <option
                  key={technician.id}
                  value={technician.id}
                >
                  {technician.fullName}
                </option>
              ))}
            </select>
          </label>

          <button
            className="secondary-button"
            type="button"
            disabled={saving || ticket.status === 'Closed'}
            onClick={handleAssign}
          >
            Assign
          </button>

          <label>
            Status
            <select
              value={status}
              disabled={saving}
              onChange={(event) =>
                setStatus(event.target.value as TicketStatus)
              }
            >
              <option value="Open">Open</option>
              <option value="InProgress">In progress</option>
              <option value="Resolved">Resolved</option>
              <option value="Closed">Closed</option>
            </select>
          </label>

          <button
            type="button"
            disabled={saving || status === ticket.status}
            onClick={handleStatusUpdate}
          >
            {saving ? 'Saving...' : 'Update status'}
          </button>
        </div>

        {error && (
          <p className="tickets-message tickets-error">{error}</p>
        )}
      </section>
    </div>
  )
}
