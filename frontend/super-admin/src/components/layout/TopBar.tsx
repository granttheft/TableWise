import { LogOut } from 'lucide-react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '@/hooks/useAuth'
import { Button } from '@/components/ui/button'
import { cn } from '@/lib/cn'
import type { PlatformRole } from '@/types/api'

const roleBadgeClass: Record<PlatformRole, string> = {
  SuperAdmin: 'bg-indigo-500/20 text-indigo-300 border-indigo-500/30',
  Marketing: 'bg-emerald-500/20 text-emerald-300 border-emerald-500/30',
  Finance: 'bg-amber-500/20 text-amber-300 border-amber-500/30',
}

const roleLabel: Record<PlatformRole, string> = {
  SuperAdmin: 'Super Admin',
  Marketing: 'Marketing',
  Finance: 'Finance',
}

export function TopBar() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  const handleLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <header className="flex h-16 items-center justify-between border-b border-border bg-card px-6">
      <div />
      <div className="flex items-center gap-4">
        {user && (
          <div className="flex items-center gap-2">
            <span className="text-sm text-muted-foreground">{user.fullName}</span>
            <span
              className={cn(
                'rounded-full border px-2 py-0.5 text-xs font-medium',
                roleBadgeClass[user.role]
              )}
            >
              {roleLabel[user.role]}
            </span>
          </div>
        )}
        <Button variant="ghost" size="icon" onClick={handleLogout} title="Çıkış">
          <LogOut className="h-4 w-4" />
        </Button>
      </div>
    </header>
  )
}
