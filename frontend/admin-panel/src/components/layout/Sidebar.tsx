import { NavLink } from 'react-router-dom'
import { useUIStore } from '@/stores/uiStore'
import { useCurrentTenant } from '@/hooks/useCurrentTenant'
import { useAuth } from '@/hooks/useAuth'
import { cn } from '@/lib/cn'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import {
  LayoutDashboard,
  Calendar,
  TableProperties,
  Settings,
  Users,
  UsersRound,
  Cog,
  ChevronLeft,
  ChevronRight,
  CreditCard,
} from 'lucide-react'

const navItems = [
  { icon: LayoutDashboard, label: 'Dashboard', path: '/dashboard' },
  { icon: Calendar, label: 'Rezervasyonlar', path: '/reservations' },
  { icon: TableProperties, label: 'Masalar', path: '/tables' },
  { icon: Settings, label: 'Kurallar', path: '/rules' },
  { icon: Users, label: 'Müşteriler', path: '/customers' },
  { icon: UsersRound, label: 'Ekip', path: '/staff' },
  { icon: Cog, label: 'Ayarlar', path: '/settings' },
]

export function Sidebar() {
  const { sidebarCollapsed, toggleSidebar } = useUIStore()
  const tenant = useCurrentTenant()
  const { user, logout } = useAuth()

  const showUpgradeButton = tenant && tenant.planName !== 'Enterprise' && tenant.planName !== 'Business'

  return (
    <aside
      className={cn(
        'flex h-screen flex-col border-r bg-card transition-all duration-300',
        sidebarCollapsed ? 'w-16' : 'w-64'
      )}
    >
      {/* Logo */}
      <div className="flex h-16 items-center justify-between border-b px-4">
        {!sidebarCollapsed && (
          <div className="flex items-center space-x-2">
            <div className="h-8 w-8 rounded-lg bg-gradient-to-br from-primary to-accent" />
            <span className="text-xl font-bold">Tablewise</span>
          </div>
        )}
        <Button
          variant="ghost"
          size="icon"
          onClick={toggleSidebar}
          className={cn('h-8 w-8', sidebarCollapsed && 'mx-auto')}
        >
          {sidebarCollapsed ? (
            <ChevronRight className="h-4 w-4" />
          ) : (
            <ChevronLeft className="h-4 w-4" />
          )}
        </Button>
      </div>

      {/* Navigation */}
      <nav className="flex-1 space-y-1 overflow-y-auto p-2">
        {navItems.map((item) => (
          <NavLink
            key={item.path}
            to={item.path}
            className={({ isActive }) =>
              cn(
                'flex items-center space-x-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors',
                isActive
                  ? 'bg-accent text-accent-foreground'
                  : 'text-muted-foreground hover:bg-accent/50 hover:text-foreground',
                sidebarCollapsed && 'justify-center px-2'
              )
            }
          >
            <item.icon className="h-5 w-5 shrink-0" />
            {!sidebarCollapsed && <span>{item.label}</span>}
          </NavLink>
        ))}
      </nav>

      {/* Bottom section */}
      <div className="border-t p-4">
        {/* Plan badge + Upgrade */}
        {tenant && showUpgradeButton && !sidebarCollapsed && (
          <Button
            variant="outline"
            size="sm"
            className="mb-3 w-full justify-between"
            onClick={() => window.location.href = '/subscription'}
          >
            <div className="flex items-center space-x-2">
              <CreditCard className="h-4 w-4" />
              <Badge variant="secondary" className="text-xs">
                {tenant.planName}
              </Badge>
            </div>
            <span className="text-accent">Yükselt</span>
          </Button>
        )}

        {/* User */}
        <div className={cn('flex items-center', sidebarCollapsed ? 'justify-center' : 'space-x-3')}>
          {!sidebarCollapsed ? (
            <>
              <div className="flex h-10 w-10 items-center justify-center rounded-full bg-accent text-accent-foreground">
                {user?.name?.charAt(0).toUpperCase() || 'U'}
              </div>
              <div className="flex-1 overflow-hidden">
                <p className="truncate text-sm font-medium">{user?.name}</p>
                <p className="truncate text-xs text-muted-foreground">{user?.role}</p>
              </div>
              <Button
                variant="ghost"
                size="sm"
                onClick={logout}
                className="h-8 px-2 text-xs"
              >
                Çıkış
              </Button>
            </>
          ) : (
            <button
              onClick={logout}
              className="flex h-10 w-10 items-center justify-center rounded-full bg-accent text-accent-foreground hover:bg-accent/80"
            >
              {user?.name?.charAt(0).toUpperCase() || 'U'}
            </button>
          )}
        </div>
      </div>
    </aside>
  )
}
