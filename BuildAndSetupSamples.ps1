#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Builds Enhanced Enums packages and sets up all samples
.DESCRIPTION
    This is the main development script that builds all Enhanced Enums packages
    and sets up all sample projects for testing and development.
#>

param(
    [string]$Configuration = "Release",
    [switch]$Clean,
    [switch]$SkipBuild,
    [switch]$Verbose,
    [string[]]$Samples = @("CrossAssemblyScanner", "CrossPackageSample", "EnhancedEnumSample")
)

$ErrorActionPreference = "Stop"

Write-Host "=== Enhanced Enums - Build and Setup All Samples ===" -ForegroundColor Cyan
Write-Host ""

if ($Clean) {
    Write-Host "🧹 Cleaning entire solution..." -ForegroundColor Yellow
    dotnet clean --configuration $Configuration --verbosity minimal
    Write-Host "✅ Solution cleaned" -ForegroundColor Green
    Write-Host ""
}

if (-not $SkipBuild) {
    Write-Host "🔨 Building Enhanced Enums solution..." -ForegroundColor Yellow
    dotnet build --configuration $Configuration --verbosity minimal
    if ($LASTEXITCODE -ne 0) {
        Write-Error "❌ Solution build failed"
        exit 1
    }
    Write-Host "✅ Solution built successfully" -ForegroundColor Green
    Write-Host ""
}

Write-Host "🎯 Setting up samples..." -ForegroundColor Yellow
$successfulSamples = @()
$failedSamples = @()

foreach ($sample in $Samples) {
    $samplePath = "samples\$sample"
    $setupScript = "$samplePath\SetUpSample.ps1"
    
    if (Test-Path $setupScript) {
        Write-Host "  📋 Setting up $sample..." -ForegroundColor Cyan
        try {
            $setupArgs = @()
            if ($Clean) { $setupArgs += "-Clean" }
            if ($SkipBuild) { $setupArgs += "-SkipBuild" }
            if ($Verbose) { $setupArgs += "-Verbose" }
            
            & $setupScript @setupArgs
            $successfulSamples += $sample
            Write-Host "    ✅ $sample setup complete" -ForegroundColor Green
        }
        catch {
            Write-Host "    ❌ $sample setup failed: $($_.Exception.Message)" -ForegroundColor Red
            $failedSamples += $sample
        }
        Write-Host ""
    }
    else {
        Write-Host "    ⚠️  Setup script not found: $setupScript" -ForegroundColor Yellow
        $failedSamples += $sample
    }
}

Write-Host "📊 Sample Setup Summary:" -ForegroundColor Cyan
Write-Host "  ✅ Successful: $($successfulSamples.Count)" -ForegroundColor Green
if ($successfulSamples.Count -gt 0) {
    $successfulSamples | ForEach-Object { Write-Host "    • $_" -ForegroundColor Gray }
}

Write-Host "  ❌ Failed: $($failedSamples.Count)" -ForegroundColor Red
if ($failedSamples.Count -gt 0) {
    $failedSamples | ForEach-Object { Write-Host "    • $_" -ForegroundColor Gray }
}

Write-Host ""
if ($successfulSamples.Count -gt 0) {
    Write-Host "🎉 Sample setup complete!" -ForegroundColor Green
    Write-Host ""
    Write-Host "To run individual samples:" -ForegroundColor Yellow
    $successfulSamples | ForEach-Object { 
        Write-Host "  • $_ : .\samples\$_\SetUpSample.ps1" -ForegroundColor Gray 
    }
    Write-Host ""
    Write-Host "All samples demonstrate different aspects of Enhanced Enums:" -ForegroundColor Cyan
    Write-Host "  📦 CrossAssemblyScanner - Cross-package enum discovery" -ForegroundColor Gray
    Write-Host "  🔧 CrossPackageSample - Service-based enum patterns" -ForegroundColor Gray  
    Write-Host "  🎯 EnhancedEnumSample - Basic single-project functionality" -ForegroundColor Gray
}

if ($failedSamples.Count -gt 0) {
    Write-Host "⚠️  Some samples failed to set up. Check the logs above." -ForegroundColor Yellow
    exit 1
}

Write-Host ""
Write-Host "Happy coding! 🚀" -ForegroundColor Green