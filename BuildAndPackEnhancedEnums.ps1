# BuildAndPackEnhancedEnums.ps1
param(
    [string]$OutputPath = "samples/CrossAssemblyScanner/LocalPackages",
    [string]$Configuration = "Release"
)

Write-Host "Building and Packing Enhanced Enums packages..." -ForegroundColor Green

# Create output directory
if (!(Test-Path $OutputPath)) {
    New-Item -ItemType Directory -Path $OutputPath -Force
}

# Core packages (required for all scenarios)
$corePackages = @(
    "src/FractalDataWorks/FractalDataWorks.csproj",
    "src/FractalDataWorks.EnhancedEnums/FractalDataWorks.EnhancedEnums.csproj"
)

# Generator packages
$generatorPackages = @(
    "src/FractalDataWorks.EnhancedEnums.SourceGenerator/FractalDataWorks.EnhancedEnums.SourceGenerator.csproj",
    "src/FractalDataWorks.EnhancedEnums.CrossAssembly/FractalDataWorks.EnhancedEnums.CrossAssembly.csproj"
)

# Sample packages for cross-assembly demos
$samplePackages = @(
    "samples/CrossAssemblyScanner/ColorOption.Library/ColorOption.Library.csproj",
    "samples/CrossAssemblyScanner/Red.Library/Red.Library.csproj", 
    "samples/CrossAssemblyScanner/Blue.Library/Blue.Library.csproj"
)

Write-Host "Packing core packages..." -ForegroundColor Cyan
foreach ($project in $corePackages) {
    Write-Host "  Packing $project..." -ForegroundColor Yellow
    dotnet pack $project --configuration $Configuration --output $OutputPath --force --no-build
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to pack $project"
        exit 1
    }
}

Write-Host "Packing generator packages..." -ForegroundColor Cyan
foreach ($project in $generatorPackages) {
    Write-Host "  Packing $project..." -ForegroundColor Yellow
    dotnet pack $project --configuration $Configuration --output $OutputPath --force --no-build
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to pack $project"
        exit 1
    }
}

Write-Host "Packing sample packages for cross-assembly demo..." -ForegroundColor Cyan
foreach ($project in $samplePackages) {
    Write-Host "  Packing $project..." -ForegroundColor Yellow
    dotnet pack $project --configuration $Configuration --output $OutputPath --force
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to pack $project"
        exit 1
    }
}

Write-Host "All packages built successfully!" -ForegroundColor Green
Write-Host "Output directory: $OutputPath" -ForegroundColor Cyan

# List generated packages
Write-Host "Generated packages:" -ForegroundColor White
Get-ChildItem $OutputPath -Filter "*.nupkg" | Sort-Object Name | ForEach-Object {
    Write-Host "  - $($_.Name)" -ForegroundColor White
}

Write-Host ""
Write-Host "To use these packages in the cross-assembly sample:" -ForegroundColor Yellow
Write-Host "1. Run: dotnet restore samples/CrossAssemblyScanner/ConsoleApp" -ForegroundColor White
Write-Host "2. Run: dotnet build samples/CrossAssemblyScanner/ConsoleApp" -ForegroundColor White
Write-Host "3. Run: dotnet run --project samples/CrossAssemblyScanner/ConsoleApp" -ForegroundColor White