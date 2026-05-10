import { useState } from 'react'
import {
  Plus,
  Code,
  Store,
  AlertCircle,
  Zap,
} from 'lucide-react'
import {
  DndContext,
  closestCenter,
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors,
  DragEndEvent,
} from '@dnd-kit/core'
import {
  arrayMove,
  SortableContext,
  sortableKeyboardCoordinates,
  verticalListSortingStrategy,
} from '@dnd-kit/sortable'
import { Button } from '@/components/ui/button'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Badge } from '@/components/ui/badge'
import { Card, CardContent } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { useVenues } from '@/hooks/useVenues'
import { usePlanLimits } from '@/hooks/usePlanLimits'
import {
  useRules,
  useCreateRule,
  useUpdateRule,
  useDeleteRule,
  useReorderRules,
  useToggleRuleActive,
} from '@/hooks/useRules'
import { RuleCard } from './components/RuleCard'
import { RuleTemplateGalleryDialog } from './components/RuleTemplateGalleryDialog'
import { RuleBuilderDialog } from './components/RuleBuilderDialog'
import { RuleTestDialog } from './components/RuleTestDialog'
import type { Rule, RuleTemplate } from '@/types/api'

export function RulesPage() {
  const [selectedVenueId, setSelectedVenueId] = useState<string>('')
  const [isTemplateGalleryOpen, setIsTemplateGalleryOpen] = useState(false)
  const [isBuilderOpen, setIsBuilderOpen] = useState(false)
  const [isTestDialogOpen, setIsTestDialogOpen] = useState(false)
  const [selectedTemplate, setSelectedTemplate] = useState<RuleTemplate | null>(null)
  const [editingRule, setEditingRule] = useState<Rule | null>(null)
  const [testingRule, setTestingRule] = useState<Rule | null>(null)

  const { data: venues, isLoading: venuesLoading } = useVenues()
  const { data: planLimits } = usePlanLimits()
  const { data: rules = [], isLoading: rulesLoading } = useRules(selectedVenueId)

  const createRuleMutation = useCreateRule()
  const updateRuleMutation = useUpdateRule()
  const deleteRuleMutation = useDeleteRule()
  const reorderRulesMutation = useReorderRules()
  const toggleActiveMutation = useToggleRuleActive()

  const sensors = useSensors(
    useSensor(PointerSensor),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  )

  // Auto-select first venue
  if (!selectedVenueId && venues && venues.length > 0 && !venuesLoading) {
    setSelectedVenueId(venues[0].id)
  }

  const sortedRules = [...rules].sort((a, b) => b.priority - a.priority)

  const isLimitWarning =
    planLimits?.maxRules && planLimits.currentRuleCount >= planLimits.maxRules * 0.8
  const isLimitReached =
    planLimits?.maxRules && planLimits.currentRuleCount >= planLimits.maxRules
  const remainingRules = planLimits?.maxRules
    ? planLimits.maxRules - planLimits.currentRuleCount
    : null

  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event

    if (over && active.id !== over.id) {
      const oldIndex = sortedRules.findIndex((r) => r.id === active.id)
      const newIndex = sortedRules.findIndex((r) => r.id === over.id)

      const reorderedRules = arrayMove(sortedRules, oldIndex, newIndex)
      const ruleIds = reorderedRules.map((r) => r.id)

      reorderRulesMutation.mutate({
        venueId: selectedVenueId,
        ruleIds,
      })
    }
  }

  const handleSelectTemplate = (template: RuleTemplate) => {
    setSelectedTemplate(template)
    setEditingRule(null)
    setIsBuilderOpen(true)
  }

  const handleNewCustomRule = () => {
    setSelectedTemplate(null)
    setEditingRule(null)
    setIsBuilderOpen(true)
  }

  const handleEditRule = (rule: Rule) => {
    setSelectedTemplate(null)
    setEditingRule(rule)
    setIsBuilderOpen(true)
  }

  const handleTestRule = (rule: Rule) => {
    setTestingRule(rule)
    setIsTestDialogOpen(true)
  }

  const handleDeleteRule = (ruleId: string) => {
    if (!confirm('Bu kuralı silmek istediğinizden emin misiniz?')) return

    deleteRuleMutation.mutate({
      ruleId,
      venueId: selectedVenueId,
    })
  }

  const handleToggleActive = (ruleId: string, isActive: boolean) => {
    toggleActiveMutation.mutate({
      ruleId,
      venueId: selectedVenueId,
      isActive,
    })
  }

  const handleSubmitRule = (ruleData: any) => {
    if (editingRule) {
      updateRuleMutation.mutate(
        {
          ruleId: editingRule.id,
          venueId: selectedVenueId,
          rule: ruleData,
        },
        {
          onSuccess: () => {
            setIsBuilderOpen(false)
            setEditingRule(null)
          },
        }
      )
    } else {
      createRuleMutation.mutate(
        {
          venueId: selectedVenueId,
          rule: ruleData,
        },
        {
          onSuccess: () => {
            setIsBuilderOpen(false)
            setSelectedTemplate(null)
          },
        }
      )
    }
  }

  if (venuesLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-96 w-full" />
      </div>
    )
  }

  if (!venues || venues.length === 0) {
    return (
      <div className="space-y-6">
        <h1 className="text-3xl font-bold">Kurallar</h1>
        <Alert>
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>
            Henüz mekanınız yok. Kural ekleyebilmek için önce bir mekan oluşturmalısınız.
          </AlertDescription>
        </Alert>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Kurallar</h1>
          <p className="text-muted-foreground">
            Rezervasyon kurallarını oluşturun ve yönetin
          </p>
        </div>

        <div className="flex items-center space-x-3">
          {venues.length > 1 && (
            <Select value={selectedVenueId} onValueChange={setSelectedVenueId}>
              <SelectTrigger className="w-[200px]">
                <Store className="mr-2 h-4 w-4" />
                <SelectValue placeholder="Mekan seçin" />
              </SelectTrigger>
              <SelectContent>
                {venues.map((venue) => (
                  <SelectItem key={venue.id} value={venue.id}>
                    {venue.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          )}

          <Button
            variant="outline"
            onClick={handleNewCustomRule}
            disabled={!selectedVenueId || !!isLimitReached}
          >
            <Code className="mr-2 h-4 w-4" />
            Özel Kural
          </Button>

          <Button
            onClick={() => setIsTemplateGalleryOpen(true)}
            disabled={!selectedVenueId || !!isLimitReached}
          >
            <Plus className="mr-2 h-4 w-4" />
            Yeni Kural
          </Button>
        </div>
      </div>

      {/* Plan Limit Banner */}
      {remainingRules !== null && remainingRules <= 5 && !isLimitReached && (
        <Alert variant="default" className="border-amber-500/50 bg-amber-500/10">
          <AlertCircle className="h-4 w-4 text-amber-500" />
          <AlertDescription className="text-amber-500">
            {remainingRules} kural hakkınız kaldı. Daha fazla kural için planınızı yükseltin.
          </AlertDescription>
        </Alert>
      )}

      {isLimitReached && (
        <Alert variant="default" className="border-destructive/50 bg-destructive/10">
          <AlertCircle className="h-4 w-4 text-destructive" />
          <AlertDescription className="flex items-center justify-between text-destructive">
            <span>
              Plan limitinize ulaştınız ({planLimits.maxRules} kural). Daha fazla kural
              ekleyemezsiniz.
            </span>
            <Button
              size="sm"
              variant="outline"
              className="ml-4"
              onClick={() => (window.location.href = '/subscription')}
            >
              Planı Yükselt
            </Button>
          </AlertDescription>
        </Alert>
      )}

      {/* Rules List */}
      {rulesLoading ? (
        <div className="space-y-4">
          {[1, 2, 3].map((i) => (
            <Skeleton key={i} className="h-24" />
          ))}
        </div>
      ) : sortedRules.length === 0 ? (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-16">
            <Zap className="mb-4 h-16 w-16 text-muted-foreground" />
            <h3 className="mb-2 text-lg font-semibold">Henüz kuralınız yok</h3>
            <p className="mb-4 max-w-md text-center text-sm text-muted-foreground">
              Kurallar sayesinde rezervasyonları otomatik yönetebilir, VIP müşterilere öncelik
              tanıyabilir, indirimler ve kapora zorunlulukları ekleyebilirsiniz.
            </p>
            <div className="flex items-center space-x-3">
              <Button variant="outline" onClick={handleNewCustomRule}>
                <Code className="mr-2 h-4 w-4" />
                Özel Kural Yaz
              </Button>
              <Button onClick={() => setIsTemplateGalleryOpen(true)}>
                <Plus className="mr-2 h-4 w-4" />
                Şablondan Başla
              </Button>
            </div>
          </CardContent>
        </Card>
      ) : (
        <DndContext
          sensors={sensors}
          collisionDetection={closestCenter}
          onDragEnd={handleDragEnd}
        >
          <SortableContext
            items={sortedRules.map((r) => r.id)}
            strategy={verticalListSortingStrategy}
          >
            <div className="space-y-3">
              {sortedRules.map((rule) => (
                <RuleCard
                  key={rule.id}
                  rule={rule}
                  onEdit={handleEditRule}
                  onDelete={handleDeleteRule}
                  onTest={handleTestRule}
                  onToggleActive={handleToggleActive}
                />
              ))}
            </div>
          </SortableContext>
        </DndContext>
      )}

      {/* Stats Footer */}
      {sortedRules.length > 0 && (
        <div className="flex items-center justify-between text-sm text-muted-foreground border-t pt-4">
          <div className="flex items-center space-x-4">
            <span>Toplam: {sortedRules.length} kural</span>
            <span>Aktif: {sortedRules.filter((r) => r.isActive).length}</span>
            <span>
              Toplam tetikleme:{' '}
              {sortedRules.reduce((sum, r) => sum + r.timesTriggered, 0)}
            </span>
          </div>
          {planLimits?.maxRules && (
            <Badge variant="outline">
              {planLimits.currentRuleCount} / {planLimits.maxRules} kural kullanılıyor
            </Badge>
          )}
        </div>
      )}

      {/* Dialogs */}
      <RuleTemplateGalleryDialog
        open={isTemplateGalleryOpen}
        onOpenChange={setIsTemplateGalleryOpen}
        onSelectTemplate={handleSelectTemplate}
      />

      <RuleBuilderDialog
        open={isBuilderOpen}
        onOpenChange={setIsBuilderOpen}
        template={selectedTemplate}
        existingRule={editingRule}
        venueId={selectedVenueId}
        onSubmit={handleSubmitRule}
        isLoading={createRuleMutation.isPending || updateRuleMutation.isPending}
      />

      {testingRule && (
        <RuleTestDialog
          open={isTestDialogOpen}
          onOpenChange={setIsTestDialogOpen}
          ruleId={testingRule.id}
          ruleName={testingRule.name}
        />
      )}
    </div>
  )
}
