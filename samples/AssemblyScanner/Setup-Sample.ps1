<#
.SYNOPSIS
    Bootstraps the AssemblyScanner sample by packing the EnhancedEnums meta-package
    and restoring/building the SampleAllOptionConsumer against the local feed.
.DESCRIPTION
    1. Cleans and creates the ./nuget folder under this script's directory.
    2. Packs the EnhancedEnums project into the local nuget feed.
    3. Restores and builds the SampleAllOptionConsumer.
.PARAMETER Clean
    If specified, deletes all existing files in the nuget folder before packing.
.PARAMETER Release
    If specified, builds and packs projects using the Release configuration. Default is Debug.
.EXAMPLE
    .\Setup-Sample.ps1 -Clean
.EXAMPLE
    .\Setup-Sample.ps1 -Release
.EXAMPLE
    .\Setup-Sample.ps1 -Clean -Release
#>
Param(
    [switch]$Clean,
    [switch]$Release
)

# Set build configuration based on parameters
$buildConfig = if ($Release) { 'Release' } else { 'Debug' }

# Directory of this script
$scriptDir = $PSScriptRoot
$nugetDir = Join-Path $scriptDir 'nuget'
$enumBaseProj = Join-Path $scriptDir 'EnumBase\EnumBase.csproj'
$consumerDir = Join-Path $scriptDir 'SampleAllOptionConsumer'
# Directory of the EnumBase project
$enumBaseDir = Join-Path $scriptDir 'EnumBase'
# Repository root (two levels up from samples/AssemblyScanner)
$repoRoot = Join-Path $scriptDir '..\..'
# Path to the EnhancedEnums source-generator project
$enhancedEnumsProj = Join-Path $repoRoot 'src\DomesticatedData.EnhancedEnums\DomesticatedData.EnhancedEnums.csproj'

Write-Host 'EnhancedEnums project path:' $enhancedEnumsProj
if (!(Test-Path $enhancedEnumsProj)) { Write-Error "Could not locate EnhancedEnums project file at $enhancedEnumsProj"; exit 1 }

if ($Clean -and (Test-Path $nugetDir)) {
    Write-Host 'Cleaning nuget folder...'
    Remove-Item "$nugetDir\*" -Recurse -Force
}

if (!(Test-Path $nugetDir)) {
    New-Item -ItemType Directory -Path $nugetDir | Out-Null
}

# Ensure the EnhancedEnums DLL is built for packaging
Write-Host 'Building EnhancedEnums project...'
Write-Host "# dotnet build $enhancedEnumsProj -c $buildConfig -v q | Out-Null"
dotnet build $enhancedEnumsProj -c $buildConfig -v q | Out-Null
if ($LASTEXITCODE -ne 0) {
    Write-Error 'EnhancedEnums build failed'
    exit 1
}

Write-Host 'Packing EnhancedEnums package...'
Write-Host "# dotnet pack $enhancedEnumsProj -c $buildConfig -o $nugetDir"
dotnet pack $enhancedEnumsProj -c $buildConfig -o $nugetDir
if ($LASTEXITCODE -ne 0) {
    Write-Error 'EnhancedEnums pack failed'
    exit 1
}
Write-Host 'EnhancedEnums pack succeeded'
Write-Host 'NuGet feed after EnhancedEnums pack:'
Get-ChildItem $nugetDir -Filter 'DomesticatedData.EnhancedEnums*.nupkg' | ForEach-Object { Write-Host " - $_" }

Write-Host 'Building EnumBase project...'
Write-Host "# dotnet restore $enumBaseProj"
dotnet restore $enumBaseProj
if ($LASTEXITCODE -ne 0) { Write-Error 'EnumBase restore failed'; exit 1 }
Write-Host "# dotnet build $enumBaseProj -c $buildConfig"
dotnet build $enumBaseProj -c $buildConfig
if ($LASTEXITCODE -ne 0) { Write-Error 'EnumBase build failed'; exit 1 }

# Verify build output exists
$enumBaseDll = Join-Path $enumBaseDir "bin\$buildConfig\net9.0\EnumBase.dll"
Write-Host "Checking for EnumBase.dll at $enumBaseDll"
if (!(Test-Path $enumBaseDll)) { Write-Error "EnumBase.dll not found at $enumBaseDll"; exit 1 }

Write-Host 'Packing EnumBase meta-package...'
Write-Host "# dotnet pack $enumBaseProj -c $buildConfig -o $nugetDir -p:IncludeBuildOutput=true -v detailed"
dotnet pack $enumBaseProj -c $buildConfig -o $nugetDir -p:IncludeBuildOutput=true -v detailed
if ($LASTEXITCODE -ne 0) { Write-Error 'EnumBase pack failed'; exit 1 }

Write-Host 'Restoring and building SampleAllOptionConsumer...'
Push-Location $consumerDir
    Write-Host "# dotnet restore"
    dotnet restore
    Write-Host "# dotnet build -c $buildConfig"
    dotnet build -c $buildConfig
Pop-Location

Write-Host 'Sample setup complete.'
