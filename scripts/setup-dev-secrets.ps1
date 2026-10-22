# Generates a random JWT signing key and stores it in .NET User Secrets (outside the Git repository).
# Usage (from the repository root):  powershell -ExecutionPolicy Bypass -File scripts/setup-dev-secrets.ps1
# The key is saved to %APPDATA%\Microsoft\UserSecrets\university-course-management-api-5f3c1a\secrets.json

$ErrorActionPreference = "Stop"
$project = Join-Path $PSScriptRoot "..\UniversityApi"

$bytes = New-Object byte[] 64
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$rng.GetBytes($bytes)
$rng.Dispose()
$key = [Convert]::ToBase64String($bytes)

dotnet user-secrets set "Jwt:Key" $key --project $project | Out-Null
if ($LASTEXITCODE -ne 0) { throw "dotnet user-secrets failed" }

Write-Host "OK: Jwt:Key (512-bit random) saved to User Secrets for UniversityApi." -ForegroundColor Green
Write-Host "Check: dotnet user-secrets list --project UniversityApi"
