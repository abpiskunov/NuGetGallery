# Self-test for build/Test-AdditiveOnlyMigrations.ps1. Run: pwsh tests/Scripts/Test-AdditiveOnlyMigrations.Tests.ps1
$ErrorActionPreference = 'Stop'
$script = Join-Path $PSScriptRoot '..\..\build\Test-AdditiveOnlyMigrations.ps1'
$dir = Join-Path ([IO.Path]::GetTempPath()) ([Guid]::NewGuid())
New-Item $dir -ItemType Directory | Out-Null

function New-Migration($id, $up, $down = 'DropColumn("dbo.T", "C");', $extra = '') {
    "$extra`npublic partial class M$id : DbMigration { public override void Up() { $up } public override void Down() { $down } }" |
        Set-Content (Join-Path $dir "${id}_M$id.cs")
}
function Invoke-Check { & pwsh -NoProfile -File $script -MigrationsPath $dir | Out-Null; $LASTEXITCODE }
function Assert($cond, $msg) { if (-not $cond) { throw "FAILED: $msg" } else { Write-Host "ok: $msg" } }

try {
    New-Migration '202701010000001' 'AddColumn("dbo.T", "C", c => c.Int());'
    Assert ((Invoke-Check) -eq 0) 'additive Up with destructive Down passes'

    New-Migration '202701010000002' 'DropColumn("dbo.T", "C");'
    Assert ((Invoke-Check) -eq 1) 'DropColumn in Up fails'
    Remove-Item "$dir\202701010000002_*"

    New-Migration '202701010000003' 'Sql("EXEC sp_rename ''a'', ''b''");'
    Assert ((Invoke-Check) -eq 1) 'sp_rename in Up fails'
    Remove-Item "$dir\202701010000003_*"

    New-Migration '202701010000004' 'AlterColumn("dbo.T", "C", c => c.Int());' 'x();' '// shared-db: allow-breaking reviewed-ticket-123'
    Assert ((Invoke-Check) -eq 0) 'allow-breaking marker passes'
    Remove-Item "$dir\202701010000004_*"

    New-Migration '202601010000005' 'DropTable("dbo.T");'
    Assert ((Invoke-Check) -eq 0) 'grandfathered migration ignored'
}
finally { Remove-Item $dir -Recurse -Force }
