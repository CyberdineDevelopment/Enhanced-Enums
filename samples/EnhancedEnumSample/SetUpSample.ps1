#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Sets up the EnhancedEnumSample for Enhanced Enums
.DESCRIPTION
    This script builds the Enhanced Enums packages and runs the basic single-project sample
    demonstrating fundamental Enhanced Enum functionality.
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

Write-Host "=== Enhanced Enums - EnhancedEnumSample Setup ===" -ForegroundColor Cyan
Write-Host "Sample: $SampleRoot" -ForegroundColor Gray
Write-Host "Solution: $SolutionRoot" -ForegroundColor Gray
Write-Host ""

if ($Clean) {
    Write-Host "🧹 Cleaning sample..." -ForegroundColor Yellow
    Remove-Item "$SampleRoot\bin" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item "$SampleRoot\obj" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item "$SampleRoot\GeneratedFiles" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "✅ Sample cleaned" -ForegroundColor Green
}

if (-not $SkipBuild) {
    Write-Host "🔨 Building Enhanced Enums packages..." -ForegroundColor Yellow
    
    # Build the main Enhanced Enums packages
    Push-Location $SolutionRoot
    try {
        dotnet build --configuration Release --verbosity minimal
        if ($LASTEXITCODE -ne 0) { throw "Build failed" }
        
        Write-Host "✅ Enhanced Enums packages built" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
}

Write-Host "🏗️ Building sample project..." -ForegroundColor Yellow

Push-Location $SampleRoot
try {
    Write-Host "  Restoring dependencies..." -ForegroundColor Gray
    dotnet restore --verbosity minimal
    if ($LASTEXITCODE -ne 0) { throw "Restore failed" }
    
    Write-Host "  Building project..." -ForegroundColor Gray
    dotnet build --configuration Release --verbosity minimal --no-restore
    if ($LASTEXITCODE -ne 0) { throw "Build failed" }
}
finally {
    Pop-Location
}

Write-Host "✅ Sample project built" -ForegroundColor Green

Write-Host ""
Write-Host "🎉 Sample setup complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Generated files can be found in:" -ForegroundColor Yellow
Write-Host "  • $SampleRoot\GeneratedFiles\" -ForegroundColor Gray
Write-Host ""
Write-Host "To run the sample:" -ForegroundColor Yellow
Write-Host "  dotnet run --project ." -ForegroundColor Gray
Write-Host ""
Write-Host "To inspect generated code:" -ForegroundColor Yellow
Write-Host "  • GeneratedFiles\FractalDataWorks.EnhancedEnums\" -ForegroundColor Gray
Write-Host ""
Write-Host "This sample demonstrates:" -ForegroundColor Cyan
Write-Host "  ✓ Basic Enhanced Enum functionality" -ForegroundColor Gray
Write-Host "  ✓ Single-project enum generation" -ForegroundColor Gray
Write-Host "  ✓ Generated collection classes" -ForegroundColor Gray
Write-Host "  ✓ Factory methods and lookup functionality" -ForegroundColor Gray
Write-Host "  ✓ File generation for debugging" -ForegroundColor Gray