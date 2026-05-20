import { useState } from 'react';
import { cn } from '@/lib/utils';
import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';
import type { VenueCustomField } from '@/types/api';

interface GroupCompositionPickerProps {
  field: VenueCustomField;
  value: string | undefined;
  onChange: (value: string | undefined) => void;
  customFieldValues?: Record<string, string | number | boolean>;
  onCustomFieldChange?: (key: string, value: string | number) => void;
  allCustomFields?: VenueCustomField[];
}

type GroupType = 'mixed' | 'male' | 'female' | 'family' | null;

const groupOptions = [
  {
    type: 'mixed' as GroupType,
    emoji: '👫',
    label: 'Karma Grup',
    subtitle: '(Kadın+Erkek)',
  },
  {
    type: 'male' as GroupType,
    emoji: '👨‍👨‍👦',
    label: 'Erkek Grubu',
    subtitle: '',
  },
  {
    type: 'female' as GroupType,
    emoji: '👩‍👩‍👦',
    label: 'Kadın Grubu',
    subtitle: '',
  },
  {
    type: 'family' as GroupType,
    emoji: '👨‍👩‍👧‍👦',
    label: 'Aile',
    subtitle: '',
  },
];

export function GroupCompositionPicker({
  field,
  value,
  onChange,
  customFieldValues = {},
  onCustomFieldChange,
  allCustomFields = [],
}: GroupCompositionPickerProps) {
  const [selected, setSelected] = useState<GroupType>(
    (value as GroupType) || null
  );

  const handleSelect = (type: GroupType) => {
    setSelected(type);
    onChange(type || undefined);
  };

  const handleSkip = () => {
    setSelected(null);
    onChange(undefined);
  };

  // Check if male_count and female_count fields exist
  const hasMaleCountField = allCustomFields.some(
    (f) => f.fieldKey === 'male_count'
  );
  const hasFemaleCountField = allCustomFields.some(
    (f) => f.fieldKey === 'female_count'
  );

  const showGenderCounts =
    selected === 'mixed' && hasMaleCountField && hasFemaleCountField;

  const maleCount = customFieldValues['male_count'] as number | undefined;
  const femaleCount = customFieldValues['female_count'] as number | undefined;

  return (
    <div className="space-y-4">
      <div>
        <Label className="text-base font-semibold">
          {field.fieldLabel}
          {field.isRequired && <span className="text-red-500 ml-1">*</span>}
        </Label>
        <p className="text-sm text-muted-foreground mt-1">
          Bu bilgi rezervasyon deneyiminizi iyileştirmek için kullanılır.
        </p>
      </div>

      {/* Grid of 4 cards */}
      <div className="grid grid-cols-2 gap-3">
        {groupOptions.map((option) => (
          <button
            key={option.type}
            type="button"
            onClick={() => handleSelect(option.type)}
            className={cn(
              'flex flex-col items-center justify-center p-6 rounded-lg border-2 transition-all min-h-[120px] hover:scale-105',
              selected === option.type
                ? 'border-amber-500 bg-amber-50'
                : 'border-gray-200 bg-white hover:border-gray-300'
            )}
          >
            <span className="text-4xl mb-2">{option.emoji}</span>
            <span className="font-medium text-sm text-center">
              {option.label}
            </span>
            {option.subtitle && (
              <span className="text-xs text-muted-foreground text-center mt-1">
                {option.subtitle}
              </span>
            )}
          </button>
        ))}
      </div>

      {/* Gender count inputs for mixed group */}
      {showGenderCounts && (
        <div className="flex gap-3 items-end animate-in fade-in slide-in-from-top-2 duration-200">
          <div className="flex-1">
            <Label htmlFor="male_count" className="text-sm">
              Kaç erkek?
            </Label>
            <Input
              id="male_count"
              type="number"
              inputMode="numeric"
              min="0"
              value={maleCount || ''}
              onChange={(e) => {
                const val = e.target.value ? parseInt(e.target.value, 10) : 0;
                onCustomFieldChange?.('male_count', val);
              }}
              className="mt-1"
            />
          </div>
          <div className="flex-1">
            <Label htmlFor="female_count" className="text-sm">
              Kaç kadın?
            </Label>
            <Input
              id="female_count"
              type="number"
              inputMode="numeric"
              min="0"
              value={femaleCount || ''}
              onChange={(e) => {
                const val = e.target.value ? parseInt(e.target.value, 10) : 0;
                onCustomFieldChange?.('female_count', val);
              }}
              className="mt-1"
            />
          </div>
        </div>
      )}

      {/* Skip option if not required */}
      {!field.isRequired && (
        <div className="text-center">
          <button
            type="button"
            onClick={handleSkip}
            className="text-sm text-muted-foreground hover:text-foreground underline"
          >
            Belirtmek istemiyorum
          </button>
        </div>
      )}
    </div>
  );
}
