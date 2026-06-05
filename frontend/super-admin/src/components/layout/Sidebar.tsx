import { NavLink } from 'react-router-dom'
import {
  LayoutDashboard,
  Building2,
  CreditCard,
  Tag,
  Receipt,
  Monitor,
  Users,
  ChevronLeft,
  ChevronRight,
} from 'lucide-react'
import { cn } from '@/lib/cn'
import { useUiStore } from '@/stores/uiStore'
import { useAuth } from '@/hooks/useAuth'
import { Badge } from '@/components/ui/badge'

interface NavItem {
  label: string
  icon: React.ComponentType<{ className?: string }>
  to: string
  soon?: boolean
  hideForRoles?: Array<'Marketing' | 'Finance'>
}

const navItems: NavItem[] = [
  { label: 'Dashboard', icon: LayoutDashboard, to: '/dashboard' },
  { label: 'Müşteriler', icon: Building2, to: '/tenants' },
  { label: 'Fiyatlandırma', icon: CreditCard, to: '/pricing', soon: true },
  { label: 'Kuponlar', icon: Tag, to: '/coupons', soon: true },
  { label: 'Ödemeler', icon: Receipt, to: '/payments', soon: true },
  { label: 'Cihazlar', icon: Monitor, to: '/devices', soon: true, hideForRoles: ['Marketing', 'Finance'] },
  { label: 'Ekip', icon: Users, to: '/team', soon: true, hideForRoles: ['Marketing', 'Finance'] },
]

export function Sidebar() {
  const { sidebarCollapsed, toggleSidebar } = useUiStore()
  const { user } = useAuth()

  const visibleItems = navItems.filter(
    (item) => !item.hideForRoles || !user || !item.hideForRoles.includes(user.role as 'Marketing' | 'Finance')
  )

  return (
    <aside
      className={cn(
        'relative flex h-screen flex-col border-r border-border bg-card transition-all duration-300',
        sidebarCollapsed ? 'w-16' : 'w-56'
      )}
    >
      {/* Logo */}
      <div className="flex h-16 items-center border-b border-border px-4">
        {!sidebarCollapsed && (
          <div>
            <span className="text-lg font-bold text-primary">Tablewise</span>
            <span className="ml-1.5 text-xs text-muted-foreground">Platform</span>
          </div>
        )}
        {sidebarCollapsed && (
          <span className="text-lg font-bold text-primary">TW</span>
        )}
      </div>

      {/* Navigation */}
      <nav className="flex-1 space-y-1 overflow-y-auto p-2">
        {visibleItems.map((item) => (
          <NavLink
            key={item.to}
            to={item.soon ? '#' : item.to}
            onClick={item.soon ? (e) => e.preventDefault() : undefined}
            className={({ isActive }) =>
              cn(
                'flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors',
                isActive && !item.soon
                  ? 'bg-primary text-primary-foreground'
                  : 'text-muted-foreground hover:bg-muted hover:text-foreground',
                item.soon && 'cursor-not-allowed opacity-60'
              )
            }
          >
            <item.icon className="h-4 w-4 shrink-0" />
            {!sidebarCollapsed && (
              <span className="flex flex-1 items-center justify-between">
                {item.label}
                {item.soon && (
                  <Badge variant="secondary" className="text-[10px] px-1 py-0 h-4">
                    Yakında
                  </Badge>
                )}
              </span>
            )}
          </NavLink>
        ))}
      </nav>

      {/* Collapse toggle */}
      <button
        onClick={toggleSidebar}
        className="absolute -right-3 top-20 flex h-6 w-6 items-center justify-center rounded-full border border-border bg-card text-muted-foreground shadow-sm hover:text-foreground"
      >
        {sidebarCollapsed ? (
          <ChevronRight className="h-3 w-3" />
        ) : (
          <ChevronLeft className="h-3 w-3" />
        )}
      </button>
    </aside>
  )
}
