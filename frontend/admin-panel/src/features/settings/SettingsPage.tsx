import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { GeneralSettings } from './components/GeneralSettings'
import { WorkingHoursSettings } from './components/WorkingHoursSettings'
import { NotificationSettings } from './components/NotificationSettings'
import { DepositSettings } from './components/DepositSettings'
import { BookingSettings } from './components/BookingSettings'
import { IntegrationSettings } from './components/IntegrationSettings'
import { VenueSettings } from './components/VenueSettings'

export function SettingsPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Ayarlar</h1>
        <p className="text-muted-foreground">Mekan ve sistem ayarlarını yönetin</p>
      </div>

      <Tabs defaultValue="general" className="space-y-4">
        <TabsList>
          <TabsTrigger value="general">Genel</TabsTrigger>
          <TabsTrigger value="venues">Mekanlar</TabsTrigger>
          <TabsTrigger value="working-hours">Çalışma Saatleri</TabsTrigger>
          <TabsTrigger value="notifications">Bildirimler</TabsTrigger>
          <TabsTrigger value="deposit">Kapora</TabsTrigger>
          <TabsTrigger value="booking">Booking</TabsTrigger>
          <TabsTrigger value="integration">Entegrasyon</TabsTrigger>
        </TabsList>

        <TabsContent value="general">
          <GeneralSettings />
        </TabsContent>

        <TabsContent value="venues">
          <VenueSettings />
        </TabsContent>

        <TabsContent value="working-hours">
          <WorkingHoursSettings />
        </TabsContent>

        <TabsContent value="notifications">
          <NotificationSettings />
        </TabsContent>

        <TabsContent value="deposit">
          <DepositSettings />
        </TabsContent>

        <TabsContent value="booking">
          <BookingSettings />
        </TabsContent>

        <TabsContent value="integration">
          <IntegrationSettings />
        </TabsContent>
      </Tabs>
    </div>
  )
}
