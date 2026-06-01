import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Check, MapPin, Users } from 'lucide-react';
import { cn } from '@/lib/utils';
import type { AvailabilitySlot } from '@/types/api';

interface Step3TableProps {
  slot: AvailabilitySlot;
  selectedTableIds: string[] | null;
  onTableSelect: (tableIds: string[] | null) => void;
  onCombinationSelect?: (combinationId: string | null) => void;
  onNext: () => void;
  onBack: () => void;
}

export function Step3Table({
  slot,
  selectedTableIds,
  onTableSelect,
  onCombinationSelect,
  onNext,
  onBack,
}: Step3TableProps) {
  const isAutoSelect = selectedTableIds === null;

  const handleAutoSelect = () => {
    onTableSelect(null);
    onCombinationSelect?.(null);
  };

  const handleTableSelect = (tableIds: string[]) => {
    onTableSelect(tableIds);
    onCombinationSelect?.(null);
  };

  const handleCombinationSelect = (combinationId: string, tableIds: string[]) => {
    onTableSelect(tableIds);
    onCombinationSelect?.(combinationId);
  };

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-xl font-semibold mb-2">Masa Seçimi</h2>
        <p className="text-sm text-muted-foreground">
          İsterseniz belirli bir masa seçebilir, ya da sistem otomatik seçsin
        </p>
      </div>

      {/* Auto select option */}
      <Card
        className={cn(
          'cursor-pointer transition-all border-2',
          isAutoSelect
            ? 'border-primary bg-primary/5'
            : 'border-gray-200 hover:border-gray-300'
        )}
        onClick={handleAutoSelect}
      >
        <CardContent className="pt-6">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <div className="w-11 h-11 rounded-full bg-primary/10 flex items-center justify-center">
                <Users className="w-5 h-5 text-primary" />
              </div>
              <div>
                <h3 className="font-semibold">Sistem Otomatik Seçsin</h3>
                <p className="text-sm text-muted-foreground">
                  Size en uygun masayı biz seçelim
                </p>
              </div>
            </div>
            {isAutoSelect && (
              <div className="w-6 h-6 rounded-full bg-primary flex items-center justify-center">
                <Check className="w-4 h-4 text-white" />
              </div>
            )}
          </div>
        </CardContent>
      </Card>

      {/* Individual tables */}
      {slot.availableTables.length > 0 && (
        <div>
          <h3 className="font-semibold mb-3">Müsait Masalar</h3>
          <div className="space-y-2">
            {slot.availableTables.map((table) => {
              const isSelected =
                selectedTableIds?.length === 1 &&
                selectedTableIds[0] === table.tableId;

              return (
                <Card
                  key={table.tableId}
                  className={cn(
                    'cursor-pointer transition-all border-2',
                    isSelected
                      ? 'border-primary bg-primary/5'
                      : 'border-gray-200 hover:border-gray-300'
                  )}
                  onClick={() => handleTableSelect([table.tableId])}
                >
                  <CardContent className="py-4">
                    <div className="flex items-center justify-between">
                      <div>
                        <h4 className="font-semibold truncate">{table.tableName}</h4>
                        <div className="flex items-center gap-4 mt-1 text-sm text-muted-foreground">
                          <span className="flex items-center gap-1">
                            <Users className="w-4 h-4" />
                            {table.capacity} kişi
                          </span>
                          {table.location && (
                            <span className="flex items-center gap-1">
                              <MapPin className="w-4 h-4" />
                              {table.location}
                            </span>
                          )}
                        </div>
                      </div>
                      {isSelected && (
                        <div className="w-6 h-6 rounded-full bg-primary flex items-center justify-center">
                          <Check className="w-4 h-4 text-white" />
                        </div>
                      )}
                    </div>
                  </CardContent>
                </Card>
              );
            })}
          </div>
        </div>
      )}

      {/* Table combinations */}
      {slot.tableCombinations.length > 0 && (
        <div>
          <h3 className="font-semibold mb-3">Kombine Masalar</h3>
          <div className="space-y-2">
            {slot.tableCombinations.map((combo) => {
              const comboTableIds = combo.tables.map((t) => t.tableId);
              const isSelected =
                JSON.stringify(selectedTableIds?.sort()) ===
                JSON.stringify(comboTableIds.sort());

              return (
                <Card
                  key={combo.combinationId}
                  className={cn(
                    'cursor-pointer transition-all border-2',
                    isSelected
                      ? 'border-primary bg-primary/5'
                      : 'border-gray-200 hover:border-gray-300'
                  )}
                  onClick={() => handleCombinationSelect(combo.combinationId, comboTableIds)}
                >
                  <CardContent className="py-4">
                    <div className="flex items-center justify-between">
                      <div>
                        <h4 className="font-semibold break-words">
                          {combo.tables.map((t) => t.tableName).join(' + ')}
                        </h4>
                        <div className="flex items-center gap-1 mt-1 text-sm text-muted-foreground">
                          <Users className="w-4 h-4" />
                          Toplam {combo.totalCapacity} kişi
                        </div>
                      </div>
                      {isSelected && (
                        <div className="w-6 h-6 rounded-full bg-primary flex items-center justify-center">
                          <Check className="w-4 h-4 text-white" />
                        </div>
                      )}
                    </div>
                  </CardContent>
                </Card>
              );
            })}
          </div>
        </div>
      )}

      <div className="flex gap-3">
        <Button variant="outline" onClick={onBack} className="flex-1">
          Geri
        </Button>
        <Button onClick={onNext} className="flex-1" size="lg">
          Devam Et
        </Button>
      </div>
    </div>
  );
}
