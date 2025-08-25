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
