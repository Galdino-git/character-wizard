<#
.SYNOPSIS
    Builds a self-contained Windows release of CharacterWizard and zips it.

.DESCRIPTION
    Pipeline:
        1. (Optional) Re-import 5etools data into ./data via the importer tool.
        2. Publish src/CharacterWizard.App as self-contained win-x64 to artifacts/publish.
        3. Copy ./data alongside the published exe.
        4. Compress the publish folder into artifacts/CharacterWizard-v{Version}-win-x64.zip.

    Idempotent — wipes artifacts/ at start. Run from anywhere.

.PARAMETER Version
    Version string used in the zip filename and as -p:Version. Default: today's date.

.PARAMETER DataSource
    Path to the cloned 5etools repo (https://github.com/TheGiddyLimit/TheGiddyLimit.github.io).
    Default: ..\..\5etools relative to this script.

.PARAMETER SkipImport
    Skip re-importing data — reuse whatever is already in ./data. Faster for testing the script itself.

.EXAMPLE
    .\tools\Build-Release.ps1 -Version 0.1.0

.EXAMPLE
    .\tools\Build-Release.ps1 -Version 0.1.0-test -SkipImport
#>
param(
    [string]$Version = (Get-Date -Format 'yyyy.MM.dd'),
    [string]$DataSource = "$PSScriptRoot\..\..\5etools",
    [switch]$SkipImport
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path "$PSScriptRoot\..").Path
$artifacts = Join-Path $repo "artifacts"
$publishDir = Join-Path $artifacts "publish"

Write-Host "==> Repo:       $repo"
Write-Host "==> Version:    $Version"
Write-Host "==> Artifacts:  $artifacts"

if (Test-Path $artifacts) {
    Write-Host "==> Cleaning artifacts/"
    Remove-Item $artifacts -Recurse -Force
}
New-Item -ItemType Directory -Path $publishDir | Out-Null

Push-Location $repo
try {
    # 1. Refresh data from upstream 5etools
    if (-not $SkipImport) {
        if (-not (Test-Path $DataSource)) {
            throw "5etools source not found at: $DataSource. Use -DataSource <path> or -SkipImport."
        }
        Write-Host "==> Importing data from $DataSource"
        dotnet run --project tools\Import5eToolsData -- --source $DataSource --target .\data
        if ($LASTEXITCODE -ne 0) { throw "Importer failed" }
    } else {
        Write-Host "==> Skipping data import (reusing ./data)"
        if (-not (Test-Path ".\data\5etools-json")) {
            throw "No imported data found. Remove -SkipImport or run the importer first."
        }
    }

    # 2. Publish self-contained
    Write-Host "==> Publishing self-contained win-x64..."
    dotnet publish src\CharacterWizard.App `
        -c Release `
        -f net10.0-windows10.0.19041.0 `
        -r win-x64 `
        --self-contained true `
        -p:Version=$Version `
        -p:WindowsPackageType=None `
        -o $publishDir
    if ($LASTEXITCODE -ne 0) { throw "Publish failed" }

    # 3. Copy data alongside the exe
    Write-Host "==> Copying data alongside exe..."
    Copy-Item -Recurse -Path ".\data" -Destination (Join-Path $publishDir "data")

    # 4. Zip the publish folder
    $zip = Join-Path $artifacts "CharacterWizard-v$Version-win-x64.zip"
    Write-Host "==> Creating zip $zip"
    Compress-Archive -Path "$publishDir\*" -DestinationPath $zip -Force

    $size = (Get-Item $zip).Length / 1MB
    Write-Host ""
    Write-Host "==> DONE"
    Write-Host ("    {0}  ({1:N1} MB)" -f $zip, $size)
}
finally {
    Pop-Location
}
