[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidatePattern('^\d+\.\d+\.\d+$')]
    [string]$Version
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$applicationPath = Join-Path $repositoryRoot 'src\WinSuperResolution\bin\Release\WinSuperResolution.exe'
$releaseNotesPath = Join-Path $repositoryRoot "docs\releases\RELEASE_NOTES_v$Version.md"
$deliverablesPath = Join-Path $repositoryRoot 'deliverables'
$workPath = Join-Path $repositoryRoot 'work'
$packageName = "WinSuperResolution-v$Version-win-x64"
$archivePath = Join-Path $deliverablesPath "$packageName.zip"
$checksumPath = "$archivePath.sha256"
$stagingPath = Join-Path $workPath "package-$packageName"
$verificationPath = Join-Path $workPath "verify-$packageName"

if (-not (Test-Path -LiteralPath $applicationPath)) {
    throw "Release executable was not found. Run scripts\\Build-Release.ps1 first: $applicationPath"
}

if (-not (Test-Path -LiteralPath $releaseNotesPath)) {
    throw "Release notes for v$Version were not found: $releaseNotesPath"
}

$files = @(
    @{ Source = (Join-Path $repositoryRoot 'LICENSE'); Destination = 'LICENSE' },
    @{ Source = (Join-Path $repositoryRoot 'README.md'); Destination = 'README.md' },
    @{ Source = $releaseNotesPath; Destination = 'RELEASE_NOTES.md' },
    @{ Source = $applicationPath; Destination = 'WinSuperResolution.exe' },
    @{ Source = (Join-Path $repositoryRoot 'docs\README.ru-RU.md'); Destination = 'docs\README.ru-RU.md' },
    @{ Source = (Join-Path $repositoryRoot 'docs\README.zh-CN.md'); Destination = 'docs\README.zh-CN.md' }
)

foreach ($file in $files) {
    if (-not (Test-Path -LiteralPath $file.Source)) {
        throw "Required package input was not found: $($file.Source)"
    }
}

try {
    Remove-Item -LiteralPath $stagingPath, $verificationPath -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $stagingPath, $verificationPath -Force | Out-Null

    foreach ($file in $files) {
        $destinationPath = Join-Path $stagingPath $file.Destination
        $destinationDirectory = Split-Path -Parent $destinationPath
        New-Item -ItemType Directory -Path $destinationDirectory -Force | Out-Null
        Copy-Item -LiteralPath $file.Source -Destination $destinationPath -Force
    }

    New-Item -ItemType Directory -Path $deliverablesPath -Force | Out-Null
    Compress-Archive -Path (Join-Path $stagingPath '*') -DestinationPath $archivePath -Force
    Expand-Archive -LiteralPath $archivePath -DestinationPath $verificationPath -Force

    $expectedFiles = @(
        'LICENSE',
        'README.md',
        'RELEASE_NOTES.md',
        'WinSuperResolution.exe',
        'docs\README.ru-RU.md',
        'docs\README.zh-CN.md'
    )
    $actualFiles = Get-ChildItem -LiteralPath $verificationPath -Recurse -File |
        ForEach-Object { $_.FullName.Substring($verificationPath.Length + 1) } |
        Sort-Object

    $difference = Compare-Object -ReferenceObject ($expectedFiles | Sort-Object) -DifferenceObject $actualFiles
    if ($difference) {
        throw "Package file list verification failed: $($difference | Out-String)"
    }

    $sourceHash = (Get-FileHash -LiteralPath $applicationPath -Algorithm SHA256).Hash
    $packageHash = (Get-FileHash -LiteralPath (Join-Path $verificationPath 'WinSuperResolution.exe') -Algorithm SHA256).Hash
    if ($sourceHash -ne $packageHash) {
        throw 'Packaged executable hash does not match the verified Release executable.'
    }

    $archiveHash = (Get-FileHash -LiteralPath $archivePath -Algorithm SHA256).Hash
    Set-Content -LiteralPath $checksumPath -Value $archiveHash -Encoding ascii

    Write-Host "Release package succeeded."
    Write-Host "Archive: $archivePath"
    Write-Host "SHA-256: $archiveHash"
}
finally {
    Remove-Item -LiteralPath $stagingPath, $verificationPath -Recurse -Force -ErrorAction SilentlyContinue
}
