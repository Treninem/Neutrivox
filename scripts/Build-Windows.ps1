param(
    [string]$Configuration = 'Release',
    [string]$Runtime = 'win-x64',
    [string]$Output = 'artifacts\windows'
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

if (Test-Path $Output) {
    Remove-Item $Output -Recurse -Force
}
New-Item -ItemType Directory -Path $Output | Out-Null

Write-Host '== Restore =='
dotnet restore .\Neutrivox.sln

Write-Host '== Release build =='
dotnet build .\Neutrivox.sln --configuration $Configuration --no-restore

Write-Host '== Smoke build/run =='
dotnet build .\tools\Neutrivox.Smoke\Neutrivox.Smoke.csproj --configuration $Configuration
dotnet run --project .\tools\Neutrivox.Smoke\Neutrivox.Smoke.csproj --configuration $Configuration --no-build

Write-Host '== Windows publish =='
dotnet publish .\src\Neutrivox\Neutrivox.csproj `
    --configuration $Configuration `
    --runtime $Runtime `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:PublishTrimmed=false `
    --output $Output

Write-Host "Published to $Output"
