import { useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'

export function LoginPage() {
  const navigate = useNavigate()
  const { signIn } = useAuth()

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError('')
    setIsSubmitting(true)

    try {
      await signIn({ email, password })
      navigate('/dashboard')
    } catch {
      setError('Invalid email or password.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="login-page">
      <section className="login-brand">
        <div className="brand-mark">M</div>

        <div>
          <p className="eyebrow">Melo Support Desk</p>
          <h1>Support that keeps work moving.</h1>
          <p className="brand-description">
            Manage requests, collaborate with technicians, and give
            customers a clear view of every support ticket.
          </p>
        </div>

        <div className="brand-footer">
          Secure access • Role-based workflow • Ticket history
        </div>
      </section>

      <section className="login-panel">
        <form className="login-card" onSubmit={handleSubmit}>
          <div>
            <p className="eyebrow">Welcome back</p>
            <h2>Sign in to your account</h2>
            <p className="form-description">
              Enter your credentials to access the support workspace.
            </p>
          </div>

          <label>
            Email address
            <input
              type="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              placeholder="name@company.com"
              autoComplete="email"
              required
            />
          </label>

          <label>
            Password
            <input
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              placeholder="Enter your password"
              autoComplete="current-password"
              required
            />
          </label>

          {error && <p className="form-error">{error}</p>}

          <button type="submit" disabled={isSubmitting}>
            {isSubmitting ? 'Signing in...' : 'Sign in'}
          </button>

          <p className="security-note">
            Protected by secure JWT authentication.
          </p>
        </form>
      </section>
    </main>
  )
}