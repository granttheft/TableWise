import type { ReactNode } from 'react'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'

interface AuthPageShellProps {
  title: string
  description: string
  children: ReactNode
}

/**
 * Login ve diğer public auth sayfaları için ortak gradient + kart kabuğu.
 */
export function AuthPageShell({ title, description, children }: AuthPageShellProps) {
  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-br from-primary via-primary/90 to-accent/20 p-4">
      <Card className="w-full max-w-md">
        <CardHeader className="space-y-1 text-center">
          <div className="mx-auto mb-4 h-12 w-12 rounded-lg bg-gradient-to-br from-primary to-accent" />
          <CardTitle className="text-2xl">{title}</CardTitle>
          <CardDescription>{description}</CardDescription>
        </CardHeader>
        <CardContent>{children}</CardContent>
      </Card>
    </div>
  )
}
