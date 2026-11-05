# book/samples/Chapter02_UnderstandingLLMs/Create-Solution.ps1
#
# Scaffolds the AiAwareEngineeringToolkit solution from scratch using the
# .NET CLI. Run this first, then copy the .cs source files from the book
# into the folders it creates.
#
# Prerequisites: .NET 8 SDK, PowerShell 7+
#
# Usage:
#   .\Create-Solution.ps1
#   .\Create-Solution.ps1 -RootPath "C:\MyProjects\Chapter02"

param(
    [string]$RootPath = $PSScriptRoot
)

$ErrorActionPreference = "Stop"
Set-Location $RootPath

Write-Host "`n=== AiAwareEngineeringToolkit — Solution Scaffold ===" -ForegroundColor Cyan
Write-Host "Root: $RootPath`n"

# ── 1. Solution ────────────────────────────────────────────────────────────

Write-Host "Creating solution..." -ForegroundColor Yellow
dotnet new sln --name AiAwareEngineeringToolkit --force

# ── 2. Projects ─────────────────────────────────────────────────────────────

Write-Host "Creating projects..." -ForegroundColor Yellow

dotnet new classlib --name AiToolkit.Core --output src/AiToolkit.Core --framework net8.0 --force
Remove-Item "src/AiToolkit.Core/Class1.cs" -ErrorAction SilentlyContinue

dotnet new classlib --name AiToolkit.Application --output src/AiToolkit.Application --framework net8.0 --force
Remove-Item "src/AiToolkit.Application/Class1.cs" -ErrorAction SilentlyContinue

dotnet new classlib --name AiToolkit.Infrastructure --output src/AiToolkit.Infrastructure --framework net8.0 --force
Remove-Item "src/AiToolkit.Infrastructure/Class1.cs" -ErrorAction SilentlyContinue

dotnet new web --name AiToolkit.Api --output src/AiToolkit.Api --framework net8.0 --force

dotnet new xunit --name AiToolkit.Tests.Unit --output tests/Unit --framework net8.0 --force
Remove-Item "tests/Unit/UnitTest1.cs" -ErrorAction SilentlyContinue

# ── 3. Add projects to solution ─────────────────────────────────────────────

Write-Host "Wiring solution..." -ForegroundColor Yellow

dotnet sln add src/AiToolkit.Core/AiToolkit.Core.csproj
dotnet sln add src/AiToolkit.Application/AiToolkit.Application.csproj
dotnet sln add src/AiToolkit.Infrastructure/AiToolkit.Infrastructure.csproj
dotnet sln add src/AiToolkit.Api/AiToolkit.Api.csproj
dotnet sln add tests/Unit/AiToolkit.Tests.Unit.csproj

# ── 4. Project references ───────────────────────────────────────────────────

Write-Host "Adding project references..." -ForegroundColor Yellow

dotnet add src/AiToolkit.Application/AiToolkit.Application.csproj reference src/AiToolkit.Core/AiToolkit.Core.csproj

dotnet add src/AiToolkit.Infrastructure/AiToolkit.Infrastructure.csproj reference src/AiToolkit.Core/AiToolkit.Core.csproj
dotnet add src/AiToolkit.Infrastructure/AiToolkit.Infrastructure.csproj reference src/AiToolkit.Application/AiToolkit.Application.csproj

dotnet add src/AiToolkit.Api/AiToolkit.Api.csproj reference src/AiToolkit.Core/AiToolkit.Core.csproj
dotnet add src/AiToolkit.Api/AiToolkit.Api.csproj reference src/AiToolkit.Application/AiToolkit.Application.csproj
dotnet add src/AiToolkit.Api/AiToolkit.Api.csproj reference src/AiToolkit.Infrastructure/AiToolkit.Infrastructure.csproj

dotnet add tests/Unit/AiToolkit.Tests.Unit.csproj reference src/AiToolkit.Core/AiToolkit.Core.csproj
dotnet add tests/Unit/AiToolkit.Tests.Unit.csproj reference src/AiToolkit.Application/AiToolkit.Application.csproj
dotnet add tests/Unit/AiToolkit.Tests.Unit.csproj reference src/AiToolkit.Infrastructure/AiToolkit.Infrastructure.csproj

# ── 5. NuGet packages ────────────────────────────────────────────────────────

Write-Host "Adding NuGet packages..." -ForegroundColor Yellow

dotnet add src/AiToolkit.Application/AiToolkit.Application.csproj package Microsoft.Extensions.DependencyInjection.Abstractions --version 8.0.2

dotnet add src/AiToolkit.Infrastructure/AiToolkit.Infrastructure.csproj package OpenAI --version 2.1.0
dotnet add src/AiToolkit.Infrastructure/AiToolkit.Infrastructure.csproj package Microsoft.Extensions.Logging.Abstractions --version 8.0.2
dotnet add src/AiToolkit.Infrastructure/AiToolkit.Infrastructure.csproj package Microsoft.Extensions.Configuration.Abstractions --version 8.0.0
dotnet add src/AiToolkit.Infrastructure/AiToolkit.Infrastructure.csproj package Microsoft.Extensions.DependencyInjection.Abstractions --version 8.0.2

dotnet add tests/Unit/AiToolkit.Tests.Unit.csproj package Microsoft.Extensions.Logging.Abstractions --version 8.0.2

# ── 6. Source folders ────────────────────────────────────────────────────────

Write-Host "Creating source folders..." -ForegroundColor Yellow

New-Item -ItemType Directory -Force -Path "src/AiToolkit.Core/Tokens"             | Out-Null
New-Item -ItemType Directory -Force -Path "src/AiToolkit.Application/Ports"       | Out-Null
New-Item -ItemType Directory -Force -Path "src/AiToolkit.Infrastructure/AI"       | Out-Null
New-Item -ItemType Directory -Force -Path "src/AiToolkit.Api/Endpoints"           | Out-Null
New-Item -ItemType Directory -Force -Path "tests/Unit/AI"                        | Out-Null

# ── 7. Verify build ──────────────────────────────────────────────────────────

Write-Host "`nVerifying build (empty projects)..." -ForegroundColor Yellow
dotnet build --verbosity minimal

Write-Host "`n=== Scaffold complete! ===" -ForegroundColor Green
Write-Host "Copy the .cs source files from the book into the folders above."
Write-Host "Then run: dotnet build && dotnet test"
