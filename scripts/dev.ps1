<#
.SYNOPSIS
    Yerel geliştirme: Docker (Postgres + Redis), ardından API ve admin paneli ayrı pencerelerde başlatır.
.DESCRIPTION
    Önce docker/docker-compose ile postgres ve redis konteynerlerini ayağa kaldırır, sonra
    Tablewise.Api (dotnet run) ve admin-panel (npm run dev) için yeni terminal pencereleri açar.
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
    Write-Warning "node_modules yok; önce şunu çalıştırın: cd '$AdminDir'; npm install"
}

$shellExe = if (Get-Command pwsh -ErrorAction SilentlyContinue) { (Get-Command pwsh).Source } else { (Get-Command powershell).Source }

$apiCmd = "Set-Location -LiteralPath '$ApiDir'; dotnet run --launch-profile http"
Start-Process -FilePath $shellExe -ArgumentList @('-NoExit', '-NoProfile', '-Command', $apiCmd)

$feCmd = "Set-Location -LiteralPath '$AdminDir'; npm run dev"
Start-Process -FilePath $shellExe -ArgumentList @('-NoExit', '-NoProfile', '-Command', $feCmd)

Write-Host ''
Write-Host 'API: http://localhost:5086  |  Admin: http://localhost:3000'
Write-Host 'İki yeni terminal penceresi açıldı; durdurmak için her pencerede Ctrl+C kullanın.'
Write-Host ''
