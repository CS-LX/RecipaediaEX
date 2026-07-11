<#
.SYNOPSIS
    Publish a RecipaediaEX .scmod release to the SC community mod site.
.DESCRIPTION
    1. POST /upyun/credentials (typeId=5 for .scmod)
    2. Upload file to Upyun
    3. POST /upload to register the file
    4. POST /post-version to attach a new version on the resource post

    Requires MOD_SITE_TOKEN (Bearer) with permission to upload and publish on the target post.
.PARAMETER ScmodPath
    Path to RecipaediaEX-{Version}.scmod
.PARAMETER Version
    modinfo.json Version string (post-version title).
.PARAMETER ApiVersion
    modinfo.json ApiVersion string (post-version version field).
.PARAMETER ReleaseNotesPath
    Optional fallback release notes (plain text / markdown).
.PARAMETER ChangelogPath
    Optional CHANGELOG.md path; preferred over ReleaseNotesPath.
.PARAMETER ConfigPath
    mod-site.config.json path.
#>

param(
    [Parameter(Mandatory)]
    [string]$ScmodPath,

    [Parameter(Mandatory)]
    [string]$Version,

    [string]$ApiVersion = "",

    [string]$ReleaseNotesPath = "",

    [string]$ChangelogPath = "",

    [string]$ConfigPath = ""
)

$ErrorActionPreference = "Stop"

$ScriptDir = $PSScriptRoot
$RepoRoot = Split-Path $ScriptDir -Parent

if ([string]::IsNullOrWhiteSpace($ConfigPath)) {
    $ConfigPath = Join-Path $ScriptDir "mod-site.config.json"
}

if ([string]::IsNullOrWhiteSpace($ChangelogPath)) {
    $ChangelogPath = Join-Path $RepoRoot "docs/CHANGELOG.md"
}

function Get-ModSiteConfig {
    if (-not (Test-Path $ConfigPath)) {
        Write-Error "[ModSite] Config not found: $ConfigPath (copy mod-site.config.example.json)."
    }
    return Get-Content $ConfigPath -Raw -Encoding UTF8 | ConvertFrom-Json
}

function Get-ModSiteToken {
    $token = $env:MOD_SITE_TOKEN
    if ([string]::IsNullOrWhiteSpace($token)) {
        Write-Error "[ModSite] MOD_SITE_TOKEN is not set."
    }
    $token = $token.Trim()
    if ($token -match '^(?i)bearer\s+') {
        $token = ($token -replace '^(?i)bearer\s+', '').Trim()
    }
    return $token
}

function Invoke-ModSiteJson {
    param(
        [string]$Method,
        [string]$Url,
        [hashtable]$Headers,
        [object]$Body = $null
    )

    $params = @{
        Method             = $Method
        Uri                = $Url
        Headers            = $Headers
        ContentType        = "application/json; charset=utf-8"
        SkipHttpErrorCheck = $true
    }
    if ($null -ne $Body) {
        $params.Body = ($Body | ConvertTo-Json -Depth 8 -Compress)
    }
    $webResponse = Invoke-WebRequest @params
    $statusCode = [int]$webResponse.StatusCode
    $responseBody = [string]$webResponse.Content
    if ($statusCode -eq 401) {
        throw "[ModSite] HTTP 401 Unauthorized calling $Url. Check MOD_SITE_TOKEN: use the JWT only (no 'Bearer ' prefix), ensure it is not expired, and that the account can publish on post $($script:ModSitePostIdForErrors)."
    }
    if ($statusCode -lt 200 -or $statusCode -ge 300) {
        throw "[ModSite] HTTP $statusCode calling ${Url}: $responseBody"
    }
    if ([string]::IsNullOrWhiteSpace($responseBody)) {
        return $null
    }
    return $responseBody | ConvertFrom-Json
}

function New-MinuteScopedSaveKey {
    $now = Get-Date
    $year = $now.Year
    $month = "{0:D2}" -f $now.Month
    $day = "{0:D2}" -f $now.Day
    $hour = "{0:D2}" -f $now.Hour
    $minute = "{0:D2}" -f $now.Minute
    return "/$year/$month/$day/$hour/$minute/{filename}{.suffix}"
}

function Resolve-UpyunFileUrl {
    param(
        [string]$PathOrUrl,
        [string]$FileDomain
    )
    if ([string]::IsNullOrWhiteSpace($PathOrUrl)) {
        return ""
    }
    if ($PathOrUrl -match '^https?://') {
        return $PathOrUrl
    }
    $path = if ($PathOrUrl.StartsWith("/")) { $PathOrUrl } else { "/$PathOrUrl" }
    return "$FileDomain$path"
}

