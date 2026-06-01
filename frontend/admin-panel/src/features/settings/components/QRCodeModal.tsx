import { useRef } from 'react'
import { QRCodeSVG } from 'qrcode.react'
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { Download, Copy } from 'lucide-react'
import { toast } from 'sonner'

interface QRCodeModalProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  url: string
}

export function QRCodeModal({ open, onOpenChange, url }: QRCodeModalProps) {
  const svgRef = useRef<SVGSVGElement>(null)

  const handleCopy = () => {
    navigator.clipboard.writeText(url)
    toast.success('Link kopyalandı')
  }

  const handleDownload = () => {
    if (!svgRef.current) return

    const svg = svgRef.current
    const serializer = new XMLSerializer()
    const svgStr = serializer.serializeToString(svg)
    const blob = new Blob([svgStr], { type: 'image/svg+xml' })
    const url = URL.createObjectURL(blob)

    const a = document.createElement('a')
    a.href = url
    a.download = 'rezervasyon-qr.svg'
    a.click()
    URL.revokeObjectURL(url)
    toast.success('QR kod indirildi')
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>QR Kod</DialogTitle>
        </DialogHeader>

        <div className="flex flex-col items-center gap-6 py-4">
          <div className="p-4 bg-white rounded-lg border">
            <QRCodeSVG
              ref={svgRef}
              value={url}
              size={200}
              level="M"
              includeMargin={false}
            />
          </div>

          <p className="text-sm text-muted-foreground text-center break-all">{url}</p>

          <div className="flex gap-2 w-full">
            <Button variant="outline" className="flex-1" onClick={handleCopy}>
              <Copy className="mr-2 h-4 w-4" />
              Linki Kopyala
            </Button>
            <Button className="flex-1" onClick={handleDownload}>
              <Download className="mr-2 h-4 w-4" />
              İndir (SVG)
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  )
}
