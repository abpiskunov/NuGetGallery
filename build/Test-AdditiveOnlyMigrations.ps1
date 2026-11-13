<#
.SYNOPSIS
Fails if a new EF6 migration's Up() is not expand-only (see docs/shared-database-schema-policy.md).
Down() is ignored. Migrations with a timestamp <= BaselineMigrationId are grandfathered.
A reviewed exception requires a line "// shared-db: allow-breaking <reason>" in the migration file.
#>
param(
    [string]$MigrationsPath = (Join-Path $PSScriptRoot '..\src\NuGetGallery\Migrations'),
    [string]$BaselineMigrationId = '202608031928454'
)

$ErrorActionPreference = 'Stop'

$operations = 'DropTable|DropColumn|RenameColumn|RenameTable|RenameIndex|AlterColumn|DropIndex|DropForeignKey|DropPrimaryKey|MoveTable|DropStoredProcedure|RenameStoredProcedure|MoveStoredProcedure'
$sqlPattern = '(?i)\b(DROP\s+(TABLE|COLUMN|INDEX|CONSTRAINT|VIEW|PROCEDURE|FUNCTION)|sp_rename|ALTER\s+COLUMN|TRUNCATE\s+TABLE|DELETE\s+FROM|UPDATE\s+\[?\w+\]?\.?\[?\w+\]?\s+SET)\b'

function Get-UpBody([string]$text) {
    $m = [regex]::Match($text, 'public\s+override\s+void\s+Up\s*\(\s*\)\s*\{')
    if (-not $m.Success) { return $null }
    $depth = 1
    $i = $m.Index + $m.Length
    while ($i -lt $text.Length -and $depth -gt 0) {
        $c = $text[$i]
        if ($c -eq '{') { $depth++ } elseif ($c -eq '}') { $depth-- }
        $i++
    }
    return $text.Substring($m.Index + $m.Length, $i - $m.Index - $m.Length - 1)
}

$violations = @()
$files = Get-ChildItem $MigrationsPath -Filter '*.cs' |
    Where-Object { $_.Name -match '^(\d{15})_' -and $_.Name -notmatch '\.Designer\.cs$' }

foreach ($file in $files) {
    $id = ($file.Name -split '_')[0]
    if ([string]::Compare($id, $BaselineMigrationId, $false) -le 0) { continue }

    $text = Get-Content $file.FullName -Raw
    if ($text -match '//\s*shared-db:\s*allow-breaking\s+\S+') { continue }

    $up = Get-UpBody $text
    if ($null -eq $up) { continue }
    $up = [regex]::Replace($up, '(?m)^\s*//.*$', '')

    foreach ($m in [regex]::Matches($up, "\b($operations)\s*\(")) {
        $violations += "$($file.Name): Up() calls $($m.Groups[1].Value)"
    }
    foreach ($m in [regex]::Matches($up, $sqlPattern)) {
        $violations += "$($file.Name): Up() contains destructive SQL '$($m.Value)'"
    }
}

if ($violations) {
    Write-Host 'Non-additive schema changes detected. The legacy and Azure hosts share one database;'
    Write-Host 'use expand/contract (docs/shared-database-schema-policy.md).'
    $violations | ForEach-Object { Write-Host "  $_" }
    exit 1
}

Write-Host "Additive-only check passed ($($files.Count) migrations scanned; baseline $BaselineMigrationId)."
