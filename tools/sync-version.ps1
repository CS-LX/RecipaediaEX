<#
.SYNOPSIS
    Sync modinfo.json Version into RecipaediaEX.csproj.
.DESCRIPTION
    modinfo.json is the single source of truth.
    Writes <Version>, <AssemblyVersion> (numeric X.X.X.X), <InformationalVersion>.
#>

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path $PSScriptRoot -Parent
$modinfoPath = Join-Path $repoRoot "modinfo.json"
$csprojPath = Join-Path $repoRoot "RecipaediaEX.csproj"

if (-not (Test-Path $modinfoPath)) {
    Write-Error "[SyncVersion] modinfo.json not found: $modinfoPath"
    exit 1
}

if (-not (Test-Path $csprojPath)) {
    Write-Error "[SyncVersion] RecipaediaEX.csproj not found: $csprojPath"
    exit 1
}

$modinfo = Get-Content $modinfoPath -Raw -Encoding UTF8 | ConvertFrom-Json
$version = [string]$modinfo.Version
$version = $version.Trim()

if ([string]::IsNullOrWhiteSpace($version)) {
    Write-Error "[SyncVersion] modinfo.json Version is empty."
    exit 1
}

$numericPart = ($version -split '-', 2)[0]
$parts = @($numericPart -split '\.')
while ($parts.Count -lt 4) {
    $parts += '0'
}
$assemblyVersion = ($parts[0..3] -join '.')

$csproj = Get-Content $csprojPath -Raw -Encoding UTF8

function Set-CsprojProperty {
    param(
        [string]$Content,
        [string]$Name,
        [string]$Value
    )

    $pattern = "<${Name}>[^<]*</${Name}>"
    $replacement = "<${Name}>${Value}</${Name}>"

    if ($Content -match $pattern) {
        return [regex]::Replace($Content, $pattern, $replacement, 1)
    }

    return $Content -replace '(<Version>[^<]*</Version>)', "`$1`r`n	  <$Name>$Value</$Name>"
}

$csproj = Set-CsprojProperty -Content $csproj -Name "Version" -Value $version
$csproj = Set-CsprojProperty -Content $csproj -Name "AssemblyVersion" -Value $assemblyVersion
$csproj = Set-CsprojProperty -Content $csproj -Name "InformationalVersion" -Value $version

Set-Content -Path $csprojPath -Value $csproj -Encoding UTF8 -NoNewline

Write-Host "[SyncVersion] modinfo Version     : $version" -ForegroundColor Cyan
Write-Host "[SyncVersion] AssemblyVersion     : $assemblyVersion" -ForegroundColor Cyan
Write-Host "[SyncVersion] Updated             : $csprojPath" -ForegroundColor Green
