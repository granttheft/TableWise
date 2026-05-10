import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Progress } from '@/components/ui/progress'
import { Badge } from '@/components/ui/badge'
import { Check, Copy, ChevronRight } from 'lucide-react'
import { toast } from 'sonner'

const STEPS = [
  {
    id: 1,
    title: 'İlk Masanızı Ekleyin',
    description: 'Rezervasyon almaya başlamak için en az bir masa tanımlayın',
  },
  {
    id: 2,
    title: 'Çalışma Saatlerinizi Ayarlayın',
    description: 'Hangi günlerde ve saatlerde açık olduğunuzu belirtin',
  },
  {
    id: 3,
    title: 'İlk Kuralınızı Seçin',
    description: 'Rezervasyon sürecinizi otomatikleştirin',
  },
  {
    id: 4,
    title: 'Hazırsınız!',
    description: 'Booking linkinizi paylaşın ve rezervasyon almaya başlayın',
  },
]

export function OnboardingPage() {
  const navigate = useNavigate()
  const [currentStep, setCurrentStep] = useState(1)
  const [completedSteps, setCompletedSteps] = useState<number[]>([])

  // Step 1 state
  const [tableName, setTableName] = useState('')
  const [tableCapacity, setTableCapacity] = useState(4)
  const [tableLocation, setTableLocation] = useState('Indoor')

  // Step 2 state
  const [openTime, setOpenTime] = useState('10:00')
  const [closeTime, setCloseTime] = useState('23:00')
  const [slotDuration, setSlotDuration] = useState('90')

  // Step 3 state
  const [selectedRule, setSelectedRule] = useState<string | null>(null)

  const bookingUrl = 'https://tablewise.com.tr/rezervasyon/demo-restoran'

  const handleNext = async () => {
    // Save current step
    console.log('Saving step:', currentStep)
    
    // Mark step as completed
    if (!completedSteps.includes(currentStep)) {
      setCompletedSteps([...completedSteps, currentStep])
    }

    if (currentStep < STEPS.length) {
      setCurrentStep(currentStep + 1)
    } else {
      // Finish onboarding
      console.log('Onboarding completed')
      // TODO: PATCH /api/v1/tenant/me { isFirstLogin: false }
      toast.success('Kurulum tamamlandı! Hoş geldiniz 🎉')
      navigate('/dashboard')
    }
  }

  const handleSkip = () => {
    if (!completedSteps.includes(currentStep)) {
      setCompletedSteps([...completedSteps, currentStep])
    }
    if (currentStep < STEPS.length) {
      setCurrentStep(currentStep + 1)
    } else {
      navigate('/dashboard')
    }
  }

  const handleCopyUrl = () => {
    navigator.clipboard.writeText(bookingUrl)
    toast.success('Booking URL kopyalandı')
  }

  const progressPercentage = (currentStep / STEPS.length) * 100

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-slate-900 via-slate-800 to-slate-900 p-4">
      <Card className="w-full max-w-2xl">
        <CardHeader>
          <div className="flex items-center justify-between mb-2">
            <Badge variant="secondary">Adım {currentStep} / {STEPS.length}</Badge>
            <Button variant="ghost" size="sm" onClick={handleSkip}>
              {currentStep === STEPS.length ? 'Dashboard\'a Git' : 'Atla'}
            </Button>
          </div>
          <Progress value={progressPercentage} className="mb-4" />
          <CardTitle className="text-2xl">{STEPS[currentStep - 1].title}</CardTitle>
          <CardDescription>{STEPS[currentStep - 1].description}</CardDescription>
        </CardHeader>

        <CardContent>
          {/* Step 1: Add First Table */}
          {currentStep === 1 && (
            <div className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="tableName">Masa Adı *</Label>
                <Input
                  id="tableName"
                  value={tableName}
                  onChange={(e) => setTableName(e.target.value)}
                  placeholder="Örn: Masa 1"
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="tableCapacity">Kapasite *</Label>
                  <Input
                    id="tableCapacity"
                    type="number"
                    min={1}
                    max={50}
                    value={tableCapacity}
                    onChange={(e) => setTableCapacity(parseInt(e.target.value) || 1)}
                  />
                </div>

                <div className="space-y-2">
                  <Label>Konum</Label>
                  <Select value={tableLocation} onValueChange={setTableLocation}>
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="Indoor">İç Mekan</SelectItem>
                      <SelectItem value="Outdoor">Dış Mekan</SelectItem>
                      <SelectItem value="Terrace">Teras</SelectItem>
                      <SelectItem value="Bar">Bar</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </div>
            </div>
          )}

          {/* Step 2: Working Hours */}
          {currentStep === 2 && (
            <div className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <div className="space-y-2">
                  <Label htmlFor="openTime">Açılış Saati</Label>
                  <Input
                    id="openTime"
                    type="time"
                    value={openTime}
                    onChange={(e) => setOpenTime(e.target.value)}
                  />
                </div>

                <div className="space-y-2">
                  <Label htmlFor="closeTime">Kapanış Saati</Label>
                  <Input
                    id="closeTime"
                    type="time"
                    value={closeTime}
                    onChange={(e) => setCloseTime(e.target.value)}
                  />
                </div>
              </div>

              <div className="space-y-2">
                <Label>Rezervasyon Süresi</Label>
                <Select value={slotDuration} onValueChange={setSlotDuration}>
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="60">60 dakika</SelectItem>
                    <SelectItem value="90">90 dakika</SelectItem>
                    <SelectItem value="120">120 dakika</SelectItem>
                  </SelectContent>
                </Select>
              </div>

              <p className="text-sm text-muted-foreground">
                Bu ayarları daha sonra Ayarlar sayfasından değiştirebilirsiniz
              </p>
            </div>
          )}

          {/* Step 3: Choose First Rule */}
          {currentStep === 3 && (
            <div className="space-y-4">
              <div className="grid gap-3">
                {[
                  {
                    id: 'early-booking',
                    title: 'Erken Rezervasyon İndirimi',
                    description: '7+ gün öncesinden rezervasyon yapanlara %10 indirim',
                  },
                  {
                    id: 'large-group',
                    title: 'Büyük Grup Önceliği',
                    description: '8+ kişilik rezervasyonlar için otomatik önceliklendirme',
                  },
                  {
                    id: 'deposit-required',
                    title: 'Kapora Zorunluluğu',
                    description: 'Cuma-Cumartesi geceleri için kapora talebi',
                  },
                ].map((rule) => (
                  <button
                    key={rule.id}
                    onClick={() => setSelectedRule(rule.id)}
                    className={`text-left p-4 rounded-lg border-2 transition-all ${
                      selectedRule === rule.id
                        ? 'border-primary bg-primary/5'
                        : 'border-muted hover:border-primary/50'
                    }`}
                  >
                    <div className="flex items-start justify-between">
                      <div>
                        <div className="font-medium">{rule.title}</div>
                        <div className="text-sm text-muted-foreground">{rule.description}</div>
                      </div>
                      {selectedRule === rule.id && (
                        <Check className="h-5 w-5 text-primary flex-shrink-0" />
                      )}
                    </div>
                  </button>
                ))}
              </div>

              <p className="text-sm text-muted-foreground">
                İstediğiniz kadar kural ekleyebilir ve özelleştirebilirsiniz
              </p>
            </div>
          )}

          {/* Step 4: Ready */}
          {currentStep === 4 && (
            <div className="space-y-6 text-center">
              <div className="inline-flex h-20 w-20 items-center justify-center rounded-full bg-green-500">
                <Check className="h-10 w-10 text-white" />
              </div>

              <div>
                <h3 className="text-xl font-bold mb-2">Tebrikler!</h3>
                <p className="text-muted-foreground">
                  Tablewise kurulumunuz tamamlandı. Artık rezervasyon almaya başlayabilirsiniz.
                </p>
              </div>

              <Card>
                <CardHeader>
                  <CardTitle className="text-base">Booking Linkiniz</CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="flex gap-2">
                    <Input value={bookingUrl} readOnly className="flex-1" />
                    <Button variant="outline" onClick={handleCopyUrl}>
                      <Copy className="mr-2 h-4 w-4" />
                      Kopyala
                    </Button>
                  </div>
                  <p className="text-sm text-muted-foreground mt-2">
                    Bu linki sosyal medyada, web sitenizde veya menünüzde paylaşın
                  </p>
                </CardContent>
              </Card>

              <div className="text-sm text-muted-foreground">
                Ayarlar → Booking sekmesinden QR kod oluşturabilir ve embed kodu alabilirsiniz
              </div>
            </div>
          )}

          <div className="flex justify-end gap-2 mt-6 pt-6 border-t">
            <Button onClick={handleNext} className="min-w-[120px]">
              {currentStep === STEPS.length ? 'Tamamla' : 'Devam Et'}
              {currentStep !== STEPS.length && <ChevronRight className="ml-2 h-4 w-4" />}
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
