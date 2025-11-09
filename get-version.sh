#!/bin/bash
# Get current version from LocalizationManager.csproj
# Outputs raw version number for use in scripts and GitHub Actions

CSPROJ_FILE="LocalizationManager.csproj"

# Check if file exists
if [ ! -f "$CSPROJ_FILE" ]; then
    echo "Error: $CSPROJ_FILE not found" >&2
    exit 1
fi

# Extract version
VERSION=$(grep -oP '(?<=<Version>)[^<]+' "$CSPROJ_FILE" | head -1)

# Check if version was found
if [ -z "$VERSION" ]; then
    echo "Error: Could not extract version from $CSPROJ_FILE" >&2
    exit 1
fi

# Output version (raw, no formatting)
echo "$VERSION"
