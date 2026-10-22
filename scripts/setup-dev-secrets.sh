#!/usr/bin/env bash
# Generates a random JWT signing key and stores it in .NET User Secrets (outside the Git repository).
# Usage (from the repository root):  bash scripts/setup-dev-secrets.sh
# The key is saved to ~/.microsoft/usersecrets/university-course-management-api-5f3c1a/secrets.json
set -euo pipefail
project="$(dirname "$0")/../UniversityApi"
key="$(head -c 64 /dev/urandom | base64 | tr -d '\n')"
dotnet user-secrets set "Jwt:Key" "$key" --project "$project" > /dev/null
echo "OK: Jwt:Key (512-bit random) saved to User Secrets for UniversityApi."
echo "Check: dotnet user-secrets list --project UniversityApi"
