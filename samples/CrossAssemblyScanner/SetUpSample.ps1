#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Sets up the CrossAssemblyScanner sample for Enhanced Enums
.DESCRIPTION
    This script builds the required packages, installs them locally, and runs the sample
    demonstrating cross-assembly enum discovery across multiple NuGet packages.
#>

param(
    [switch]$Clean,
    [switch]$SkipBuild,
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"

# Get script directory
$SampleRoot = $PSScriptRoot
$SolutionRoot = Split-Path (Split-Path $SampleRoot -Parent) -Parent

Write-Host "=== Enhanced Enums - CrossAssemblyScanner Sample Setup ===" -ForegroundColor Cyan
Write-Host "Sample: $SampleRoot" -ForegroundColor Gray
Write-Host "Solution: $SolutionRoot" -ForegroundColor Gray
Write-Host ""

if ($Clean) {
    Write-Host "🧹 Cleaning sample..." -ForegroundColor Yellow
    Remove-Item "$SampleRoot\*\bin" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item "$SampleRoot\*\obj" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item "$SampleRoot\LocalPackages" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item "$SampleRoot\*\GeneratedFiles" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "✅ Sample cleaned" -ForegroundColor Green
}

if (-not $SkipBuild) {
    Write-Host "🔨 Building Enhanced Enums packages..." -ForegroundColor Yellow
    
    # Build the main Enhanced Enums packages
    Push-Location $SolutionRoot
    try {
        dotnet build --configuration Release --verbosity minimal
        if ($LASTEXITCODE -ne 0) { throw "Build failed" }
        
        # Pack the packages
        dotnet pack --configuration Release --no-build --verbosity minimal --output "$SampleRoot\LocalPackages"
        if ($LASTEXITCODE -ne 0) { throw "Pack failed" }
        
        Write-Host "✅ Enhanced Enums packages built" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
}

Write-Host "📦 Building sample library packages..." -ForegroundColor Yellow

# Build and pack the sample libraries in dependency order
$Libraries = @("ColorOption.Library", "Red.Library", "Blue.Library")

foreach ($Library in $Libraries) {
    Push-Location "$SampleRoot\$Library"
    try {
        Write-Host "  Building $Library..." -ForegroundColor Gray
        
        dotnet build --configuration Release --verbosity minimal
        if ($LASTEXITCODE -ne 0) { throw "Build failed for $Library" }
        
        dotnet pack --configuration Release --no-build --verbosity minimal --output "$SampleRoot\LocalPackages"
        if ($LASTEXITCODE -ne 0) { throw "Pack failed for $Library" }
    }
    finally {
        Pop-Location
    }
}

Write-Host "✅ Sample libraries built and packed" -ForegroundColor Green

Write-Host "🔄 Restoring ConsoleApp dependencies..." -ForegroundColor Yellow
Push-Location "$SampleRoot\ConsoleApp"
try {
    dotnet restore --verbosity minimal
    if ($LASTEXITCODE -ne 0) { throw "Restore failed" }
    
    Write-Host "✅ Dependencies restored" -ForegroundColor Green
}
finally {
    Pop-Location
}

Write-Host "🏗️ Building ConsoleApp..." -ForegroundColor Yellow
Push-Location "$SampleRoot\ConsoleApp"
try {
    dotnet build --configuration Release --verbosity minimal
    if ($LASTEXITCODE -ne 0) { throw "Build failed" }
    
    Write-Host "✅ ConsoleApp built" -ForegroundColor Green
}
finally {
    Pop-Location
}

Write-Host ""
Write-Host "🎉 Sample setup complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Generated files can be found in:" -ForegroundColor Yellow
Write-Host "  • $SampleRoot\ConsoleApp\GeneratedFiles\" -ForegroundColor Gray
Write-Host ""
Write-Host "To run the sample:" -ForegroundColor Yellow
Write-Host "  dotnet run --project ConsoleApp" -ForegroundColor Gray
Write-Host ""
Write-Host "To inspect generated code:" -ForegroundColor Yellow
Write-Host "  • Standard Roslyn output: ConsoleApp\GeneratedFiles\FractalDataWorks.EnhancedEnums.CrossAssembly\" -ForegroundColor Gray
Write-Host "  • Enhanced Enums output: ConsoleApp\GeneratedFiles\EnhancedEnums\" -ForegroundColor Gray
Write-Host ""
Write-Host "This sample demonstrates:" -ForegroundColor Cyan
Write-Host "  ✓ Cross-assembly enum discovery" -ForegroundColor Gray
Write-Host "  ✓ NuGet package-based enum options" -ForegroundColor Gray
Write-Host "  ✓ Generated collection with all discovered options" -ForegroundColor Gray
Write-Host "  ✓ Custom GeneratorOutPutTo for cleaner debugging" -ForegroundColor Gray