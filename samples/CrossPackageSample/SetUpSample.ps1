#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Sets up the CrossPackageSample for Enhanced Enums
.DESCRIPTION
    This script builds the Enhanced Enums packages and runs the cross-package sample
    demonstrating service-based enum patterns across multiple projects.
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

Write-Host "=== Enhanced Enums - CrossPackageSample Setup ===" -ForegroundColor Cyan
Write-Host "Sample: $SampleRoot" -ForegroundColor Gray
Write-Host "Solution: $SolutionRoot" -ForegroundColor Gray
Write-Host ""

if ($Clean) {
    Write-Host "🧹 Cleaning sample..." -ForegroundColor Yellow
    Remove-Item "$SampleRoot\*\bin" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item "$SampleRoot\*\obj" -Recurse -Force -ErrorAction SilentlyContinue
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
        
        Write-Host "✅ Enhanced Enums packages built" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }
}

Write-Host "🏗️ Building sample projects..." -ForegroundColor Yellow

# Build projects in dependency order
$Projects = @(
    "Services.Notification",
    "Services.Notification.Email", 
    "Services.Notification.Sms",
    "NotificationConsole"
)

foreach ($Project in $Projects) {
    if (Test-Path "$SampleRoot\$Project") {
        Push-Location "$SampleRoot\$Project"
        try {
            Write-Host "  Building $Project..." -ForegroundColor Gray
            
            dotnet restore --verbosity minimal
            if ($LASTEXITCODE -ne 0) { throw "Restore failed for $Project" }
            
            dotnet build --configuration Release --verbosity minimal --no-restore
            if ($LASTEXITCODE -ne 0) { throw "Build failed for $Project" }
        }
        finally {
            Pop-Location
        }
    }
}

Write-Host "✅ Sample projects built" -ForegroundColor Green

Write-Host ""
Write-Host "🎉 Sample setup complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Generated files can be found in:" -ForegroundColor Yellow
Write-Host "  • $SampleRoot\NotificationConsole\GeneratedFiles\" -ForegroundColor Gray
Write-Host ""
Write-Host "To run the sample:" -ForegroundColor Yellow
Write-Host "  dotnet run --project NotificationConsole" -ForegroundColor Gray
Write-Host ""
Write-Host "To inspect generated code:" -ForegroundColor Yellow
Write-Host "  • NotificationConsole\GeneratedFiles\FractalDataWorks.EnhancedEnums\" -ForegroundColor Gray
Write-Host ""
Write-Host "This sample demonstrates:" -ForegroundColor Cyan
Write-Host "  ✓ Single-assembly enum generation" -ForegroundColor Gray
Write-Host "  ✓ Service-based enum patterns" -ForegroundColor Gray
Write-Host "  ✓ Project reference-based discovery" -ForegroundColor Gray
Write-Host "  ✓ Generated collections within the same project" -ForegroundColor Gray