<#
.SYNOPSIS
    Yerel gelistirme: Docker (Postgres + Redis), ardindan API, admin panel, super-admin ve booking UI'yi baslatir.
.DESCRIPTION
    Once 5086 / 3000 / 3001 / 5174 portlarinda dinleyen surecleri sonlandirir (zaten calisiyorsa),
    docker compose ile postgres ve redis'i ayaga kaldirir, sonra Tablewise.Api, admin-panel,
    super-admin ve booking-ui icin ayri terminal pencereleri acar.
.PARAMETER SkipDocker
    Postgres/Redis compose adimini atlar (servisleri zaten calisiyorsa kullanin).
#>
param(
    [switch]$SkipDocker
)

$ErrorActionPreference = 'Stop'
$RepoRoot = Split-Path -Parent $PSScriptRoot
$DockerDir = Join-Path $RepoRoot 'docker'
$ApiDir = Join-Path $RepoRoot 'src/Tablewise.Api'
$AdminDir = Join-Path $RepoRoot 'frontend/admin-panel'
$SuperAdminDir = Join-Path $RepoRoot 'frontend/super-admin'
$BookingDir = Join-Path $RepoRoot 'frontend/booking-ui'
$LandingDir = Join-Path $RepoRoot 'frontend/landing'

# Yerel gelistirme portlari (vite.config / launchSettings ile uyumlu)
$ApiPort = 5086
$AdminPort = 3000
$SuperAdminPort = 3001
$BookingPort = 5174
$LandingPort = 4000

function Stop-ListenerOnPort {
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
            Write-Host ('  {0} (:{1}) - calisan surec yok.' -f $Label, $Port)
            return
        }

        $pids = $connections | Select-Object -ExpandProperty OwningProcess -Unique
        foreach ($procId in $pids) {
            if ($procId -le 0) { continue }
            $proc = Get-Process -Id $procId -ErrorAction SilentlyContinue
            if ($proc) {
                Stop-Process -Id $procId -Force -ErrorAction SilentlyContinue
                $stopped += ('{0} (PID {1})' -f $proc.ProcessName, $procId)
            }
        }
    }
    catch {
        $lines = netstat -ano | Select-String (':{0}\s' -f $Port)
        foreach ($line in $lines) {
            if ($line -match '\s+(\d+)\s*$') {
                $procId = [int]$Matches[1]
                if ($procId -gt 0) {
                    Stop-Process -Id $procId -Force -ErrorAction SilentlyContinue
                    $stopped += ('PID {0}' -f $procId)
                }
            }
        }
    }

    if ($stopped.Count -gt 0) {
        Write-Host ('  {0} (:{1}) - durduruldu: {2}' -f $Label, $Port, ($stopped -join ', '))
    }
    else {
        Write-Host ('  {0} (:{1}) - calisan surec yok.' -f $Label, $Port)
    }
}

Write-Host ''
Write-Host 'Mevcut gelistirme surecleri kontrol ediliyor...'
Stop-ListenerOnPort -Port $ApiPort -Label 'API'
Stop-ListenerOnPort -Port $AdminPort -Label 'Admin panel'
Stop-ListenerOnPort -Port $SuperAdminPort -Label 'Super Admin'
Stop-ListenerOnPort -Port $BookingPort -Label 'Booking UI'
Stop-ListenerOnPort -Port $LandingPort -Label 'Landing'
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
        Write-Warning 'docker bulunamadi; Postgres ve Redis''i kendiniz baslatin.'
    }
}
else {
    Write-Host 'SkipDocker: compose adimi atlandi.'
}

if (-not (Test-Path (Join-Path $AdminDir 'node_modules'))) {
    Write-Warning ('admin-panel node_modules yok; once: cd ''{0}''; npm install' -f $AdminDir)
}

if (-not (Test-Path (Join-Path $SuperAdminDir 'node_modules'))) {
    Write-Warning ('super-admin node_modules yok; once: cd ''{0}''; npm install' -f $SuperAdminDir)
}

if (-not (Test-Path (Join-Path $BookingDir 'node_modules'))) {
    Write-Warning ('booking-ui node_modules yok; once: cd ''{0}''; npm install' -f $BookingDir)
}

if (-not (Test-Path (Join-Path $LandingDir 'node_modules'))) {
    Write-Warning ('landing node_modules yok; once: cd ''{0}''; npm install' -f $LandingDir)
}

$shellExe = if (Get-Command pwsh -ErrorAction SilentlyContinue) { (Get-Command pwsh).Source } else { (Get-Command powershell).Source }

$apiCmd = ('Set-Location -LiteralPath ''{0}''; dotnet run --launch-profile http' -f $ApiDir)
Start-Process -FilePath $shellExe -ArgumentList @('-NoExit', '-NoProfile', '-Command', $apiCmd)

$adminCmd = ('Set-Location -LiteralPath ''{0}''; npm run dev' -f $AdminDir)
Start-Process -FilePath $shellExe -ArgumentList @('-NoExit', '-NoProfile', '-Command', $adminCmd)

$superAdminCmd = ('Set-Location -LiteralPath ''{0}''; npm run dev' -f $SuperAdminDir)
Start-Process -FilePath $shellExe -ArgumentList @('-NoExit', '-NoProfile', '-Command', $superAdminCmd)

$bookingCmd = ('Set-Location -LiteralPath ''{0}''; npm run dev' -f $BookingDir)
Start-Process -FilePath $shellExe -ArgumentList @('-NoExit', '-NoProfile', '-Command', $bookingCmd)

$landingCmd = ('Set-Location -LiteralPath ''{0}''; npm run dev' -f $LandingDir)
Start-Process -FilePath $shellExe -ArgumentList @('-NoExit', '-NoProfile', '-Command', $landingCmd)

Write-Host ''
Write-Host ('API:     http://localhost:{0}' -f $ApiPort)
Write-Host ('Admin:       http://localhost:{0}' -f $AdminPort)
Write-Host ('Super Admin: http://localhost:{0}' -f $SuperAdminPort)
Write-Host ('Booking:     http://localhost:{0}/rezervasyon/{{slug}}' -f $BookingPort)
Write-Host ('Landing:     http://localhost:{0}' -f $LandingPort)
Write-Host ''
Write-Host 'Bes yeni terminal penceresi acildi; durdurmak icin her pencerede Ctrl+C kullanin.'
Write-Host 'Tekrar calistirirsaniz script once bu portlardaki surecleri kapatir.'
Write-Host ''
