<#
.SYNOPSIS
    Yerel geliştirme: Docker (Postgres + Redis), ardından API, admin panel ve booking UI'yi başlatır.
.DESCRIPTION
    Önce 5086 / 3000 / 5174 portlarında dinleyen süreçleri sonlandırır (zaten çalışıyorsa),
    docker compose ile postgres ve redis'i ayağa kaldırır, sonra Tablewise.Api, admin-panel ve
    booking-ui için ayrı terminal pencereleri açar.
.PARAMETER SkipDocker
    Postgres/Redis compose adımını atlar (servisleri zaten çalışıyorsa kullanın).
#>
param(
    [switch]$SkipDocker
)

$ErrorActionPreference = 'Stop'
$RepoRoot = Split-Path -Parent $PSScriptRoot
$DockerDir = Join-Path $RepoRoot 'docker'
$ApiDir = Join-Path $RepoRoot 'src/Tablewise.Api'
$AdminDir = Join-Path $RepoRoot 'frontend/admin-panel'
$BookingDir = Join-Path $RepoRoot 'frontend/booking-ui'

# Yerel geliştirme portları (vite.config / launchSettings ile uyumlu)
$ApiPort = 5086
$AdminPort = 3000
$BookingPort = 5174

function Stop-ListenerOnPort {
    # Belirtilen TCP portunda LISTEN durumundaki süreçleri sonlandırır.
    param(
        [Parameter(Mandatory = $true)]
        [int]$Port,

        [Parameter(Mandatory = $true)]
        [string]$Label
    )

    $stopped = @()
    try {
        $connections = Get-NetTCPConnection -LocalPort $Port -State Listen -ErrorAction SilentlyContinue
        if (-not $connections) {
            Write-Host "  $Label (:$Port) — çalışan süreç yok."
            return
        }

        $pids = $connections | Select-Object -ExpandProperty OwningProcess -Unique
        foreach ($procId in $pids) {
            if ($procId -le 0) { continue }
            $proc = Get-Process -Id $procId -ErrorAction SilentlyContinue
            if ($proc) {
                Stop-Process -Id $procId -Force -ErrorAction SilentlyContinue
                $stopped += "$($proc.ProcessName) (PID $procId)"
            }
        }
    }
    catch {
        # Get-NetTCPConnection bazı ortamlarda yok; netstat yedek yolu
        $lines = netstat -ano | Select-String ":\s*$Port\s"
        foreach ($line in $lines) {
            if ($line -match '\s+(\d+)\s*$') {
                $procId = [int]$Matches[1]
                if ($procId -gt 0) {
                    Stop-Process -Id $procId -Force -ErrorAction SilentlyContinue
                    $stopped += "PID $procId"
                }
            }
        }
    }

    if ($stopped.Count -gt 0) {
        Write-Host "  $Label (:$Port) — durduruldu: $($stopped -join ', ')"
    }
    else {
        Write-Host "  $Label (:$Port) — çalışan süreç yok."
    }
}

Write-Host ''
Write-Host 'Mevcut geliştirme süreçleri kontrol ediliyor...'
Stop-ListenerOnPort -Port $ApiPort -Label 'API'
Stop-ListenerOnPort -Port $AdminPort -Label 'Admin panel'
Stop-ListenerOnPort -Port $BookingPort -Label 'Booking UI'
Start-Sleep -Seconds 1
Write-Host ''

if (-not $SkipDocker) {
    if (Get-Command docker -ErrorAction SilentlyContinue) {
        Push-Location $DockerDir
        try {
            docker compose up -d postgres redis
        }
        finally {
            Pop-Location
        }
    }
    else {
        Write-Warning 'docker bulunamadı; Postgres ve Redis''i kendiniz başlatın (ör. docker compose).'
    }
}
else {
    Write-Host 'SkipDocker: compose adımı atlandı.'
}

if (-not (Test-Path (Join-Path $AdminDir 'node_modules'))) {
    Write-Warning "admin-panel node_modules yok; önce: cd '$AdminDir'; npm install"
}

if (-not (Test-Path (Join-Path $BookingDir 'node_modules'))) {
    Write-Warning "booking-ui node_modules yok; önce: cd '$BookingDir'; npm install"
}

$shellExe = if (Get-Command pwsh -ErrorAction SilentlyContinue) { (Get-Command pwsh).Source } else { (Get-Command powershell).Source }

$apiCmd = "Set-Location -LiteralPath '$ApiDir'; dotnet run --launch-profile http"
Start-Process -FilePath $shellExe -ArgumentList @('-NoExit', '-NoProfile', '-Command', $apiCmd)

$adminCmd = "Set-Location -LiteralPath '$AdminDir'; npm run dev"
Start-Process -FilePath $shellExe -ArgumentList @('-NoExit', '-NoProfile', '-Command', $adminCmd)

$bookingCmd = "Set-Location -LiteralPath '$BookingDir'; npm run dev"
Start-Process -FilePath $shellExe -ArgumentList @('-NoExit', '-NoProfile', '-Command', $bookingCmd)

Write-Host ''
Write-Host "API:     http://localhost:$ApiPort"
Write-Host "Admin:   http://localhost:$AdminPort"
Write-Host ('Booking: http://localhost:' + $BookingPort + '/rezervasyon/{slug}')
Write-Host ''
Write-Host 'Üç yeni terminal penceresi açıldı; durdurmak için her pencerede Ctrl+C kullanın.'
Write-Host 'Tekrar çalıştırırsanız script önce bu portlardaki süreçleri kapatır.'
Write-Host ''
