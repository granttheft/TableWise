import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { Checkbox } from '@/components/ui/checkbox';
import { CustomFieldRenderer } from '@/components/CustomFieldRenderer';
import { RuleResultBanner } from '@/components/RuleResultBanner';
import { useEvaluate } from '@/hooks/useEvaluate';
import { useDebounce } from '@/hooks/useDebounce';
import type { VenueConfig, AvailabilitySlot } from '@/types/api';
import { parsePhoneNumber, toLocalDateString } from '@/lib/utils';

interface Step4InfoProps {
  config: VenueConfig;
  selectedDate: Date;
  selectedSlot: AvailabilitySlot;
  partySize: number;
  tableIds: string[] | null;
  formData: FormData;
  onFormDataChange: (data: FormData) => void;
  onNext: () => void;
  onBack: () => void;
}

export interface FormData {
  customerName: string;
  customerEmail: string;
  customerPhone: string;
  specialRequests?: string;
  customFieldValues: Record<string, string | number | boolean>;
  acceptsKvkk: boolean;
  whatsAppConsent: boolean;
}

const createSchema = (customFields: VenueConfig['customFields']) => {
  const customFieldValidation: Record<string, z.ZodTypeAny> = {};

  customFields.forEach((field) => {
    if (field.isRequired) {
      if (field.fieldType === 'text') {
        customFieldValidation[field.fieldKey] = z.string().min(1, 'Zorunlu alan');
      } else if (field.fieldType === 'number') {
        customFieldValidation[field.fieldKey] = z.number().min(0);
      } else if (field.fieldType === 'boolean') {
        customFieldValidation[field.fieldKey] = z.boolean();
      } else if (field.fieldType === 'date') {
        customFieldValidation[field.fieldKey] = z.string().min(1);
      } else if (field.fieldType === 'select') {
        customFieldValidation[field.fieldKey] = z.string().min(1);
      }
    } else {
      if (field.fieldType === 'text' || field.fieldType === 'date' || field.fieldType === 'select') {
        customFieldValidation[field.fieldKey] = z.string().optional();
      } else if (field.fieldType === 'number') {
        customFieldValidation[field.fieldKey] = z.number().optional();
      } else if (field.fieldType === 'boolean') {
        customFieldValidation[field.fieldKey] = z.boolean().optional();
      }
    }
  });

  return z.object({
    customerName: z.string().min(2, 'Ad soyad en az 2 karakter olmalı'),
    customerEmail: z.string().email('Geçerli bir e-posta adresi girin'),
    customerPhone: z
      .string()
      .min(1, 'Telefon numarası zorunludur')
      .refine(
        (val) => /^(\+90|0)?5[0-9]{9}$/.test(val.replace(/[\s\-()]/g, '')),
        'Geçerli bir Türkiye telefon numarası girin (örn: 05XX XXX XX XX)'
      ),
    specialRequests: z.string().optional(),
    customFieldValues: z.object(customFieldValidation),
    acceptsKvkk: z.literal(true, {
      errorMap: () => ({ message: 'KVKK onayı zorunludur' }),
    }),
    whatsAppConsent: z.boolean().optional().default(false),
  });
};

