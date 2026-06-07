import { Routes, Route, Navigate } from 'react-router-dom'
import { useAuth } from '@/hooks/useAuth'
import { AppLayout } from '@/components/layout/AppLayout'
import { LoginPage } from '@/features/auth/LoginPage'
import { DashboardPage } from '@/features/dashboard/DashboardPage'
import { TenantsPage } from '@/features/tenants/TenantsPage'
import { TenantDetailPage } from '@/features/tenants/TenantDetailPage'
import { PricingPage } from '@/features/pricing/PricingPage'
import { CouponsPage } from '@/features/coupons/CouponsPage'
import { TeamPage } from '@/features/team/TeamPage'
import { PaymentsPage } from '@/features/payments/PaymentsPage'
import { DevicesPage } from '@/features/devices/DevicesPage'
import type { PlatformRole } from '@/types/api'

interface ProtectedRouteProps {
  children: React.ReactNode
  allowedRoles?: PlatformRole[]
}

function ProtectedRoute({ children, allowedRoles }: ProtectedRouteProps) {
  const { isAuthenticated, user } = useAuth()

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />
  }

  if (allowedRoles && user && !allowedRoles.includes(user.role)) {
    return <Navigate to="/dashboard" replace />
  }

  return <>{children}</>
}

export function AppRouter() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />

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
        <Route path="tenants" element={<TenantsPage />} />
        <Route path="tenants/:id" element={<TenantDetailPage />} />
        <Route
          path="pricing"
          element={
            <ProtectedRoute allowedRoles={['SuperAdmin', 'Finance']}>
              <PricingPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="coupons"
          element={
            <ProtectedRoute allowedRoles={['SuperAdmin', 'Marketing', 'Finance']}>
              <CouponsPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="payments"
          element={
            <ProtectedRoute allowedRoles={['SuperAdmin', 'Finance']}>
              <PaymentsPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="devices"
          element={
            <ProtectedRoute allowedRoles={['SuperAdmin']}>
              <DevicesPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="team"
          element={
            <ProtectedRoute allowedRoles={['SuperAdmin']}>
              <TeamPage />
            </ProtectedRoute>
          }
        />
        <Route path="*" element={<Navigate to="/dashboard" replace />} />
      </Route>
    </Routes>
  )
}
