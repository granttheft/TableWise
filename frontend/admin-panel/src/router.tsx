import { Routes, Route, Navigate } from 'react-router-dom'
import { useAuth } from '@/hooks/useAuth'
import { useCurrentTenant } from '@/hooks/useCurrentTenant'

// Layout
import { AppLayout } from '@/components/layout/AppLayout'

// Auth pages
import { LoginPage } from '@/features/auth/LoginPage'
import { RegisterPage } from '@/features/auth/RegisterPage'
import { ForgotPasswordPage } from '@/features/auth/ForgotPasswordPage'
import { ResetPasswordPage } from '@/features/auth/ResetPasswordPage'
import { VerifyEmailPage } from '@/features/auth/VerifyEmailPage'
import { InvitePage } from '@/features/auth/InvitePage'

// Dashboard
import { DashboardPage } from '@/features/dashboard/DashboardPage'

// Feature pages
import { TablesPage } from '@/features/tables/TablesPage'
import { RulesPage } from '@/features/rules/RulesPage'
import { ReservationsPage } from '@/features/reservations/ReservationsPage'
import { CustomersPage } from '@/features/customers/CustomersPage'
import { StaffPage } from '@/features/staff/StaffPage'
import { SettingsPage } from '@/features/settings/SettingsPage'
import { SubscriptionPage } from '@/features/subscription/SubscriptionPage'
import { OnboardingPage } from '@/features/onboarding/OnboardingPage'

interface ProtectedRouteProps {
  children: React.ReactNode
}

function ProtectedRoute({ children }: ProtectedRouteProps) {
  const { isAuthenticated } = useAuth()
  const tenant = useCurrentTenant()

  if (!isAuthenticated) {
    const currentPath = window.location.pathname
    return <Navigate to={`/login?redirect=${encodeURIComponent(currentPath)}`} replace />
  }

  // Plan askıya alınmışsa subscription'a yönlendir
  if (tenant?.isSuspended && window.location.pathname !== '/subscription') {
    return <Navigate to="/subscription" replace />
  }

  return <>{children}</>
}

export function AppRouter() {
  return (
    <Routes>
      {/* Public routes */}
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route path="/forgot-password" element={<ForgotPasswordPage />} />
      <Route path="/reset-password/:token" element={<ResetPasswordPage />} />
      <Route path="/verify-email/:token" element={<VerifyEmailPage />} />
      <Route path="/invite/:token" element={<InvitePage />} />
      <Route path="/onboarding" element={<OnboardingPage />} />

      {/* Protected routes */}
      <Route
        path="/*"
        element={
          <ProtectedRoute>
            <AppLayout />
          </ProtectedRoute>
        }
      >
        <Route index element={<Navigate to="/dashboard" replace />} />
        <Route path="dashboard" element={<DashboardPage />} />
        <Route path="tables" element={<TablesPage />} />
        <Route path="rules" element={<RulesPage />} />
        <Route path="reservations" element={<ReservationsPage />} />
        <Route path="customers" element={<CustomersPage />} />
        <Route path="staff" element={<StaffPage />} />
        <Route path="settings" element={<SettingsPage />} />
        <Route path="subscription" element={<SubscriptionPage />} />
      </Route>

      {/* 404 */}
      <Route path="*" element={<Navigate to="/dashboard" replace />} />
    </Routes>
  )
}
