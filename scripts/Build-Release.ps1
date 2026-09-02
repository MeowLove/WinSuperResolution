[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$msbuildPath = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe'
$solutionPath = Join-Path $repositoryRoot 'WinSuperResolution.sln'
$smokeTestPath = Join-Path $repositoryRoot 'tests\WinSuperResolution.SmokeTests\bin\Release\WinSuperResolution.SmokeTests.exe'
$applicationPath = Join-Path $repositoryRoot 'src\WinSuperResolution\bin\Release\WinSuperResolution.exe'

if (-not (Test-Path -LiteralPath $msbuildPath)) {
    throw "Required .NET Framework MSBuild was not found: $msbuildPath"
}

& $msbuildPath $solutionPath /t:Rebuild /p:Configuration=Release /p:Platform=x64 /nologo /v:minimal
if ($LASTEXITCODE -ne 0) {
    throw "Release build failed with exit code $LASTEXITCODE."
}

if (-not (Test-Path -LiteralPath $smokeTestPath)) {
    throw "Smoke test executable was not produced: $smokeTestPath"
}

& $smokeTestPath
if ($LASTEXITCODE -ne 0) {
    throw "Smoke tests failed with exit code $LASTEXITCODE."
}

if (-not (Test-Path -LiteralPath $applicationPath)) {
    throw "Release executable was not produced: $applicationPath"
}

$hash = (Get-FileHash -LiteralPath $applicationPath -Algorithm SHA256).Hash
Write-Host "Release build and smoke tests succeeded."
Write-Host "Executable: $applicationPath"
Write-Host "SHA-256: $hash"
