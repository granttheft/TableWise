import { Card, CardContent } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Switch } from '@/components/ui/switch'
import { Button } from '@/components/ui/button'
import {
  GripVertical,
  Calendar,
  Crown,
  Users,
  CreditCard,
  Clock,
  Timer,
  Scale,
  Code,
  Edit,
  Trash2,
  Play,
  Zap,
  LucideIcon,
} from 'lucide-react'
import { useSortable } from '@dnd-kit/sortable'
import { CSS } from '@dnd-kit/utilities'
import type { Rule, RuleType } from '@/types/api'
import { getPriorityColor, getTriggerLabel, ruleToHumanReadable } from '../utils/ruleHumanReadable'
import { cn } from '@/lib/cn'

interface RuleCardProps {
  rule: Rule
  onEdit: (rule: Rule) => void
  onDelete: (ruleId: string) => void
  onTest: (rule: Rule) => void
  onToggleActive: (ruleId: string, isActive: boolean) => void
}

const ruleTypeIcons: Record<RuleType, LucideIcon> = {
  EarlyBooking: Calendar,
  VipPriority: Crown,
  LargeGroup: Users,
  DepositRequired: CreditCard,
  PeakHour: Clock,
  TableCooldown: Timer,
  GroupComposition: Scale,
  CustomCondition: Code,
}

export function RuleCard({
  rule,
  onEdit,
  onDelete,
  onTest,
  onToggleActive,
}: RuleCardProps) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({ id: rule.id })

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
  }

  const Icon = ruleTypeIcons[rule.ruleType] || Code
  const humanReadable = ruleToHumanReadable(rule)
  const isGroupComposition = rule.ruleType === 'GroupComposition'

  return (
    <Card
      ref={setNodeRef}
      style={style}
      className={cn(
        'transition-all',
        isDragging && 'opacity-50 shadow-lg',
        !rule.isActive && 'opacity-60'
      )}
    >
      <CardContent className="p-4">
        <div className="flex items-start gap-3">
          {/* Drag Handle */}
          <div
            {...attributes}
            {...listeners}
            className="cursor-grab touch-none active:cursor-grabbing mt-1 p-1 hover:bg-muted rounded"
          >
            <GripVertical className="h-5 w-5 text-muted-foreground" />
          </div>

          {/* Priority Badge */}
          <Badge className={cn('shrink-0 mt-1', getPriorityColor(rule.priority))}>
            {rule.priority}
          </Badge>

          {/* Icon */}
          <div
            className={cn(
              'shrink-0 p-2 rounded-full bg-accent/10 text-accent',
              isGroupComposition && 'bg-pink-500/10 text-pink-600 ring-2 ring-pink-500/35'
            )}
          >
            <Icon className="h-5 w-5" />
          </div>

          {/* Content */}
          <div className="flex-1 min-w-0">
            <div className="flex items-center gap-2 mb-1">
              <h3 className="font-semibold truncate">{rule.name}</h3>
              <Badge variant="outline" className="shrink-0 text-xs">
                {getTriggerLabel(rule.trigger)}
              </Badge>
            </div>

            <p className="text-sm text-muted-foreground line-clamp-2 mb-2">
              {rule.description || humanReadable}
            </p>

            {/* Stats */}
            <div className="flex items-center gap-4 text-xs text-muted-foreground">
              <div className="flex items-center gap-1">
                <Zap className="h-3 w-3" />
                <span>{rule.timesTriggered} tetikleme</span>
              </div>
              {rule.lastTriggeredAt && (
                <span>
                  Son: {new Date(rule.lastTriggeredAt).toLocaleDateString('tr-TR')}
                </span>
              )}
            </div>
          </div>

          {/* Actions */}
          <div className="flex items-center gap-2 shrink-0">
            <Switch
              checked={rule.isActive}
              onCheckedChange={(checked) => onToggleActive(rule.id, checked)}
            />

            <Button
              variant="ghost"
              size="icon"
              onClick={() => onTest(rule)}
              title="Test Et"
            >
              <Play className="h-4 w-4" />
            </Button>

            <Button
              variant="ghost"
              size="icon"
              onClick={() => onEdit(rule)}
              title="Düzenle"
            >
              <Edit className="h-4 w-4" />
            </Button>

            <Button
              variant="ghost"
              size="icon"
              onClick={() => onDelete(rule.id)}
              title="Sil"
            >
              <Trash2 className="h-4 w-4 text-destructive" />
            </Button>
          </div>
        </div>
      </CardContent>
    </Card>
  )
}
