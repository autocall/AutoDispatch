# Документы локальной копии компании (DataBase/copy company to local.sql) с хоста Ubuntu в FilesPath локального Web
# (AppSettings.Environment.Local.cs: E:\AutoDispatch\Files\). Только каталоги {EntityId} строк autodispatch_local —
# тот же список, что блок 5 DataBase/migrate from autocall dispatch.sql. AI/wiki/data-migration.md «Локальная копия компании».
#
# Источник — копия E:\Files V1 на хосте (/home/safer/temp/Files, FileZilla; после copy-dispatch-files.sh — /home/autodispatch/files).
# Регистр имён на хосте не важен: список каталогов источника сверяется без учёта регистра, локально (NTFS) регистр тоже не важен.
# Только чтение на хосте: ls, du, tar -c. Уже скачанные каталоги пропускаются — повторный запуск докачивает.
# Бинарный pipe — через cmd (PowerShell 5.1 портит его), как в sync-dispatch-files.ps1.
#
#   .\pull-local-files.ps1 -SizeOnly        # сколько каталогов найдено на хосте и их размер
#   .\pull-local-files.ps1                  # скачать
#   .\pull-local-files.ps1 -ActiveLoadsOnly # грузы только не в статусе Paid (меньше объём)

param(
    [string]$SshHost = "root@67.220.74.190",
    [string]$Source = "/home/safer/temp/Files",
    [string]$Target = "E:\AutoDispatch\Files",
    [string]$SqlServer = "(local)\SQLEXPRESS",
    [string]$Database = "autodispatch_local",
    [int]$BatchSize = 500,
    [switch]$SizeOnly,
    [switch]$ActiveLoadsOnly
)

$ErrorActionPreference = "Stop"
$utf8 = New-Object System.Text.UTF8Encoding $false
$work = Join-Path $env:TEMP "pull-local-files"
New-Item -ItemType Directory -Force $work | Out-Null

# 1. нужные каталоги — по строкам локальной БД (блок 5)
$loadFilter = if ($ActiveLoadsOnly) { "IsDeleted = 0 and Status <> 6" } else { "IsDeleted = 0" }
$query = @"
set nocount on;
select lower(convert(varchar(36), EntityId)) from dbo.Load where $loadFilter
union select lower(convert(varchar(36), EntityId)) from dbo.DatCompany      where IsDeleted = 0
union select lower(convert(varchar(36), EntityId)) from dbo.SetupPacket     where IsDeleted = 0
union select lower(convert(varchar(36), EntityId)) from dbo.Driver          where IsDeleted = 0
union select lower(convert(varchar(36), EntityId)) from dbo.Truck           where IsDeleted = 0
union select lower(convert(varchar(36), EntityId)) from dbo.Trailer         where IsDeleted = 0
union select lower(convert(varchar(36), EntityId)) from dbo.Defect          where IsDeleted = 0
union select lower(convert(varchar(36), EntityId)) from dbo.DefectRepair    where IsDeleted = 0
union select lower(convert(varchar(36), EntityId)) from dbo.DispatchCompany where IsDeleted = 0;
"@
$needed = @(sqlcmd -S $SqlServer -d $Database -h -1 -W -b -Q $query | Where-Object { $_ -match '^[0-9a-f-]{36}$' })
if ($LASTEXITCODE -ne 0) { throw "sqlcmd failed" }
Write-Host "Нужно каталогов (строки $Database): $($needed.Count)"

# 2. какие из них есть на хосте — имя как на хосте (регистр V1 может отличаться)
$remote = @(ssh $SshHost "ls -1 '$Source'")
if ($LASTEXITCODE -ne 0) { throw "ssh ls $Source failed" }
$byLower = @{}
foreach ($name in $remote) { $byLower[$name.ToLowerInvariant()] = $name }
$found = @($needed | Where-Object { $byLower.ContainsKey($_) } | ForEach-Object { $byLower[$_] })
Write-Host "Есть на хосте: $($found.Count) (у остальных документов нет)"

# 3. уже скачанные пропускаются
$todo = @($found | Where-Object { -not (Test-Path (Join-Path $Target $_)) })
Write-Host "Ещё не скачано: $($todo.Count)"
if ($todo.Count -eq 0) { return }

$list = Join-Path $work "list.txt"
if ($SizeOnly) {
    [IO.File]::WriteAllText($list, (($todo -join "`n") + "`n"), $utf8)
    cmd /c "type `"$list`" | ssh $SshHost `"cd '$Source' && xargs -d '\n' du -sc -- | tail -1`""
    return
}

# 4. скачивание пачками: tar на хосте -> tar локально
New-Item -ItemType Directory -Force $Target | Out-Null
for ($i = 0; $i -lt $todo.Count; $i += $BatchSize) {
    $batch = $todo[$i..([Math]::Min($i + $BatchSize, $todo.Count) - 1)]
    [IO.File]::WriteAllText($list, (($batch -join "`n") + "`n"), $utf8)
    cmd /c "type `"$list`" | ssh $SshHost `"cd '$Source' && tar -cf - -T -`" | tar -xf - -C `"$Target`""
    if ($LASTEXITCODE -ne 0) { throw "tar | ssh failed on batch starting at $i" }
    Write-Host ("{0}/{1}" -f ([Math]::Min($i + $BatchSize, $todo.Count)), $todo.Count)
}
Write-Host "Готово: $Target"
