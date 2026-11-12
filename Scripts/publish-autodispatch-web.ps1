# publish-autodispatch-web.ps1 — публикация AutoDispatch.Web на 67.220.74.190 (копия AutoCall V3 publish-autocall-web.ps1).
# Шаги: фронт (npm) → dotnet publish → Docker runtime-образ через удалённый context → перезапуск контейнера.
# Запуск из любого места: .\Scripts\publish-autodispatch-web.ps1 [-Tag 2026.11.11-1] [-SkipNpm]

param(
    [string]$Tag = "",
    [switch]$SkipNpm
)

# "Continue": PowerShell 5.1 при "Stop" роняет скрипт на любом stderr native-команд (npm warn); ошибки ловим по $LASTEXITCODE
$ErrorActionPreference = "Continue"

# --- paths ---
$repo       = Split-Path -Parent $PSScriptRoot
$serverProj = Join-Path $repo "Sources\Web\AutoDispatch.Web.Server"
$dockerDir  = Join-Path $repo "Deploy\AutoDispatch.Web"
$publishDir = Join-Path $dockerDir "publish"

# --- docker ---
$DockerContext = "ububuilder"           # docker context ls
$image         = "autodispatch.web"
$container     = "autodispatch.web"
$hostPort      = "127.0.0.1:5005"       # nginx → 127.0.0.1:5005 (Deploy/nginx/dispatch.myautocall.net.conf)
$hostRoot      = "/home/autodispatch"   # logs/, keys/, download/, files/, config/appsettings.Production.json (Deploy/server-setup.txt)

if ($Tag -eq "") {
    $git = (git -C $repo describe --tags --always 2>$null)
    $Tag = if ($git) { $git } else { Get-Date -Format "yyyy.MM.dd-HHmm" }
}

Write-Host "== AutoDispatch.Web $Tag =="

# --- frontend ---
if (-not $SkipNpm) {
    Write-Host "== npm build =="
    Push-Location $serverProj
    try {
        npm ci --no-audit --no-fund
        if ($LASTEXITCODE -ne 0) { throw "npm ci failed" }
        npm run build
        if ($LASTEXITCODE -ne 0) { throw "npm run build failed" }
    } finally {
        Pop-Location
    }
}

# --- dotnet publish ---
Write-Host "== dotnet publish =="
if (Test-Path $publishDir) { Remove-Item -Recurse -Force $publishDir }
dotnet publish $serverProj -c Release -o $publishDir -p:InformationalVersion=$Tag --nologo
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed" }
# секреты только на сервере: appsettings.Production.json монтируется volume-ом
Remove-Item -Force (Join-Path $publishDir "appsettings.Development.json") -ErrorAction SilentlyContinue
Remove-Item -Force (Join-Path $publishDir "appsettings.Production.json") -ErrorAction SilentlyContinue

# --- docker build on remote daemon ---
Write-Host "== docker build on '$DockerContext' =="
$previousContext = (docker context show)
docker context use $DockerContext | Out-Null
try {
    Push-Location $dockerDir
    try {
        docker build -t "${image}:$Tag" -t "${image}:latest" .
        if ($LASTEXITCODE -ne 0) { throw "docker build failed" }
    } finally {
        Pop-Location
    }

    Write-Host "== restart container =="
    docker rm -f $container 2>$null | Out-Null
    docker run -d `
        --name $container `
        --restart unless-stopped `
        -p "${hostPort}:8080" `
        -v "${hostRoot}/logs:/logs" `
        -v "${hostRoot}/keys:/keys" `
        -v "${hostRoot}/download:/download" `
        -v "${hostRoot}/files:/files" `
        -v "${hostRoot}/config/appsettings.Production.json:/app/appsettings.Production.json:ro" `
        "${image}:$Tag"
    if ($LASTEXITCODE -ne 0) { throw "docker run failed" }

    Start-Sleep -Seconds 5
    Write-Host ""
    Write-Host "== status =="
    docker ps --filter "name=$container" --format "table {{.Names}}\t{{.Image}}\t{{.Status}}\t{{.Ports}}"
    docker logs --tail 15 $container
} finally {
    docker context use $previousContext | Out-Null
}

Write-Host ""
Write-Host "Done. Check: curl -s https://dispatch.myautocall.net/Test"
Write-Host "Rollback: docker --context $DockerContext run ... ${image}:<previous tag>  (docker --context $DockerContext images $image)"
