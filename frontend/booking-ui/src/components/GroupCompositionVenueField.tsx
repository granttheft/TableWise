import type { GroupCompositionBookingSelection, GroupCompositionBookingValue } from '../types/groupCompositionBooking'

export interface GroupCompositionVenueFieldProps {
  /** Alan etiketi (ör. venue custom field label). */
  label: string
  value: GroupCompositionBookingSelection
  onChange: (value: GroupCompositionBookingSelection) => void
  required?: boolean
  disabled?: boolean
  errorMessage?: string
}

const OPTIONS: {
  value: GroupCompositionBookingValue
  title: string
  symbol: string
}[] = [
  { value: 'Mixed', title: 'Karma', symbol: '👫' },
  { value: 'AllMale', title: 'Sadece ♂', symbol: '👨' },
  { value: 'AllFemale', title: 'Sadece ♀', symbol: '👩' },
  { value: 'Family', title: 'Aile', symbol: '👨‍👩‍👧' },
]

/**
 * VenueCustomFields içinde `field_key === "group_composition"` için kart seçici.
 * Seçim opsiyoneldir; boş değer "Belirtmek istemiyorum" anlamına gelir.
 */
export function GroupCompositionVenueField({
  label,
  value,
  onChange,
  required,
  disabled,
  errorMessage,
}: GroupCompositionVenueFieldProps) {
  return (
    <fieldset disabled={disabled} className="space-y-3">
      <legend className="text-sm font-medium text-neutral-900 dark:text-neutral-100">
        {label}
        {required ? <span className="text-red-500"> *</span> : null}
      </legend>
      <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
        {OPTIONS.map((opt) => {
          const selected = value === opt.value
          return (
            <button
              key={opt.value}
              type="button"
              onClick={() => onChange(selected ? '' : opt.value)}
              className={[
                'flex flex-col items-center justify-center rounded-xl border-2 p-4 text-center transition-colors',
                selected
                  ? 'border-pink-500 bg-pink-500/10 text-pink-900 shadow-sm dark:border-pink-400 dark:bg-pink-500/15 dark:text-pink-50'
                  : 'border-neutral-200 bg-white hover:border-pink-300 hover:bg-pink-50/40 dark:border-neutral-700 dark:bg-neutral-900 dark:hover:border-pink-500/50',
                disabled ? 'cursor-not-allowed opacity-60' : 'cursor-pointer',
              ].join(' ')}
            >
              <span className="text-3xl leading-none" aria-hidden>
                {opt.symbol}
              </span>
              <span className="mt-2 text-sm font-medium">{opt.title}</span>
            </button>
          )
        })}
      </div>
      {!required ? (
        <button
          type="button"
          onClick={() => onChange('')}
          className={[
            'w-full rounded-lg border px-3 py-2 text-sm transition-colors',
            value === ''
              ? 'border-pink-500 bg-pink-500/10 font-medium text-pink-900 dark:text-pink-50'
              : 'border-neutral-200 bg-neutral-50 text-neutral-700 hover:bg-neutral-100 dark:border-neutral-700 dark:bg-neutral-900 dark:text-neutral-200',
            disabled ? 'cursor-not-allowed opacity-60' : '',
          ].join(' ')}
        >
          Belirtmek istemiyorum
        </button>
      ) : null}
      {errorMessage ? <p className="text-sm text-red-600">{errorMessage}</p> : null}
    </fieldset>
  )
}