function Convert-MarkdownLinesToHtml {
    param([string[]]$Lines)

    $htmlParts = New-Object System.Collections.Generic.List[string]
    $inList = $false

    foreach ($line in $Lines) {
        $trimmed = $line.TrimEnd()
        if ([string]::IsNullOrWhiteSpace($trimmed)) {
            if ($inList) {
                $htmlParts.Add("</ul>")
                $inList = $false
            }
            continue
        }
        if ($trimmed -match '^###\s+(.+)$') {
            if ($inList) {
                $htmlParts.Add("</ul>")
                $inList = $false
            }
            $htmlParts.Add("<p><strong>$($Matches[1])</strong></p>")
            continue
        }
        if ($trimmed -match '^##\s+(.+)$') {
            if ($inList) {
                $htmlParts.Add("</ul>")
                $inList = $false
            }
            $htmlParts.Add("<p><strong>$($Matches[1])</strong></p>")
            continue
        }
        if ($trimmed -match '^[-*]\s+(.+)$') {
            if (-not $inList) {
                $htmlParts.Add("<ul>")
                $inList = $true
            }
            $item = $Matches[1] -replace '`([^`]+)`', '<code>$1</code>'
            $htmlParts.Add("<li><p>$item</p></li>")
            continue
        }
        if ($inList) {
            $htmlParts.Add("</ul>")
            $inList = $false
        }
        $text = $trimmed -replace '`([^`]+)`', '<code>$1</code>'
        $htmlParts.Add("<p>$text</p>")
    }
    if ($inList) {
        $htmlParts.Add("</ul>")
    }
    return ($htmlParts -join "")
}

function Get-VersionChangelogLines {
    param(
        [string]$Path,
        [string]$TargetVersion
    )

    if (-not (Test-Path $Path)) {
        return @()
    }
    $lines = Get-Content $Path -Encoding UTF8
    $header = "## [$TargetVersion]"
    $start = -1
    for ($i = 0; $i -lt $lines.Count; $i++) {
        if ($lines[$i].TrimStart().StartsWith($header)) {
            $start = $i + 1
            break
        }
    }
    if ($start -lt 0) {
        return @()
    }
    $section = New-Object System.Collections.Generic.List[string]
    for ($i = $start; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        if ($line -match '^\s*##\s+\[') {
            break
        }
        if ($line -match '^\s*---\s*$') {
            continue
        }
        $section.Add($line)
    }
    return ,$section.ToArray()
}

function Get-ReleaseContentHtml {
    param(
        [string]$TargetVersion,
        [string]$NotesPath
    )

    $changelogLines = Get-VersionChangelogLines -Path $ChangelogPath -TargetVersion $TargetVersion
    if ($changelogLines.Count -gt 0) {
        return Convert-MarkdownLinesToHtml -Lines $changelogLines
    }
    if (-not [string]::IsNullOrWhiteSpace($NotesPath) -and (Test-Path $NotesPath)) {
        $raw = Get-Content $NotesPath -Raw -Encoding UTF8
        $lines = $raw -split "`r?`n"
        return Convert-MarkdownLinesToHtml -Lines $lines
    }
    return "<p>RecipaediaEX $TargetVersion</p>"
}

function Get-UpyunCredentials {
    param(
        [string]$ApiBaseUrl,
        [hashtable]$Headers,
        [int]$TypeId,
        [string]$SaveKey
    )

    $body = @{
        saveKey = $SaveKey
        typeId  = $TypeId
    }
    $response = Invoke-ModSiteJson -Method Post -Url "$ApiBaseUrl/upyun/credentials" -Headers $Headers -Body $body
    if ($response.code -ne 200 -or -not $response.data) {
        throw "[ModSite] Failed to get Upyun credentials: $($response.msg)"
    }
    return $response.data
}

function Send-UpyunFile {
    param(
        [string]$UploadUrl,
        [string]$Policy,
        [string]$Authorization,
        [string]$FilePath
    )

    Add-Type -AssemblyName System.Net.Http
    $handler = New-Object System.Net.Http.HttpClientHandler
    $client = [System.Net.Http.HttpClient]::new($handler)
    $client.Timeout = [TimeSpan]::FromMinutes(30)
    try {
        $content = [System.Net.Http.MultipartFormDataContent]::new()
        $content.Add([System.Net.Http.StringContent]::new($Policy), "policy")
        $content.Add([System.Net.Http.StringContent]::new($Authorization), "authorization")
        $stream = [System.IO.File]::OpenRead($FilePath)
        try {
            $fileContent = [System.Net.Http.StreamContent]::new($stream)
            $fileContent.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse("application/octet-stream")
            $fileName = [IO.Path]::GetFileName($FilePath)
            $content.Add($fileContent, "file", $fileName)
            $response = $client.PostAsync($UploadUrl, $content).GetAwaiter().GetResult()
            $responseBody = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
        }
        finally {
            $stream.Dispose()
        }
        if (-not $response.IsSuccessStatusCode) {
            throw "[ModSite] Upyun upload HTTP $($response.StatusCode): $responseBody"
        }
        $parsed = $responseBody | ConvertFrom-Json
        if ($parsed.code -ne 200) {
            throw "[ModSite] Upyun upload rejected: $($parsed.message)"
        }
        return $parsed
    }
    finally {
        $client.Dispose()
        $handler.Dispose()
    }
}

