# .NET Desktop Development Environment for CodeSpaces

This directory contains the configuration for setting up a complete .NET Desktop development environment in GitHub CodeSpaces for the OuterWildFixFont project.

## Files Overview

- `devcontainer.json` - Default Linux-based configuration with Mono support
- `devcontainer-windows.json` - Windows-based configuration (better .NET Framework support)
- `setup.sh` - Linux setup script
- `setup.ps1` - Windows setup script

## Quick Start

### Option 1: Linux with Mono (Default)
1. Open this repository in CodeSpaces
2. The default `devcontainer.json` will be used automatically
3. Wait for the container to build and setup to complete
4. Use `./build.sh` to build the project

### Option 2: Windows Container (Recommended for .NET Framework)
1. Rename `devcontainer-windows.json` to `devcontainer.json`
2. Open this repository in CodeSpaces
3. Wait for the Windows container to build and setup to complete
4. Use `.\build.ps1` to build the project

## What's Included

### Development Tools
- Visual Studio Code with C# extensions
- .NET SDK 8.0 and 6.0
- PowerShell
- Git
- MSBuild
- NuGet

### VS Code Extensions
- C# extension with IntelliSense
- .NET Test Explorer
- C# Extensions for better productivity
- PowerShell extension

### Build Tools
- MSBuild for building the solution
- NuGet for package management
- Automatic package restoration
- Build scripts for easy compilation

## Building the Project

### Linux (with Mono)
```bash
# Build the project
./build.sh

# Or use MSBuild directly
msbuild OuterWildFixFont.sln

# Clean and rebuild
msbuild OuterWildFixFont.sln /t:Rebuild
```

### Windows
```powershell
# Build the project
.\build.ps1

# Or use MSBuild directly
msbuild OuterWildFixFont.sln

# Clean and rebuild
msbuild OuterWildFixFont.sln /t:Rebuild
```

## Project Structure

This is a .NET Framework 4.8 project that creates a mod for Outer Wilds game to fix Chinese font rendering issues. The project includes:

- `OuterWildFixFont.cs` - Main mod implementation
- `OuterWildFixFont.csproj` - Project file
- `OuterWildFixFont.sln` - Solution file
- `manifest.json` - Mod manifest
- `default-config.json` - Default configuration

## Dependencies

The project uses the following NuGet packages:
- OWML (Outer Wilds Mod Loader)
- OuterWildsGameLibs (Game libraries)

These will be automatically restored when the container is created.

## Troubleshooting

### Linux Issues
- If Mono compatibility issues occur, consider using the Windows container
- Ensure all NuGet packages are restored: `nuget restore OuterWildFixFont.sln`

### Windows Issues
- If Chocolatey installation fails, try running setup manually
- Ensure .NET Framework 4.8 Developer Pack is installed

### General Issues
- Check that the solution file exists in the root directory
- Verify NuGet packages are restored successfully
- Try rebuilding the container if extensions don't load properly

## Notes

- The Linux configuration uses Mono for .NET Framework compatibility
- The Windows configuration provides native .NET Framework support
- Both configurations include the same VS Code extensions for consistency
- Build outputs will be in the standard bin/Debug or bin/Release directories