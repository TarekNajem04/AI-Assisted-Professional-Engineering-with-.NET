# book/samples/Chapter-03/EngineeringCommunicationToolkit/Create-Solution.ps1
#
# Scaffolds the EngineeringCommunicationToolkit solution using the .NET CLI.
# Run this FIRST in an empty directory, then copy the source files from the book.
#
# Usage:
#   .\Create-Solution.ps1
#   .\Create-Solution.ps1 -RootPath ".\Chapter03Sample"

param([string]$RootPath = $PSScriptRoot)

$ErrorActionPreference = "Stop"
Set-Location $RootPath

Write-Host "`n=== EngineeringCommunicationToolkit — Solution Scaffold ===" -ForegroundColor Cyan
Write-Host "Root: $RootPath`n"

# ── 1. Solution ─────────────────────────────────────────────────────────────
Write-Host "Creating solution..." -ForegroundColor Yellow
dotnet new sln --name EngineeringCommunicationToolkit --format slnx --force

# ── 2. Projects ──────────────────────────────────────────────────────────────
Write-Host "Creating projects..." -ForegroundColor Yellow

dotnet new classlib --name AiToolkit.Core          --output src/AiToolkit.Core          --framework net8.0 --force
dotnet new classlib --name AiToolkit.Application   --output src/AiToolkit.Application   --framework net8.0 --force
dotnet new classlib --name AiToolkit.Infrastructure --output src/AiToolkit.Infrastructure --framework net8.0 --force
dotnet new web      --name AiToolkit.Api           --output src/AiToolkit.Api           --framework net8.0 --force
dotnet new mstest   --name AiToolkit.Tests.Unit    --output tests/Unit                  --framework net8.0 --force

Remove-Item "src/AiToolkit.Core/Class1.cs"            -ErrorAction SilentlyContinue
Remove-Item "src/AiToolkit.Application/Class1.cs"     -ErrorAction SilentlyContinue
Remove-Item "src/AiToolkit.Infrastructure/Class1.cs"  -ErrorAction SilentlyContinue
Remove-Item "tests/Unit/Test1.cs"                     -ErrorAction SilentlyContinue
Remove-Item "tests/Unit/UnitTest1.cs"                 -ErrorAction SilentlyContinue

# ── 3. Add to solution ────────────────────────────────────────────────────────
Write-Host "Wiring solution..." -ForegroundColor Yellow

dotnet sln EngineeringCommunicationToolkit.slnx add src/AiToolkit.Core/AiToolkit.Core.csproj
dotnet sln EngineeringCommunicationToolkit.slnx add src/AiToolkit.Application/AiToolkit.Application.csproj
dotnet sln EngineeringCommunicationToolkit.slnx add src/AiToolkit.Infrastructure/AiToolkit.Infrastructure.csproj
dotnet sln EngineeringCommunicationToolkit.slnx add src/AiToolkit.Api/AiToolkit.Api.csproj
dotnet sln EngineeringCommunicationToolkit.slnx add tests/Unit/AiToolkit.Tests.Unit.csproj

# ── 4. Project references ─────────────────────────────────────────────────────
Write-Host "Adding project references..." -ForegroundColor Yellow

dotnet add src/AiToolkit.Application/AiToolkit.Application.csproj     reference src/AiToolkit.Core/AiToolkit.Core.csproj
dotnet add src/AiToolkit.Infrastructure/AiToolkit.Infrastructure.csproj reference src/AiToolkit.Core/AiToolkit.Core.csproj
dotnet add src/AiToolkit.Infrastructure/AiToolkit.Infrastructure.csproj reference src/AiToolkit.Application/AiToolkit.Application.csproj
dotnet add src/AiToolkit.Api/AiToolkit.Api.csproj reference src/AiToolkit.Core/AiToolkit.Core.csproj
dotnet add src/AiToolkit.Api/AiToolkit.Api.csproj reference src/AiToolkit.Application/AiToolkit.Application.csproj
dotnet add src/AiToolkit.Api/AiToolkit.Api.csproj reference src/AiToolkit.Infrastructure/AiToolkit.Infrastructure.csproj
dotnet add tests/Unit/AiToolkit.Tests.Unit.csproj reference src/AiToolkit.Core/AiToolkit.Core.csproj
dotnet add tests/Unit/AiToolkit.Tests.Unit.csproj reference src/AiToolkit.Application/AiToolkit.Application.csproj
dotnet add tests/Unit/AiToolkit.Tests.Unit.csproj reference src/AiToolkit.Infrastructure/AiToolkit.Infrastructure.csproj

