import { useState, type FormEvent } from 'react'
import { createTicket } from '../api/tickets'
import type { Ticket, TicketPriority } from '../types/ticket'

interface CreateTicketModalProps {
  open: boolean
  onClose: () => void
  onCreated: (ticket: Ticket) => void
}

export function CreateTicketModal({
  open,
  onClose,
  onCreated,
}: CreateTicketModalProps) {
  const [title, setTitle] = useState('')
  const [description, setDescription] = useState('')
  const [category, setCategory] = useState('')
  const [priority, setPriority] = useState<TicketPriority>('Medium')
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')

  if (!open) {
    return null
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setSubmitting(true)
    setError('')

    try {
      const ticket = await createTicket({
        title,
        description,
        category,
        priority,
      })

      setTitle('')
      setDescription('')
      setCategory('')
      setPriority('Medium')
      onCreated(ticket)
    } catch {
      setError('Unable to create the ticket. Please try again.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="modal-backdrop" role="presentation">
      <section
        className="ticket-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="create-ticket-title"
      >
        <div className="modal-header">
          <div>
            <p className="eyebrow">New request</p>
            <h2 id="create-ticket-title">Create ticket</h2>
          </div>

          <button
            className="modal-close"
            type="button"
            onClick={onClose}
            disabled={submitting}
            aria-label="Close"
          >
            ×
          </button>
        </div>

        <form className="ticket-form" onSubmit={handleSubmit}>
          <label>
            Title
            <input
              type="text"
              value={title}
              onChange={(event) => setTitle(event.target.value)}
              placeholder="Briefly describe the issue"
              required
            />
          </label>

          <label>
            Description
            <textarea
              value={description}
              onChange={(event) => setDescription(event.target.value)}
              placeholder="Provide more information about the request"
              rows={5}
              required
            />
          </label>

          <div className="form-row">
            <label>
              Category
              <input
                type="text"
                value={category}
                onChange={(event) => setCategory(event.target.value)}
                placeholder="Example: Email"
                required
              />
            </label>

            <label>
              Priority
              <select
                value={priority}
                onChange={(event) =>
                  setPriority(event.target.value as TicketPriority)
                }
              >
                <option value="Low">Low</option>
                <option value="Medium">Medium</option>
                <option value="High">High</option>
              </select>
            </label>
          </div>

          {error && <div className="form-error">{error}</div>}

          <div className="modal-actions">
            <button
              className="secondary-button"
              type="button"
              onClick={onClose}
              disabled={submitting}
            >
              Cancel
            </button>

            <button type="submit" disabled={submitting}>
              {submitting ? 'Creating...' : 'Create ticket'}
            </button>
          </div>
        </form>
      </section>
    </div>
  )
}
