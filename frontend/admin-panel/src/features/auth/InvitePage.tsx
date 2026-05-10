import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'

export function InvitePage() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-primary via-primary/90 to-accent/20 p-4">
      <Card className="w-full max-w-md">
        <CardHeader>
          <CardTitle>Davet</CardTitle>
          <CardDescription>Ekibe katılın</CardDescription>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground">Coming soon...</p>
        </CardContent>
      </Card>
    </div>
  )
}
