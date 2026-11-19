# publish-autodispatch-app.ps1 — ClickOnce-публикация AutoDispatch App (AutoCall.WPF, AssemblyName AutoDispatch) на 67.220.74.190.
# Копия AutoCall V3 Scripts/publish-autocall-app.ps1: ревизия в ClickOnceProfile.pubxml +1 → msbuild /t:Publish → tar | ssh
# в /home/autodispatch/download (= https://dispatch.myautocall.net/Download/, volume /download контейнера autodispatch.web).
# dotnet publish ClickOnce не умеет и AutoPhoneV2 (COMReference) собирает только MSBuild из Visual Studio.
# Запуск из любого места: .\Scripts\publish-autodispatch-app.ps1 [-SkipUpload]

param(
    [switch]$SkipUpload
)

# "Continue": PowerShell 5.1 при "Stop" роняет скрипт на любом stderr native-команд; ошибки ловим по $LASTEXITCODE
$ErrorActionPreference = "Continue"

# --- paths ---
$repo       = Split-Path -Parent $PSScriptRoot
$viewProj   = Join-Path $repo "Sources\App\AutoCall.WPF\AutoCall.WPF.csproj"
$pubxml     = Join-Path $repo "Sources\App\AutoCall.WPF\Properties\PublishProfiles\ClickOnceProfile.pubxml"
$publishDir = "D:\Publish\AutoDispatch_Publish"
# vswhere -latest: VS 18 (SDK .NET 10), а не VS 2022 — её MSBuild берёт SDK 9 и не собирает net10
$msbuild    = & "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * `
    -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe"
if (-not $msbuild) { throw "MSBuild.exe not found (Visual Studio required)" }

# --- server ---
$sshHost  = "root@67.220.74.190"            # тот же хост, что docker context ububuilder
$hostDir  = "/home/autodispatch/download"   # volume контейнера /download → /Download (Deploy/server-setup.txt)

# --- revision: как VS — ApplicationRevision +1, MinimumRequiredVersion = новая версия (UpdateRequired) ---
[xml]$profile = Get-Content $pubxml
$props    = $profile.Project.PropertyGroup
if ([string]::IsNullOrWhiteSpace($props.ManifestCertificateThumbprint)) {
    throw "ManifestCertificateThumbprint is empty in ClickOnceProfile.pubxml: create the signing certificate first (VS: project properties -> Signing)"
}
$revision = [int]$props.ApplicationRevision
$version  = $props.ApplicationVersion -replace '\*$', $revision
$props.MinimumRequiredVersion = $version
$props.ApplicationRevision    = [string]($revision + 1)
$profile.Save($pubxml)

Write-Host "== AutoDispatch $version =="

# --- msbuild publish ---
Write-Host "== msbuild publish =="
& $msbuild $viewProj /restore /t:Publish /p:Configuration=Release /p:PublishProfile=ClickOnceProfile `
    /p:ApplicationRevision=$revision /p:MinimumRequiredVersion=$version /nologo /v:m
if ($LASTEXITCODE -ne 0) { throw "msbuild publish failed" }

# --- copy ClickOnce files to $publishDir (как VS копирует в PublishUrl) ---
# в PublishDir профиля (bin\...\app.publish) кроме ClickOnce-файлов лежит обычный publish-выход (dll, папки локалей) — его не берём;
# $publishDir не чистим: предыдущие Application Files/AutoDispatch_* остаются как локальный архив
$appPublish  = Join-Path (Split-Path $viewProj) $props.PublishDir
$versionName = "AutoDispatch_" + ($version -replace '\.', '_')
$versionDir  = Join-Path $publishDir "Application Files\$versionName"
if (Test-Path $versionDir) { Remove-Item -Recurse -Force $versionDir }
New-Item -ItemType Directory -Force (Join-Path $publishDir "Application Files") | Out-Null
Copy-Item -Recurse (Join-Path $appPublish "Application Files\$versionName") $versionDir
# Publish.html генерирует VS, а не MSBuild; при публикации скриптом остаётся от прошлого раза
foreach ($f in "AutoDispatch.application", "setup.exe", "Publish.html") {
    $src = Join-Path $appPublish $f
    if (Test-Path $src) { Copy-Item $src $publishDir }
}

if ($SkipUpload) {
    Write-Host "Published to $publishDir (upload skipped)"
    return
}

# --- upload: только новая версия + манифесты, старые Application Files на сервере не трогаем ---
Write-Host "== upload to ${sshHost}:$hostDir =="
# tar | ssh через cmd: scp на Windows ломает путь с пробелом ("Application Files"), а PowerShell 5.1 портит бинарный pipe
$rootFiles = "AutoDispatch.application", "setup.exe", "Publish.html" | Where-Object { Test-Path (Join-Path $publishDir $_) }
$items = (@("""Application Files/$versionName""") + $rootFiles) -join " "
cmd /c "tar -C ""$publishDir"" -cf - $items | ssh $sshHost ""tar -C $hostDir -xf -"""
if ($LASTEXITCODE -ne 0) { throw "upload failed" }

Write-Host ""
Write-Host "Done. Check: curl -sI https://dispatch.myautocall.net/Download/AutoDispatch.application"
Write-Host "Rollback: ClickOnce does not downgrade, checkout previous commit and publish again (new revision)"
