<#
.SYNOPSIS
    Gets the actual version of the built EnhancedEnums assembly.
.DESCRIPTION
    This script checks the actual version information from the built DLL and any packed NuGet packages.
.PARAMETER Configuration
    The build configuration to check (Debug or Release). Default is Debug.
.EXAMPLE
    .\Get-BuiltVersion.ps1
.EXAMPLE
    .\Get-BuiltVersion.ps1 -Configuration Release
#>
Param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug'
)

# Repository root (two levels up from samples/AssemblyScanner)
$repoRoot = Join-Path $PSScriptRoot '..\..'
$enhancedEnumsDll = Join-Path $repoRoot "src\DomesticatedData.EnhancedEnums\bin\$Configuration\netstandard2.0\DomesticatedData.EnhancedEnums.dll"
$localNugetDir = Join-Path $PSScriptRoot 'nuget'

Write-Host "Checking built version information:" -ForegroundColor Cyan
Write-Host ""

# Check if the DLL exists
if (Test-Path $enhancedEnumsDll) {
    Write-Host "Found built DLL at: $enhancedEnumsDll" -ForegroundColor Green
    
    # Get assembly version info
    $assembly = [System.Reflection.Assembly]::LoadFrom($enhancedEnumsDll)
    $assemblyName = $assembly.GetName()
    
    Write-Host ""
    Write-Host "Assembly Information:" -ForegroundColor Yellow
    Write-Host "- Assembly Version: $($assemblyName.Version)"
    Write-Host "- Full Name: $($assemblyName.FullName)"
    
    # Get file version info
    $fileVersion = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($enhancedEnumsDll)
    Write-Host ""
    Write-Host "File Version Information:" -ForegroundColor Yellow
    Write-Host "- File Version: $($fileVersion.FileVersion)"
    Write-Host "- Product Version: $($fileVersion.ProductVersion)"
    Write-Host "- Assembly Version: $($fileVersion.AssemblyVersion)"
} else {
    Write-Host "DLL not found at: $enhancedEnumsDll" -ForegroundColor Red
    Write-Host "Please build the project first with: dotnet build" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Checking for packed NuGet packages:" -ForegroundColor Cyan

# Check local nuget folder
if (Test-Path $localNugetDir) {
    $packages = Get-ChildItem $localNugetDir -Filter "DomesticatedData.EnhancedEnums.*.nupkg" | Sort-Object LastWriteTime -Descending
    if ($packages) {
        Write-Host "Found packages in local nuget folder:" -ForegroundColor Green
        foreach ($package in $packages) {
            Write-Host "- $($package.Name)" -ForegroundColor Yellow
            
            # Extract version from filename
            if ($package.Name -match 'DomesticatedData\.EnhancedEnums\.(.+)\.nupkg') {
                Write-Host "  Version: $($matches[1])" -ForegroundColor Gray
            }
        }
    } else {
        Write-Host "No packages found in: $localNugetDir" -ForegroundColor Yellow
    }
} else {
    Write-Host "Local nuget folder not found: $localNugetDir" -ForegroundColor Yellow
}

# Also check the default pack output location
$defaultPackDir = Join-Path $repoRoot "src\DomesticatedData.EnhancedEnums\bin\$Configuration"
if (Test-Path $defaultPackDir) {
    $defaultPackages = Get-ChildItem $defaultPackDir -Filter "DomesticatedData.EnhancedEnums.*.nupkg" -ErrorAction SilentlyContinue
    if ($defaultPackages) {
        Write-Host ""
        Write-Host "Found packages in default output:" -ForegroundColor Green
        foreach ($package in $defaultPackages) {
            Write-Host "- $($package.Name)" -ForegroundColor Yellow
        }
    }
}