# ── 5. NuGet packages ─────────────────────────────────────────────────────────
Write-Host "Adding NuGet packages..." -ForegroundColor Yellow

dotnet add src/AiToolkit.Application/AiToolkit.Application.csproj package Microsoft.Extensions.DependencyInjection.Abstractions --version 8.0.2
dotnet add src/AiToolkit.Infrastructure/AiToolkit.Infrastructure.csproj package OpenAI --version '[2.0.0,3.0.0)'
dotnet add src/AiToolkit.Infrastructure/AiToolkit.Infrastructure.csproj package Microsoft.Extensions.Logging.Abstractions --version 8.0.2
dotnet add src/AiToolkit.Infrastructure/AiToolkit.Infrastructure.csproj package Microsoft.Extensions.Configuration.Abstractions --version 8.0.0
dotnet add src/AiToolkit.Infrastructure/AiToolkit.Infrastructure.csproj package Microsoft.Extensions.DependencyInjection.Abstractions --version 8.0.2
dotnet add tests/Unit/AiToolkit.Tests.Unit.csproj package Microsoft.NET.Test.Sdk --version 17.12.0
dotnet add tests/Unit/AiToolkit.Tests.Unit.csproj package MSTest.TestAdapter --version 3.7.3
dotnet add tests/Unit/AiToolkit.Tests.Unit.csproj package MSTest.TestFramework --version 3.7.3
dotnet add tests/Unit/AiToolkit.Tests.Unit.csproj package Jeninnet.Assertions --version 1.0.0
dotnet add tests/Unit/AiToolkit.Tests.Unit.csproj package Microsoft.Extensions.Logging.Abstractions --version 8.0.2

# ── 6. Source folders and configuration ──────────────────────────────────────
Write-Host "Creating folders and configuration..." -ForegroundColor Yellow

New-Item -ItemType Directory -Force -Path "src/AiToolkit.Core/Prompts"         | Out-Null
New-Item -ItemType Directory -Force -Path "src/AiToolkit.Core/Tokens"          | Out-Null
New-Item -ItemType Directory -Force -Path "src/AiToolkit.Application/Ports"    | Out-Null
New-Item -ItemType Directory -Force -Path "src/AiToolkit.Infrastructure/AI"    | Out-Null
New-Item -ItemType Directory -Force -Path "src/AiToolkit.Api/Endpoints"        | Out-Null
New-Item -ItemType Directory -Force -Path "tests/Unit"                         | Out-Null

@'
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "OpenAI": {
    "ApiKey": ""
  },
  "AiToolkit": {
    "ClientMode": "",
    "FakeFailure": false
  }
}
'@ | Set-Content -Path "src/AiToolkit.Api/appsettings.json" -Encoding UTF8

@'
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  }
}
'@ | Set-Content -Path "src/AiToolkit.Api/appsettings.Development.json" -Encoding UTF8

@'
root = true

[*]
charset = utf-8
indent_style = space
indent_size = 4
trim_trailing_whitespace = true
insert_final_newline = true
'@ | Set-Content -Path ".editorconfig" -Encoding UTF8

# ── 7. Verify ─────────────────────────────────────────────────────────────────
Write-Host "`nVerifying build (empty projects)..." -ForegroundColor Yellow
dotnet build EngineeringCommunicationToolkit.slnx --verbosity minimal

Write-Host "`n=== Scaffold complete! ===" -ForegroundColor Green
Write-Host "Copy the .cs source files from the book into the folders above."
Write-Host "Then run: dotnet build && dotnet test"
