import { Outlet } from 'react-router-dom'
import { Sidebar } from './Sidebar'
import { TopBar } from './TopBar'
import { TrialBanner } from '@/components/common/TrialBanner'
import { useUIStore } from '@/stores/uiStore'
import { useCurrentTenant } from '@/hooks/useCurrentTenant'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { AlertCircle } from 'lucide-react'

export function AppLayout() {
  const { sidebarCollapsed } = useUIStore()
  const tenant = useCurrentTenant()

  return (
    <div className="flex h-screen overflow-hidden bg-background">
      <Sidebar />
      
      <div className="flex flex-1 flex-col overflow-hidden">
        <TopBar />
        
        <main className="flex-1 overflow-y-auto">
          {/* Trial active banner */}
          <TrialBanner />
          
          {/* Trial expired banner */}
          {tenant?.isTrialExpired && (
            <div className="border-b border-amber-500/20 bg-amber-500/10 px-4 py-3">
              <Alert variant="default" className="border-amber-500/50 bg-transparent">
                <AlertCircle className="h-4 w-4 text-amber-500" />
                <AlertDescription className="flex items-center justify-between">
                  <span className="text-sm text-amber-500">
                    Deneme süreniz sona erdi. Hizmetlerimizi kullanmaya devam etmek için planınızı yükseltin.
                  </span>
                  <Button
                    size="sm"
                    variant="outline"
                    className="ml-4 border-amber-500 text-amber-500 hover:bg-amber-500 hover:text-white"
                    onClick={() => window.location.href = '/subscription'}
                  >
                    Planı Yükselt
                  </Button>
                </AlertDescription>
              </Alert>
            </div>
          )}
          
          <div className={`mx-auto max-w-screen-2xl p-6 transition-all ${sidebarCollapsed ? 'md:pl-20' : 'md:pl-6'}`}>
            <Outlet />
          </div>
        </main>
      </div>
    </div>
  )
}
