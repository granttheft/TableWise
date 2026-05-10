import { Card, CardContent } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Switch } from '@/components/ui/switch'
import { Button } from '@/components/ui/button'
import { GripVertical, MapPin, Users, Edit, Trash2 } from 'lucide-react'
import { useSortable } from '@dnd-kit/sortable'
import { CSS } from '@dnd-kit/utilities'
import type { Table, TableLocation } from '@/types/api'
import { cn } from '@/lib/cn'

interface TableCardProps {
  table: Table
  onEdit: (table: Table) => void
  onDelete: (tableId: string) => void
  onToggleActive: (tableId: string, isActive: boolean) => void
}

const locationLabels: Record<TableLocation, string> = {
  Indoor: 'İç Mekan',
  Outdoor: 'Dış Mekan',
  Balcony: 'Balkon',
  Bar: 'Bar',
  Private: 'Özel Salon',
  Terrace: 'Teras',
  Garden: 'Bahçe',
}

export function TableCard({ table, onEdit, onDelete, onToggleActive }: TableCardProps) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({ id: table.id })

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
  }

  return (
    <Card
      ref={setNodeRef}
      style={style}
      className={cn(
        'relative',
        isDragging && 'opacity-50',
        !table.isActive && 'opacity-60'
      )}
    >
      <CardContent className="p-4">
        <div className="flex items-start justify-between">
          <div className="flex items-start space-x-3 flex-1">
            <div
              {...attributes}
              {...listeners}
              className="cursor-grab touch-none active:cursor-grabbing mt-1"
            >
              <GripVertical className="h-5 w-5 text-muted-foreground" />
            </div>

            <div className="flex-1 min-w-0">
              <div className="flex items-center space-x-2 mb-2">
                <h3 className="text-lg font-semibold truncate">{table.name}</h3>
                <Badge variant="secondary" className="flex items-center space-x-1">
                  <Users className="h-3 w-3" />
                  <span>{table.capacity}</span>
                </Badge>
              </div>

              <div className="flex items-center space-x-1 text-sm text-muted-foreground mb-2">
                <MapPin className="h-3 w-3" />
                <span>{locationLabels[table.location]}</span>
              </div>

              {table.description && (
                <p className="text-sm text-muted-foreground line-clamp-2">
                  {table.description}
                </p>
              )}
            </div>
          </div>
        </div>

        <div className="flex items-center justify-between mt-4 pt-4 border-t">
          <div className="flex items-center space-x-2">
            <Switch
              checked={table.isActive}
              onCheckedChange={(checked) => onToggleActive(table.id, checked)}
            />
            <span className="text-sm text-muted-foreground">
              {table.isActive ? 'Aktif' : 'Pasif'}
            </span>
          </div>

          <div className="flex items-center space-x-2">
            <Button
              variant="ghost"
              size="sm"
              onClick={() => onEdit(table)}
            >
              <Edit className="h-4 w-4" />
            </Button>
            <Button
              variant="ghost"
              size="sm"
              onClick={() => onDelete(table.id)}
            >
              <Trash2 className="h-4 w-4 text-destructive" />
            </Button>
          </div>
        </div>
      </CardContent>
    </Card>
  )
}
