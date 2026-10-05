import { useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import smartSolarLogo from '@/assets/branding/smart-solar-logo.png'
import { Alert } from '@/components/feedback/Alert'
import { Button } from '@/components/ui/Button'
import { Card } from '@/components/ui/Card'
import { Input } from '@/components/ui/Input'
import { useAuth } from '@/features/auth/hooks/useAuth'
import { toApiClientError, type ApiClientError } from '@/services/api/apiError'

export function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [loginError, setLoginError] = useState<ApiClientError | null>(null)

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (isSubmitting) return

    setLoginError(null)
    setIsSubmitting(true)
    try {
      await login({ email: email.trim(), password })
      navigate('/', { replace: true })
    } catch (error) {
      setLoginError(toApiClientError(error))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="centered-page">
      <Card className="centered-page__panel">
        <div className="centered-page__brand">
          <img className="centered-page__logo" src={smartSolarLogo} alt="Smart Solar" />
          <div><strong>Smart Solar Microgrid Trading System</strong><small>Backoffice and Grid Operator portal</small></div>
        </div>
        <p className="centered-page__eyebrow">Secure Web access</p>
        <h1 className="centered-page__title">Sign in</h1>
        <p className="centered-page__description">Use your Backoffice or Grid Operator account.</p>
        <form className="login-form" onSubmit={handleSubmit}>
          {loginError && <Alert variant="error" title="Sign in failed">{loginError.message}</Alert>}
          <Input
            label="Email"
            type="email"
            name="email"
            autoComplete="email"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            disabled={isSubmitting}
            required
          />
          <Input
            label="Password"
            type="password"
            name="password"
            autoComplete="current-password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            disabled={isSubmitting}
            required
          />
          <Button className="login-form__submit" type="submit" loading={isSubmitting} disabled={isSubmitting}>
            Sign in
          </Button>
        </form>
      </Card>
    </main>
  )
}