function Register-ModSiteUpload {
    param(
        [string]$ApiBaseUrl,
        [hashtable]$Headers,
        [string]$FileUrl,
        [int]$TypeId
    )

    $body = @{
        path = $FileUrl
        type = $TypeId
    }
    $response = Invoke-ModSiteJson -Method Post -Url "$ApiBaseUrl/upload" -Headers $Headers -Body $body
    if ($response.code -ne 200 -or -not $response.data) {
        throw "[ModSite] Failed to register upload: $($response.msg)"
    }
    return $response.data
}

function Publish-ModSiteVersion {
    param(
        [string]$ApiBaseUrl,
        [hashtable]$Headers,
        [int]$PostId,
        [int[]]$GameVersionIds,
        [string]$Title,
        [string]$ApiVersionValue,
        [string]$ContentHtml,
        [int[]]$FileIds
    )

    $body = @{
        id              = 0
        title           = $Title
        version         = $ApiVersionValue
        content         = $ContentHtml
        files           = $FileIds
        postId          = $PostId
        gameVersionIds  = @($GameVersionIds)
    }
    $response = Invoke-ModSiteJson -Method Post -Url "$ApiBaseUrl/post-version" -Headers $Headers -Body $body
    if ($response.code -ne 200) {
        throw "[ModSite] Failed to publish post-version: $($response.msg)"
    }
    return $response.data
}

if (-not (Test-Path $ScmodPath)) {
    Write-Error "[ModSite] Scmod not found: $ScmodPath"
}

$config = Get-ModSiteConfig
$token = Get-ModSiteToken
$apiBase = [string]$config.ApiBaseUrl.TrimEnd('/')
$fileDomain = [string]$config.FileDomain.TrimEnd('/')
$postId = [int]$config.PostId
$script:ModSitePostIdForErrors = $postId
$typeId = [int]$config.ScmodTypeId
$gameVersionIds = @($config.GameVersionIds | ForEach-Object { [int]$_ })

if ([string]::IsNullOrWhiteSpace($ApiVersion)) {
    $modinfoPath = Join-Path $RepoRoot "modinfo.json"
    if (Test-Path $modinfoPath) {
        $modinfo = Get-Content $modinfoPath -Raw -Encoding UTF8 | ConvertFrom-Json
        $ApiVersion = [string]$modinfo.ApiVersion
    }
}
if ([string]::IsNullOrWhiteSpace($ApiVersion)) {
    Write-Error "[ModSite] ApiVersion is empty; pass -ApiVersion or ensure modinfo.json exists."
}

$headers = @{
    Authorization = "Bearer $token"
}

Write-Host ""
Write-Host "[ModSite] ----------------------------------------" -ForegroundColor Cyan
Write-Host "[ModSite] PostId    : $postId" -ForegroundColor Cyan
Write-Host "[ModSite] Version   : $Version" -ForegroundColor Cyan
Write-Host "[ModSite] ApiVersion: $ApiVersion" -ForegroundColor Cyan
Write-Host "[ModSite] File      : $ScmodPath" -ForegroundColor Cyan
Write-Host "[ModSite] ----------------------------------------" -ForegroundColor Cyan

$contentHtml = Get-ReleaseContentHtml -TargetVersion $Version -NotesPath $ReleaseNotesPath
$saveKey = New-MinuteScopedSaveKey

Write-Host "[ModSite] Requesting Upyun credentials (typeId=$typeId)..." -ForegroundColor Cyan
$credentials = Get-UpyunCredentials -ApiBaseUrl $apiBase -Headers $headers -TypeId $typeId -SaveKey $saveKey

Write-Host "[ModSite] Uploading to Upyun..." -ForegroundColor Cyan
$upyunResult = Send-UpyunFile `
    -UploadUrl $credentials.uploadUrl `
    -Policy $credentials.policy `
    -Authorization $credentials.authorization `
    -FilePath $ScmodPath

$fileUrl = Resolve-UpyunFileUrl -PathOrUrl ([string]$upyunResult.url) -FileDomain $fileDomain
Write-Host "[ModSite] Upyun URL : $fileUrl" -ForegroundColor Cyan

Write-Host "[ModSite] Registering upload record..." -ForegroundColor Cyan
$uploadRecord = Register-ModSiteUpload -ApiBaseUrl $apiBase -Headers $headers -FileUrl $fileUrl -TypeId $typeId
$uploadId = [int]$uploadRecord.id
Write-Host "[ModSite] Upload id : $uploadId ($($uploadRecord.filename))" -ForegroundColor Cyan

Write-Host "[ModSite] Creating post-version..." -ForegroundColor Cyan
$versionRecord = Publish-ModSiteVersion `
    -ApiBaseUrl $apiBase `
    -Headers $headers `
    -PostId $postId `
    -GameVersionIds $gameVersionIds `
    -Title $Version `
    -ApiVersionValue $ApiVersion `
    -ContentHtml $contentHtml `
    -FileIds @($uploadId)

Write-Host "[ModSite] OK - Published version id $($versionRecord.id) on post $postId" -ForegroundColor Green
Write-Host "[ModSite] URL: https://test.suancaixianyu.cn/#/postDetails/$postId" -ForegroundColor Green
Write-Host ""
