import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import type { VenueCustomField } from '@/types/api';
import { GroupCompositionPicker } from './GroupCompositionPicker';

interface CustomFieldRendererProps {
  field: VenueCustomField;
  value: string | number | boolean | undefined;
  onChange: (value: string | number | boolean | undefined) => void;
  allFields?: VenueCustomField[];
  allValues?: Record<string, string | number | boolean>;
  onFieldChange?: (key: string, value: string | number | boolean) => void;
}

export function CustomFieldRenderer({
  field,
  value,
  onChange,
  allFields = [],
  allValues = {},
  onFieldChange,
}: CustomFieldRendererProps) {
  // Special handling for group_composition
  if (field.fieldKey === 'group_composition') {
    return (
      <GroupCompositionPicker
        field={field}
        value={value as string | undefined}
        onChange={onChange}
        customFieldValues={allValues}
        onCustomFieldChange={onFieldChange}
        allCustomFields={allFields}
      />
    );
  }

  // Skip male_count and female_count as they're rendered inside GroupCompositionPicker
  if (field.fieldKey === 'male_count' || field.fieldKey === 'female_count') {
    return null;
  }

  // Text field
  if (field.fieldType === 'text') {
    return (
      <div className="space-y-2">
        <Label htmlFor={field.fieldKey}>
          {field.fieldLabel}
          {field.isRequired && <span className="text-red-500 ml-1">*</span>}
        </Label>
        <Input
          id={field.fieldKey}
          type="text"
          value={(value as string) || ''}
          onChange={(e) => onChange(e.target.value)}
          required={field.isRequired}
        />
      </div>
    );
  }

  // Number field
  if (field.fieldType === 'number') {
    return (
      <div className="space-y-2">
        <Label htmlFor={field.fieldKey}>
          {field.fieldLabel}
          {field.isRequired && <span className="text-red-500 ml-1">*</span>}
        </Label>
        <Input
          id={field.fieldKey}
          type="number"
          inputMode="numeric"
          value={(value as number) || ''}
          onChange={(e) =>
            onChange(e.target.value ? parseFloat(e.target.value) : undefined)
          }
          required={field.isRequired}
        />
      </div>
    );
  }

  // Date field
  if (field.fieldType === 'date') {
    return (
      <div className="space-y-2">
        <Label htmlFor={field.fieldKey}>
          {field.fieldLabel}
          {field.isRequired && <span className="text-red-500 ml-1">*</span>}
        </Label>
        <Input
          id={field.fieldKey}
          type="date"
          value={(value as string) || ''}
          onChange={(e) => onChange(e.target.value)}
          required={field.isRequired}
        />
      </div>
    );
  }

  // Boolean field
  if (field.fieldType === 'boolean') {
    return (
      <div className="space-y-2">
        <Label>
          {field.fieldLabel}
          {field.isRequired && <span className="text-red-500 ml-1">*</span>}
        </Label>
        <div className="grid grid-cols-2 gap-3">
          <button
            type="button"
            onClick={() => onChange(true)}
            className={`p-4 rounded-lg border-2 font-medium transition-all ${
              value === true
                ? 'border-primary bg-primary/10'
                : 'border-gray-200 hover:border-gray-300'
            }`}
          >
            Evet
          </button>
          <button
            type="button"
            onClick={() => onChange(false)}
            className={`p-4 rounded-lg border-2 font-medium transition-all ${
              value === false
                ? 'border-primary bg-primary/10'
                : 'border-gray-200 hover:border-gray-300'
            }`}
          >
            Hayır
          </button>
        </div>
      </div>
    );
  }

  // Select field
  if (field.fieldType === 'select' && field.selectOptions) {
    return (
      <div className="space-y-2">
        <Label htmlFor={field.fieldKey}>
          {field.fieldLabel}
          {field.isRequired && <span className="text-red-500 ml-1">*</span>}
        </Label>
        <Select
          value={(value as string) || ''}
          onValueChange={(val) => onChange(val)}
          required={field.isRequired}
        >
          <SelectTrigger id={field.fieldKey}>
            <SelectValue placeholder="Seçiniz..." />
          </SelectTrigger>
          <SelectContent>
            {field.selectOptions.map((option) => (
              <SelectItem key={option} value={option}>
                {option}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>
    );
  }

  return null;
}
