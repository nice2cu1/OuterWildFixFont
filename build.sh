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
