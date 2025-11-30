#!/bin/bash
# Copyright (c) 2025 Nikolaos Protopapas
# Licensed under the MIT License
#
# Bundle LRM binaries for VS Code extension
# Copies platform-specific binaries into vscode-extension/bin/

set -e

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m'

echo -e "${BLUE}╔════════════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║      Bundling LRM Binaries for VS Code Extension              ║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════════════════════════════╝${NC}"
echo ""

PUBLISH_DIR="publish"
EXTENSION_BIN_DIR="vscode-extension/bin"

# Check if publish directory exists
if [ ! -d "$PUBLISH_DIR" ]; then
    echo -e "${RED}Error: $PUBLISH_DIR directory not found${NC}"
    echo "Please run ./build.sh first to build all platform binaries"
    exit 1
fi

# Create extension bin directory
echo -e "${YELLOW}►${NC} Creating extension bin directory..."
rm -rf "$EXTENSION_BIN_DIR"
mkdir -p "$EXTENSION_BIN_DIR"

# Platform mappings (publish dir name -> extension bin name)
declare -A platforms=(
    ["linux-x64"]="linux-x64"
    ["linux-arm64"]="linux-arm64"
    ["win-x64"]="win32-x64"
    ["win-arm64"]="win32-arm64"
    ["osx-x64"]="darwin-x64"
    ["osx-arm64"]="darwin-arm64"
)

# Copy binaries
for pub_platform in "${!platforms[@]}"; do
    ext_platform="${platforms[$pub_platform]}"

    # Determine executable name
    if [[ $pub_platform == win-* ]]; then
        exe_name="lrm.exe"
    else
        exe_name="lrm"
    fi

    source_path="$PUBLISH_DIR/$pub_platform/$exe_name"
    dest_dir="$EXTENSION_BIN_DIR/$ext_platform"

    if [ -f "$source_path" ]; then
        mkdir -p "$dest_dir"
        cp "$source_path" "$dest_dir/"

        # Make executable on Unix platforms
        if [[ $pub_platform != win-* ]]; then
            chmod +x "$dest_dir/$exe_name"
        fi

        size=$(du -h "$source_path" | cut -f1)
        echo -e "${GREEN}✓${NC} Copied $pub_platform -> $ext_platform ($size)"
    else
        echo -e "${YELLOW}⚠${NC} Skipping $pub_platform (binary not found at $source_path)"
    fi
done

echo ""
echo -e "${GREEN}✓${NC} Binary bundling complete!"
echo ""
echo -e "${BLUE}Extension bin directory structure:${NC}"
tree -L 2 "$EXTENSION_BIN_DIR" 2>/dev/null || find "$EXTENSION_BIN_DIR" -type f
echo ""
echo -e "${BLUE}Next steps:${NC}"
echo "  1. Open vscode-extension folder in VS Code"
echo "  2. Press F5 to launch Extension Development Host"
echo "  3. Test the extension in the new VS Code window"
echo ""
