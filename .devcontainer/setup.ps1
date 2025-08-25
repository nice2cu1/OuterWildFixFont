# CodeSpaces .NET Desktop Environment Setup Script (Windows)
Write-Host "Setting up .NET Desktop development environment on Windows..." -ForegroundColor Green

# Install Chocolatey if not already installed
if (-not (Get-Command choco -ErrorAction SilentlyContinue)) {
    Write-Host "Installing Chocolatey..." -ForegroundColor Yellow
    Set-ExecutionPolicy Bypass -Scope Process -Force
    [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072
    iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))
}

# Install .NET Framework 4.8 SDK
Write-Host "Installing .NET Framework 4.8 Developer Pack..." -ForegroundColor Yellow
choco install netfx-4.8-devpack -y

# Install MSBuild Tools
Write-Host "Installing MSBuild Tools..." -ForegroundColor Yellow
choco install microsoft-build-tools -y

# Install NuGet
Write-Host "Installing NuGet..." -ForegroundColor Yellow
choco install nuget.commandline -y

# Restore NuGet packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
if (Test-Path "OuterWildFixFont.sln") {
    nuget restore OuterWildFixFont.sln
    Write-Host "NuGet packages restored successfully." -ForegroundColor Green
} else {
    Write-Host "Solution file not found. Skipping NuGet restore." -ForegroundColor Yellow
}

# Create development build directory
New-Item -ItemType Directory -Force -Path "$env:USERPROFILE\OuterWildsModManager\OWML\Mods\nice2cu1.OuterWildFixFont"

# Create build script
$buildScript = @'
# Build script for OuterWildFixFont
Write-Host "Building OuterWildFixFont..." -ForegroundColor Green
dotnet build OuterWildFixFont.sln --configuration Release --verbosity minimal
if ($LASTEXITCODE -eq 0) {
    Write-Host "Build completed successfully!" -ForegroundColor Green
    Write-Host "Output directory: $env:USERPROFILE\OuterWildsModManager\OWML\Mods\nice2cu1.OuterWildFixFont" -ForegroundColor Cyan
} else {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}
'@

$buildScript | Out-File -FilePath "build.ps1" -Encoding UTF8

# Create development build script
$buildDevScript = @'
# Development build script for OuterWildFixFont
Write-Host "Building OuterWildFixFont for development..." -ForegroundColor Green
dotnet build OuterWildFixFont.sln --configuration Debug --verbosity minimal /p:OutputPath=bin/Debug
if ($LASTEXITCODE -eq 0) {
    Write-Host "Development build completed successfully!" -ForegroundColor Green
    Write-Host "Output directory: OuterWildFixFont\bin\Debug" -ForegroundColor Cyan
} else {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}
'@

$buildDevScript | Out-File -FilePath "build-dev.ps1" -Encoding UTF8

Write-Host "Setup completed!" -ForegroundColor Green
Write-Host "You can now:" -ForegroundColor Cyan
Write-Host "1. Build the project with: .\build.ps1" -ForegroundColor White
Write-Host "2. Build for development with: .\build-dev.ps1 (outputs to bin\Debug)" -ForegroundColor White
Write-Host "3. Or use dotnet directly: dotnet build OuterWildFixFont.sln" -ForegroundColor White