export function Step4Info({
  config,
  selectedDate,
  selectedSlot,
  partySize,
  tableIds,
  formData,
  onFormDataChange,
  onNext,
  onBack,
}: Step4InfoProps) {
  const schema = createSchema(config.customFields);
  type SchemaType = z.infer<typeof schema>;

  const {
    register,
    handleSubmit,
    watch,
    setValue,
    formState: { errors },
  } = useForm<SchemaType>({
    resolver: zodResolver(schema),
    defaultValues: {
      customerName: formData.customerName || '',
      customerEmail: formData.customerEmail || '',
      customerPhone: formData.customerPhone || '',
      specialRequests: formData.specialRequests || '',
      customFieldValues: formData.customFieldValues || {},
      acceptsKvkk: formData.acceptsKvkk || false,
      whatsAppConsent: formData.whatsAppConsent || false,
    } as SchemaType,
  });

  const watchedValues = watch();
  const debouncedValues = useDebounce(watchedValues, 500);

  // Build evaluation request
  const dateStr = toLocalDateString(selectedDate);
  const evaluationRequest =
    debouncedValues.customerName &&
    debouncedValues.customerEmail &&
    debouncedValues.customerPhone
      ? {
          date: dateStr,
          time: selectedSlot.time,
          partySize,
          tableIds: tableIds || undefined,
          customerEmail: debouncedValues.customerEmail,
          customerPhone: debouncedValues.customerPhone,
          customFieldValues: debouncedValues.customFieldValues,
        }
      : null;

  const { data: evaluationResult } = useEvaluate(
    config.slug,
    evaluationRequest,
    !!evaluationRequest
  );

  const canProceed = evaluationResult?.canProceed ?? true;
  const hasBlockingAction =
    evaluationResult?.actions.some((a) => a.actionType === 'Block') ?? false;

  const onSubmit = (data: SchemaType) => {
    onFormDataChange({
      ...data,
      specialRequests: data.specialRequests || '',
      whatsAppConsent: data.whatsAppConsent ?? false,
    });
    onNext();
  };

  const handleCustomFieldChange = (
    key: string,
    value: string | number | boolean
  ) => {
    setValue(`customFieldValues.${key}` as any, value, {
      shouldValidate: true,
    });
  };

  // Sort custom fields by displayOrder
  const sortedCustomFields = [...config.customFields].sort(
    (a, b) => a.displayOrder - b.displayOrder
  );

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <div>
        <h2 className="text-xl font-semibold mb-2">Rezervasyon Bilgileri</h2>
        <p className="text-sm text-muted-foreground">
          Lütfen iletişim bilgilerinizi ve gerekli diğer bilgileri doldurun
        </p>
      </div>

      {/* Standard fields */}
      <div className="space-y-4">
        <div>
          <Label htmlFor="customerName">
            Ad Soyad <span className="text-red-500">*</span>
          </Label>
          <Input
            id="customerName"
            {...register('customerName')}
            className="mt-1 h-11"
          />
          {errors.customerName && (
            <p className="text-sm text-red-600 mt-1">
              {errors.customerName.message}
            </p>
          )}
        </div>

        <div>
          <Label htmlFor="customerEmail">
            E-posta <span className="text-red-500">*</span>
          </Label>
          <Input
            id="customerEmail"
            type="email"
            inputMode="email"
            {...register('customerEmail')}
            className="mt-1 h-11"
          />
          {errors.customerEmail && (
            <p className="text-sm text-red-600 mt-1">
              {errors.customerEmail.message}
            </p>
          )}
        </div>

        <div>
          <Label htmlFor="customerPhone">
            Telefon <span className="text-red-500">*</span>
          </Label>
          <Input
            id="customerPhone"
            type="tel"
            inputMode="tel"
            placeholder="05XX XXX XX XX"
            {...register('customerPhone', {
              setValueAs: (value) => parsePhoneNumber(value),
            })}
            className="mt-1 h-11"
          />
          {errors.customerPhone && (
            <p className="text-sm text-red-600 mt-1">
              {errors.customerPhone.message}
            </p>
          )}
        </div>

        <div>
          <Label htmlFor="specialRequests">Özel İstek (Opsiyonel)</Label>
          <Textarea
            id="specialRequests"
            {...register('specialRequests')}
            className="mt-1"
            rows={3}
            placeholder="Özel bir isteğiniz varsa buraya yazabilirsiniz..."
          />
        </div>
      </div>

      {/* Custom fields */}
      {sortedCustomFields.length > 0 && (
        <div className="space-y-4 border-t pt-4">
          {sortedCustomFields.map((field) => {
            const value = watchedValues.customFieldValues?.[field.fieldKey];

            return (
              <div key={field.id}>
                <CustomFieldRenderer
                  field={field}
                  value={value}
                  onChange={(val) =>
                    handleCustomFieldChange(
                      field.fieldKey,
                      val as string | number | boolean
                    )
                  }
                  allFields={sortedCustomFields}
                  allValues={watchedValues.customFieldValues}
                  onFieldChange={handleCustomFieldChange}
                />
                {errors.customFieldValues?.[field.fieldKey] && (
                  <p className="text-sm text-red-600 mt-1">
                    {String(errors.customFieldValues[field.fieldKey]?.message || '')}
                  </p>
                )}
              </div>
            );
          })}
        </div>
      )}

      {/* Rule evaluation results */}
      {evaluationResult && evaluationResult.actions.length > 0 && (
        <div className="border-t pt-4">
          <RuleResultBanner actions={evaluationResult.actions} />
        </div>
      )}

      {/* KVKK checkbox */}
      <div className="flex items-start gap-2 border-t pt-4">
        <Checkbox
          id="acceptsKvkk"
          checked={watchedValues.acceptsKvkk === true}
          onCheckedChange={(checked) => setValue('acceptsKvkk', checked === true ? true as const : false as any)}
          className="mt-1"
        />
        <Label htmlFor="acceptsKvkk" className="text-sm leading-relaxed cursor-pointer">
          Kişisel verilerimin işlenmesini ve gizlilik politikasını
          onaylıyorum. <span className="text-red-500">*</span>
        </Label>
      </div>
      {errors.acceptsKvkk && (
        <p className="text-sm text-red-600 -mt-2">
          {errors.acceptsKvkk.message}
        </p>
      )}

      {/* WhatsApp rızası — KVKK uyumu */}
      <div className="flex items-start gap-2 p-3 bg-green-50 rounded-lg border border-green-100">
        <Checkbox
          id="whatsAppConsent"
          checked={watchedValues.whatsAppConsent === true}
          onCheckedChange={(checked) =>
            setValue('whatsAppConsent', checked === true)
          }
          className="mt-0.5"
        />
        <Label htmlFor="whatsAppConsent" className="text-sm leading-relaxed cursor-pointer text-gray-700">
          WhatsApp üzerinden rezervasyon bildirimlerini (onay, hatırlatma, iptal) almak
          istiyorum.{' '}
          <a
            href="/gizlilik"
            target="_blank"
            rel="noopener noreferrer"
            className="text-green-600 underline underline-offset-2"
          >
            Gizlilik Politikası
          </a>
        </Label>
      </div>

      <div className="flex gap-3">
        <Button
          type="button"
          variant="outline"
          onClick={onBack}
          className="flex-1"
        >
          Geri
        </Button>
        <Button
          type="submit"
          disabled={hasBlockingAction || !canProceed}
          className="flex-1"
          size="lg"
        >
          Devam Et
        </Button>
      </div>
    </form>
  );
}
