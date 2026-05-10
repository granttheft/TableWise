import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import {
  Calendar,
  Crown,
  Users,
  CreditCard,
  Clock,
  Timer,
  Baby,
  Ban,
  Percent,
  AlertTriangle,
  Code,
  LucideIcon,
} from 'lucide-react'
import { ruleTemplates, customConditionTemplate } from '../data/ruleTemplates'
import type { RuleTemplate } from '@/types/api'

interface RuleTemplateGalleryDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onSelectTemplate: (template: RuleTemplate) => void
}

const iconMap: Record<string, LucideIcon> = {
  Calendar,
  Crown,
  Users,
  CreditCard,
  Clock,
  Timer,
  Baby,
  Ban,
  Percent,
  AlertTriangle,
  Code,
}

export function RuleTemplateGalleryDialog({
  open,
  onOpenChange,
  onSelectTemplate,
}: RuleTemplateGalleryDialogProps) {
  const handleSelect = (template: RuleTemplate) => {
    onSelectTemplate(template)
    onOpenChange(false)
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-4xl max-h-[85vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="text-xl">Kural Şablonu Seçin</DialogTitle>
          <DialogDescription>
            Hazır şablonlardan birini seçerek hızlıca kural oluşturun veya sıfırdan özel kural yazın.
          </DialogDescription>
        </DialogHeader>

        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3 mt-4">
          {ruleTemplates.map((template) => {
            const Icon = iconMap[template.icon] || Code
            return (
              <Card
                key={template.id}
                className="cursor-pointer transition-all hover:border-accent hover:shadow-md group"
                onClick={() => handleSelect(template)}
              >
                <CardContent className="p-4">
                  <div className="flex flex-col items-center text-center space-y-3">
                    <div className="p-3 rounded-full bg-accent/10 text-accent group-hover:bg-accent group-hover:text-white transition-colors">
                      <Icon className="h-8 w-8" />
                    </div>
                    <div>
                      <h3 className="font-semibold">{template.name}</h3>
                      <p className="text-sm text-muted-foreground mt-1 line-clamp-2">
                        {template.description}
                      </p>
                    </div>
                    <Badge variant="secondary" className="text-xs">
                      {template.ruleType}
                    </Badge>
                  </div>
                </CardContent>
              </Card>
            )
          })}

          {/* Custom Condition - Special Card */}
          <Card
            className="cursor-pointer transition-all hover:border-primary hover:shadow-md group border-dashed border-2"
            onClick={() => handleSelect(customConditionTemplate)}
          >
            <CardContent className="p-4">
              <div className="flex flex-col items-center text-center space-y-3">
                <div className="p-3 rounded-full bg-primary/10 text-primary group-hover:bg-primary group-hover:text-white transition-colors">
                  <Code className="h-8 w-8" />
                </div>
                <div>
                  <h3 className="font-semibold">{customConditionTemplate.name}</h3>
                  <p className="text-sm text-muted-foreground mt-1 line-clamp-2">
                    {customConditionTemplate.description}
                  </p>
                </div>
                <Badge variant="outline" className="text-xs">
                  İleri Seviye
                </Badge>
              </div>
            </CardContent>
          </Card>
        </div>

        <div className="flex justify-end mt-4">
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            İptal
          </Button>
        </div>
      </DialogContent>
    </Dialog>
  )
}
