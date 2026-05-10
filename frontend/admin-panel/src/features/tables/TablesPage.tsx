import { useState } from 'react'
import { Plus, Combine, Store, AlertCircle } from 'lucide-react'
import { DndContext, closestCenter, KeyboardSensor, PointerSensor, useSensor, useSensors, DragEndEvent } from '@dnd-kit/core'
import { arrayMove, SortableContext, sortableKeyboardCoordinates, verticalListSortingStrategy } from '@dnd-kit/sortable'
import { Button } from '@/components/ui/button'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Badge } from '@/components/ui/badge'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { Card, CardContent } from '@/components/ui/card'
import { useVenues, usePlanLimits } from '@/hooks/useVenues'
import { useTables, useCreateTable, useUpdateTable, useDeleteTable, useReorderTables } from '@/hooks/useTables'
import { useTableCombinations, useCreateTableCombination, useDeleteTableCombination } from '@/hooks/useTableCombinations'
import { TableCard } from './components/TableCard'
import { TableFormDialog } from './components/TableFormDialog'
import { TableCombinationDialog } from './components/TableCombinationDialog'
import { Skeleton } from '@/components/ui/skeleton'
import type { Table } from '@/types/api'

export function TablesPage() {
  const [selectedVenueId, setSelectedVenueId] = useState<string>('')
  const [isTableFormOpen, setIsTableFormOpen] = useState(false)
  const [isCombinationFormOpen, setIsCombinationFormOpen] = useState(false)
  const [editingTable, setEditingTable] = useState<Table | null>(null)

  const { data: venues, isLoading: venuesLoading } = useVenues()
  const { data: planLimits } = usePlanLimits()
  const { data: tables = [], isLoading: tablesLoading } = useTables(selectedVenueId)
  const { data: combinations = [], isLoading: combinationsLoading } = useTableCombinations(selectedVenueId)

  const createTableMutation = useCreateTable()
  const updateTableMutation = useUpdateTable()
  const deleteTableMutation = useDeleteTable()
  const reorderTablesMutation = useReorderTables()
  const createCombinationMutation = useCreateTableCombination()
  const deleteCombinationMutation = useDeleteTableCombination()

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

  const currentVenue = venues?.find((v) => v.id === selectedVenueId)
  const isLimitWarning = planLimits?.maxTables && planLimits.currentTableCount >= planLimits.maxTables * 0.8
  const isLimitReached = planLimits?.maxTables && planLimits.currentTableCount >= planLimits.maxTables

  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event

    if (over && active.id !== over.id) {
      const oldIndex = tables.findIndex((t) => t.id === active.id)
      const newIndex = tables.findIndex((t) => t.id === over.id)
      
      const reorderedTables = arrayMove(tables, oldIndex, newIndex)
      const tableIds = reorderedTables.map((t) => t.id)

      reorderTablesMutation.mutate({
        venueId: selectedVenueId,
        tableIds,
      })
    }
  }

  const handleCreateTable = (data: any) => {
    createTableMutation.mutate(
      {
        venueId: selectedVenueId,
        table: data,
      },
      {
        onSuccess: () => {
          setIsTableFormOpen(false)
          setEditingTable(null)
        },
      }
    )
  }

  const handleUpdateTable = (data: any) => {
    if (!editingTable) return

    updateTableMutation.mutate(
      {
        venueId: selectedVenueId,
        tableId: editingTable.id,
        table: data,
      },
      {
        onSuccess: () => {
          setIsTableFormOpen(false)
          setEditingTable(null)
        },
      }
    )
  }

  const handleDeleteTable = (tableId: string) => {
    if (!confirm('Bu masayı silmek istediğinizden emin misiniz?')) return

    deleteTableMutation.mutate({
      venueId: selectedVenueId,
      tableId,
    })
  }

  const handleToggleActive = (tableId: string, isActive: boolean) => {
    updateTableMutation.mutate({
      venueId: selectedVenueId,
      tableId,
      table: { isActive },
    })
  }

  const handleCreateCombination = (data: any) => {
    createCombinationMutation.mutate(
      {
        venueId: selectedVenueId,
        combination: data,
      },
      {
        onSuccess: () => {
          setIsCombinationFormOpen(false)
        },
      }
    )
  }

  const handleDeleteCombination = (combinationId: string) => {
    if (!confirm('Bu birleşimi silmek istediğinizden emin misiniz?')) return

    deleteCombinationMutation.mutate({
      venueId: selectedVenueId,
      combinationId,
    })
  }

  const handleEditTable = (table: Table) => {
    setEditingTable(table)
    setIsTableFormOpen(true)
  }

  const handleNewTable = () => {
    setEditingTable(null)
    setIsTableFormOpen(true)
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
        <h1 className="text-3xl font-bold">Masalar</h1>
        <Alert>
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>
            Henüz mekanınız yok. Masa ekleyebilmek için önce bir mekan oluşturmalısınız.
          </AlertDescription>
        </Alert>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Masalar</h1>
          <p className="text-muted-foreground">Masa ve oturma düzenini yönetin</p>
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
            onClick={() => setIsCombinationFormOpen(true)}
            disabled={!selectedVenueId || tables.length < 2}
          >
            <Combine className="mr-2 h-4 w-4" />
            Masa Birleşimi
          </Button>

          <Button
            onClick={handleNewTable}
            disabled={!selectedVenueId || isLimitReached}
          >
            <Plus className="mr-2 h-4 w-4" />
            Yeni Masa
          </Button>
        </div>
      </div>

      {isLimitWarning && !isLimitReached && (
        <Alert variant="default" className="border-amber-500/50 bg-amber-500/10">
          <AlertCircle className="h-4 w-4 text-amber-500" />
          <AlertDescription className="text-amber-500">
            Plan limitinize yaklaşıyorsunuz ({planLimits.currentTableCount} / {planLimits.maxTables} masa).
            Daha fazla masa eklemek için planınızı yükseltin.
          </AlertDescription>
        </Alert>
      )}

      {isLimitReached && (
        <Alert variant="default" className="border-destructive/50 bg-destructive/10">
          <AlertCircle className="h-4 w-4 text-destructive" />
          <AlertDescription className="flex items-center justify-between text-destructive">
            <span>
              Plan limitinize ulaştınız ({planLimits.maxTables} masa). Daha fazla masa ekleyemezsiniz.
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

      <Tabs defaultValue="tables" className="space-y-4">
        <TabsList>
          <TabsTrigger value="tables">
            Masalar {tables.length > 0 && <Badge variant="secondary" className="ml-2">{tables.length}</Badge>}
          </TabsTrigger>
          <TabsTrigger value="combinations">
            Birleşimler {combinations.length > 0 && <Badge variant="secondary" className="ml-2">{combinations.length}</Badge>}
          </TabsTrigger>
        </TabsList>

        <TabsContent value="tables" className="space-y-4">
          {tablesLoading ? (
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
              {[1, 2, 3].map((i) => (
                <Skeleton key={i} className="h-48" />
              ))}
            </div>
          ) : tables.length === 0 ? (
            <Card>
              <CardContent className="flex flex-col items-center justify-center py-16">
                <Store className="mb-4 h-16 w-16 text-muted-foreground" />
                <h3 className="mb-2 text-lg font-semibold">Henüz masanız yok</h3>
                <p className="mb-4 text-center text-sm text-muted-foreground">
                  İlk masanızı ekleyerek rezervasyon almaya başlayın.
                </p>
                <Button onClick={handleNewTable} disabled={isLimitReached}>
                  <Plus className="mr-2 h-4 w-4" />
                  İlk Masayı Ekle
                </Button>
              </CardContent>
            </Card>
          ) : (
            <DndContext
              sensors={sensors}
              collisionDetection={closestCenter}
              onDragEnd={handleDragEnd}
            >
              <SortableContext
                items={tables.map((t) => t.id)}
                strategy={verticalListSortingStrategy}
              >
                <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
                  {tables.map((table) => (
                    <TableCard
                      key={table.id}
                      table={table}
                      onEdit={handleEditTable}
                      onDelete={handleDeleteTable}
                      onToggleActive={handleToggleActive}
                    />
                  ))}
                </div>
              </SortableContext>
            </DndContext>
          )}
        </TabsContent>

        <TabsContent value="combinations" className="space-y-4">
          {combinationsLoading ? (
            <Skeleton className="h-48" />
          ) : combinations.length === 0 ? (
            <Card>
              <CardContent className="flex flex-col items-center justify-center py-16">
                <Combine className="mb-4 h-16 w-16 text-muted-foreground" />
                <h3 className="mb-2 text-lg font-semibold">Henüz birleşim yok</h3>
                <p className="mb-4 text-center text-sm text-muted-foreground">
                  Birden fazla masayı birleştirerek daha büyük gruplar için yer oluşturun.
                </p>
                <Button
                  onClick={() => setIsCombinationFormOpen(true)}
                  disabled={tables.length < 2}
                >
                  <Plus className="mr-2 h-4 w-4" />
                  İlk Birleşimi Oluştur
                </Button>
              </CardContent>
            </Card>
          ) : (
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
              {combinations.map((combination) => (
                <Card key={combination.id}>
                  <CardContent className="p-4">
                    <div className="flex items-start justify-between mb-3">
                      <div>
                        <h3 className="font-semibold">{combination.name}</h3>
                        <Badge variant="secondary" className="mt-2">
                          {combination.combinedCapacity} kişi
                        </Badge>
                      </div>
                      <Button
                        variant="ghost"
                        size="sm"
                        onClick={() => handleDeleteCombination(combination.id)}
                      >
                        <AlertCircle className="h-4 w-4 text-destructive" />
                      </Button>
                    </div>
                    <div className="space-y-1">
                      <p className="text-sm text-muted-foreground">Masalar:</p>
                      <div className="flex flex-wrap gap-2">
                        {combination.tables.map((table) => (
                          <Badge key={table.id} variant="outline">
                            {table.name}
                          </Badge>
                        ))}
                      </div>
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>
          )}
        </TabsContent>
      </Tabs>

      <TableFormDialog
        open={isTableFormOpen}
        onOpenChange={setIsTableFormOpen}
        table={editingTable}
        onSubmit={editingTable ? handleUpdateTable : handleCreateTable}
        isLoading={createTableMutation.isPending || updateTableMutation.isPending}
      />

      <TableCombinationDialog
        open={isCombinationFormOpen}
        onOpenChange={setIsCombinationFormOpen}
        tables={tables}
        onSubmit={handleCreateCombination}
        isLoading={createCombinationMutation.isPending}
      />
    </div>
  )
}
