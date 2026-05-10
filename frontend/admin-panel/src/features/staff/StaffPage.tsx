import { useState } from 'react'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Button } from '@/components/ui/button'
import { Card } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Avatar, AvatarFallback } from '@/components/ui/avatar'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { Plus, MoreVertical, Mail, RefreshCw, X } from 'lucide-react'
import { format } from 'date-fns'
import { tr } from 'date-fns/locale'
import { toast } from 'sonner'
import { InviteStaffDialog } from './components/InviteStaffDialog'
import type { StaffMember, StaffInvite } from '@/types/api'

export function StaffPage() {
  const [isInviteDialogOpen, setIsInviteDialogOpen] = useState(false)

  // TODO: Fetch staff members and invites
  const staffMembers: StaffMember[] = []
  const pendingInvites: StaffInvite[] = []
  const isLoading = false

  const handleChangeRole = (staffId: string, newRole: 'Owner' | 'Staff') => {
    console.log('Change role:', staffId, newRole)
    toast.success('Rol güncellendi')
  }

  const handleRemoveStaff = (staffId: string) => {
    if (!confirm('Bu kullanıcıyı silmek istediğinizden emin misiniz?')) {
      return
    }
    console.log('Remove staff:', staffId)
    toast.success('Kullanıcı silindi')
  }

  const handleResendInvite = (inviteId: string) => {
    console.log('Resend invite:', inviteId)
    toast.success('Davet tekrar gönderildi')
  }

  const handleCancelInvite = (inviteId: string) => {
    console.log('Cancel invite:', inviteId)
    toast.success('Davet iptal edildi')
  }

  const getInitials = (name: string) => {
    return name
      .split(' ')
      .map((n) => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2)
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Ekip</h1>
          <p className="text-muted-foreground">Ekip üyelerinizi ve davetlerinizi yönetin</p>
        </div>
        <Button onClick={() => setIsInviteDialogOpen(true)}>
          <Plus className="mr-2 h-4 w-4" />
          Yeni Davet
        </Button>
      </div>

      <Tabs defaultValue="active" className="space-y-4">
        <TabsList>
          <TabsTrigger value="active">
            Aktif Ekip ({staffMembers.length})
          </TabsTrigger>
          <TabsTrigger value="invites">
            Bekleyen Davetler ({pendingInvites.length})
          </TabsTrigger>
        </TabsList>

        <TabsContent value="active">
          {isLoading ? (
            <Card className="flex items-center justify-center py-12">
              <div className="text-muted-foreground">Yükleniyor...</div>
            </Card>
          ) : staffMembers.length === 0 ? (
            <Card className="flex flex-col items-center justify-center py-12">
              <p className="text-muted-foreground">Henüz ekip üyeniz yok</p>
              <Button variant="link" onClick={() => setIsInviteDialogOpen(true)}>
                İlk davetinizi gönderin
              </Button>
            </Card>
          ) : (
            <Card>
              <div className="divide-y">
                {staffMembers.map((staff) => {
                  const isOwner = staff.role === 'Owner'
                  const isLastOwner = isOwner && staffMembers.filter((s) => s.role === 'Owner').length === 1

                  return (
                    <div key={staff.id} className="flex items-center justify-between p-4">
                      <div className="flex items-center gap-4">
                        <Avatar className="h-12 w-12">
                          <AvatarFallback className="bg-primary text-primary-foreground">
                            {getInitials(staff.fullName)}
                          </AvatarFallback>
                        </Avatar>

                        <div>
                          <div className="flex items-center gap-2">
                            <span className="font-medium">{staff.fullName}</span>
                            <Badge variant={isOwner ? 'default' : 'secondary'}>
                              {isOwner ? 'Owner' : 'Staff'}
                            </Badge>
                          </div>
                          <div className="text-sm text-muted-foreground">{staff.email}</div>
                          <div className="text-xs text-muted-foreground">
                            Son giriş: {staff.lastLoginAt ? format(new Date(staff.lastLoginAt), 'dd MMM yyyy HH:mm', { locale: tr }) : 'Hiç'}
                          </div>
                        </div>
                      </div>

                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button variant="ghost" size="icon">
                            <MoreVertical className="h-4 w-4" />
                          </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end">
                          {!isOwner && (
                            <DropdownMenuItem onClick={() => handleChangeRole(staff.id, 'Owner')}>
                              Owner Yap
                            </DropdownMenuItem>
                          )}
                          {isOwner && !isLastOwner && (
                            <DropdownMenuItem onClick={() => handleChangeRole(staff.id, 'Staff')}>
                              Staff Yap
                            </DropdownMenuItem>
                          )}
                          <DropdownMenuItem
                            onClick={() => handleRemoveStaff(staff.id)}
                            disabled={isLastOwner}
                            className="text-destructive"
                          >
                            {isLastOwner ? 'Son Owner Silinemez' : 'Sil'}
                          </DropdownMenuItem>
                        </DropdownMenuContent>
                      </DropdownMenu>
                    </div>
                  )
                })}
              </div>
            </Card>
          )}
        </TabsContent>

        <TabsContent value="invites">
          {isLoading ? (
            <Card className="flex items-center justify-center py-12">
              <div className="text-muted-foreground">Yükleniyor...</div>
            </Card>
          ) : pendingInvites.length === 0 ? (
            <Card className="flex flex-col items-center justify-center py-12">
              <Mail className="mb-4 h-12 w-12 text-muted-foreground" />
              <p className="text-muted-foreground">Bekleyen davet yok</p>
            </Card>
          ) : (
            <Card>
              <div className="divide-y">
                {pendingInvites.map((invite) => {
                  const isExpired = new Date(invite.expiresAt) < new Date()

                  return (
                    <div key={invite.id} className="flex items-center justify-between p-4">
                      <div className="space-y-1">
                        <div className="flex items-center gap-2">
                          <span className="font-medium">{invite.email}</span>
                          <Badge variant="secondary">{invite.role}</Badge>
                          {isExpired && (
                            <Badge variant="destructive">Süresi Doldu</Badge>
                          )}
                        </div>
                        <div className="text-sm text-muted-foreground">
                          Gönderildi: {format(new Date(invite.createdAt), 'dd MMM yyyy', { locale: tr })}
                        </div>
                        <div className="text-sm text-muted-foreground">
                          Sona erme: {format(new Date(invite.expiresAt), 'dd MMM yyyy', { locale: tr })}
                        </div>
                      </div>

                      <div className="flex gap-2">
                        <Button
                          variant="outline"
                          size="sm"
                          onClick={() => handleResendInvite(invite.id)}
                        >
                          <RefreshCw className="mr-2 h-4 w-4" />
                          Tekrar Gönder
                        </Button>
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleCancelInvite(invite.id)}
                        >
                          <X className="mr-2 h-4 w-4" />
                          İptal
                        </Button>
                      </div>
                    </div>
                  )
                })}
              </div>
            </Card>
          )}
        </TabsContent>
      </Tabs>

      <InviteStaffDialog
        open={isInviteDialogOpen}
        onOpenChange={setIsInviteDialogOpen}
      />
    </div>
  )
}
