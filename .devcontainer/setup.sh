#!/bin/bash

# CodeSpaces .NET Desktop Environment Setup Script
echo "Setting up .NET Desktop development environment..."

# Update package lists
sudo apt-get update

# Create development build directory
mkdir -p ~/OuterWildsModManager/OWML/Mods/nice2cu1.OuterWildFixFont

# Set environment variable for AppData equivalent
export APPDATA="$HOME"
echo 'export APPDATA="$HOME"' >> ~/.bashrc

# Set up development aliases
echo "Setting up development aliases..."
echo 'alias build="dotnet build OuterWildFixFont.sln --configuration Release"' >> ~/.bashrc
echo 'alias rebuild="dotnet build OuterWildFixFont.sln --configuration Release --no-incremental"' >> ~/.bashrc
echo 'alias clean="dotnet clean OuterWildFixFont.sln"' >> ~/.bashrc
echo 'alias restore="dotnet restore OuterWildFixFont.sln"' >> ~/.bashrc

# Create build script
cat > build.sh << 'EOF'
#!/bin/bash
echo "Building OuterWildFixFont..."
export APPDATA="$HOME"
dotnet build OuterWildFixFont.sln --configuration Release --verbosity minimal
if [ $? -eq 0 ]; then
    echo "Build completed successfully!"
    echo "Output directory: $HOME/OuterWildsModManager/OWML/Mods/nice2cu1.OuterWildFixFont"
else
    echo "Build failed!"
    exit 1
fi
EOF

# Create development build script (builds to local bin directory)
cat > build-dev.sh << 'EOF'
#!/bin/bash
echo "Building OuterWildFixFont for development..."
dotnet build OuterWildFixFont.sln --configuration Debug --verbosity minimal /p:OutputPath=bin/Debug
if [ $? -eq 0 ]; then
    echo "Development build completed successfully!"
    echo "Output directory: OuterWildFixFont/bin/Debug"
else
    echo "Build failed!"
    exit 1
fi
EOF

chmod +x build.sh
chmod +x build-dev.sh

echo "Setup completed! You can now:"
echo "1. Build the project with: ./build.sh"
echo "2. Build for development with: ./build-dev.sh (outputs to bin/Debug)"
echo "3. Or use dotnet directly: dotnet build OuterWildFixFont.sln"
echo "4. Use 'build', 'rebuild', 'clean', or 'restore' aliases after reloading your shell"