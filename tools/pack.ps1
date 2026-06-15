<#
.SYNOPSIS
    RecipaediaEX mod packaging script.
.DESCRIPTION
    Packs build output into a .scmod archive (plain ZIP, no asset encryption).

    Naming (see docs/打包发布CI策划.md):
      Local     : RecipaediaEX.scmod
      CI        : RecipaediaEX-ci.{sha7}.scmod  (-PackageLabel ci -GitSha)
      Release   : RecipaediaEX-{Version}.scmod  (-ArtifactDir, no ci label)
.PARAMETER BuildOutputDir
    MSBuild output directory ($(TargetDir)).
.PARAMETER Configuration
    Debug / Release.
.PARAMETER ArtifactDir
    When set, writes the .scmod here (CI / Release). Skips pack.config.json.
.PARAMETER PackageLabel
    Channel label, e.g. "ci".
.PARAMETER GitSha
    Full or short git commit SHA (CI uses first 7 chars).
.PARAMETER Version
    Release version string; defaults to modinfo.json Version when packing for Release.
#>

param(
    [Parameter(Mandatory)]
    [string]$BuildOutputDir,

    [string]$Configuration = "Release",

    [string]$ModFileName = "RecipaediaEX",

    [string]$ArtifactDir = "",

    [string]$PackageLabel = "",

    [string]$GitSha = "",

    [string]$Version = ""
)

$ErrorActionPreference = "Stop"

function Get-ModinfoVersion {
    $modinfoPath = Join-Path (Split-Path $PSScriptRoot -Parent) "modinfo.json"
    if (-not (Test-Path $modinfoPath)) {
        return $null
    }
    $modinfo = Get-Content $modinfoPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $v = [string]$modinfo.Version
    if ([string]::IsNullOrWhiteSpace($v)) { return $null }
    return $v.Trim()
}

function Resolve-PackageBaseName {
    param(
        [string]$DefaultName,
        [string]$Label,
        [string]$Sha,
        [string]$ExplicitVersion,
        [bool]$IsArtifactOutput
    )

    if ($Label -eq "ci" -and -not [string]::IsNullOrWhiteSpace($Sha)) {
        $shortSha = $Sha.Trim()
        if ($shortSha.Length -gt 7) {
            $shortSha = $shortSha.Substring(0, 7)
        }
        return "RecipaediaEX-ci.$shortSha"
    }

    $releaseVersion = $ExplicitVersion
    if ([string]::IsNullOrWhiteSpace($releaseVersion) -and $IsArtifactOutput -and [string]::IsNullOrWhiteSpace($Label)) {
        $releaseVersion = Get-ModinfoVersion
    }

    if (-not [string]::IsNullOrWhiteSpace($releaseVersion)) {
        return "RecipaediaEX-$releaseVersion"
    }

    return $DefaultName
}

$BuildOutputDir = $BuildOutputDir.TrimEnd('\', '/') + '\'
$ScriptDir = $PSScriptRoot
$ConfigPath = Join-Path $ScriptDir "pack.config.json"
$sevenZipCandidates = @(
    (Join-Path $ScriptDir "7z\7z.exe"),
    (Join-Path (Join-Path $ScriptDir "..\..\SCIENEW\tools\7z") "7z.exe"),
    (Join-Path (Join-Path $ScriptDir "..\..\..\SCIENEW\tools\7z") "7z.exe")
)
$sevenZipExe = $sevenZipCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1

if (-not (Test-Path $BuildOutputDir)) {
    Write-Error "[PackMod] ERROR: Build output directory does not exist: $BuildOutputDir"
    exit 1
}

$DestDir = $null
$useArtifactNaming = $false

if (-not [string]::IsNullOrWhiteSpace($ArtifactDir)) {
    $DestDir = $ArtifactDir
    $useArtifactNaming = $true
}
elseif (Test-Path $ConfigPath) {
    $Config = Get-Content $ConfigPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $DestDir = $Config.ModsFolder
    if ($Config.ModFileName) {
        $ModFileName = $Config.ModFileName
    }
}
else {
    Write-Host ""
    Write-Host "[PackMod] INFO: pack.config.json not found and -ArtifactDir not set; skipping deployment." -ForegroundColor Yellow
    Write-Host "[PackMod] Copy tools\pack.config.example.json to tools\pack.config.json for local deploy." -ForegroundColor Yellow
    Write-Host ""
    exit 0
}

if ([string]::IsNullOrWhiteSpace($DestDir)) {
    Write-Error "[PackMod] ERROR: destination directory is empty."
    exit 1
}

if (-not (Test-Path $DestDir)) {
    New-Item -ItemType Directory -Path $DestDir -Force | Out-Null
}

$packageBaseName = Resolve-PackageBaseName `
    -DefaultName $ModFileName `
    -Label $PackageLabel `
    -Sha $GitSha `
    -ExplicitVersion $Version `
    -IsArtifactOutput $useArtifactNaming

$TempZip = Join-Path $env:TEMP "$packageBaseName.zip"
$ScmodFile = Join-Path $env:TEMP "$packageBaseName.scmod"
$DestFile = Join-Path $DestDir "$packageBaseName.scmod"

Write-Host ""
Write-Host "[PackMod] ----------------------------------------" -ForegroundColor Cyan
Write-Host "[PackMod] Mod     : RecipaediaEX" -ForegroundColor Cyan
Write-Host "[PackMod] Config  : $Configuration" -ForegroundColor Cyan
Write-Host "[PackMod] Source  : $BuildOutputDir" -ForegroundColor Cyan
Write-Host "[PackMod] Target  : $DestFile" -ForegroundColor Cyan
Write-Host "[PackMod] ----------------------------------------" -ForegroundColor Cyan

if (Test-Path $TempZip) { Remove-Item $TempZip -Force }
if (Test-Path $ScmodFile) { Remove-Item $ScmodFile -Force }

Write-Host "[PackMod] Compressing (plaintext, no AMPK)..." -ForegroundColor Cyan
Push-Location $BuildOutputDir
try {
    if ($sevenZipExe) {
        & $sevenZipExe a -tzip -mx=1 -r "$TempZip" "*" | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Error "[PackMod] ERROR: 7z compression failed (exit code: $LASTEXITCODE)."
            exit 1
        }
    }
    else {
        Compress-Archive -Path * -DestinationPath $TempZip -Force
    }
}
finally {
    Pop-Location
}

Move-Item $TempZip $ScmodFile -Force
Move-Item $ScmodFile $DestFile -Force

Write-Host "[PackMod] OK - Packaged: $DestFile" -ForegroundColor Green
Write-Host ""
