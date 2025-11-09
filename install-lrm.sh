#!/bin/bash
# Installation script for Localization Resource Manager (LRM)
# Usage: curl -sSL https://raw.githubusercontent.com/nickprotop/LocalizationManager/main/install-lrm.sh | bash
# Or: wget -qO- https://raw.githubusercontent.com/nickprotop/LocalizationManager/main/install-lrm.sh | bash

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}╔════════════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║   Localization Resource Manager (LRM) Installer                ║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════════════════════════════╝${NC}"
echo

# Detect OS and architecture
OS=$(uname -s)
ARCH=$(uname -m)

if [[ "$OS" == "Linux" ]]; then
    if [[ "$ARCH" == "x86_64" ]]; then
        PLATFORM="linux-x64"
    elif [[ "$ARCH" == "aarch64" || "$ARCH" == "arm64" ]]; then
        PLATFORM="linux-arm64"
    else
        echo -e "${RED}Error: Unsupported architecture: $ARCH${NC}"
        exit 1
    fi
elif [[ "$OS" == "Darwin" ]]; then
    echo -e "${RED}Error: macOS is not yet supported. Please download manually from releases.${NC}"
    exit 1
else
    echo -e "${RED}Error: Unsupported OS: $OS${NC}"
    echo "This script is for Linux only. For Windows, download from:"
    echo "https://github.com/nickprotop/LocalizationManager/releases"
    exit 1
fi

echo -e "${GREEN}✓${NC} Detected platform: $PLATFORM"

# Get latest version
echo "Fetching latest version..."
VERSION=$(curl -s https://api.github.com/repos/nickprotop/LocalizationManager/releases/latest | grep '"tag_name":' | sed -E 's/.*"v([^"]+)".*/\1/')

if [[ -z "$VERSION" ]]; then
    echo -e "${RED}Error: Could not fetch latest version${NC}"
    exit 1
fi

echo -e "${GREEN}✓${NC} Latest version: v$VERSION"

# Download URL
FILENAME="lrm-${VERSION}-${PLATFORM}.tar.gz"
URL="https://github.com/nickprotop/LocalizationManager/releases/download/v${VERSION}/${FILENAME}"

echo "Downloading LRM..."
TEMP_DIR=$(mktemp -d)
cd "$TEMP_DIR"

if ! curl -L -o "$FILENAME" "$URL"; then
    echo -e "${RED}Error: Download failed${NC}"
    rm -rf "$TEMP_DIR"
    exit 1
fi

echo -e "${GREEN}✓${NC} Downloaded successfully"

# Extract
echo "Extracting..."
tar -xzf "$FILENAME"

# Install
INSTALL_DIR="$HOME/.local/bin"
mkdir -p "$INSTALL_DIR"

cp "${PLATFORM}/lrm" "$INSTALL_DIR/lrm"
chmod +x "$INSTALL_DIR/lrm"

echo -e "${GREEN}✓${NC} Installed to $INSTALL_DIR/lrm"

# Cleanup
cd - > /dev/null
rm -rf "$TEMP_DIR"

# Check if in PATH
if [[ ":$PATH:" != *":$INSTALL_DIR:"* ]]; then
    echo
    echo -e "${BLUE}Note: $INSTALL_DIR is not in your PATH${NC}"
    echo "Add this to your ~/.bashrc or ~/.zshrc:"
    echo
    echo "    export PATH=\"\$HOME/.local/bin:\$PATH\""
    echo
else
    echo -e "${GREEN}✓${NC} $INSTALL_DIR is already in your PATH"
fi

# Verify installation
echo
echo "Verifying installation..."
if "$INSTALL_DIR/lrm" --version > /dev/null 2>&1; then
    echo -e "${GREEN}✓${NC} LRM installed successfully!"
    echo
    "$INSTALL_DIR/lrm" --version
    echo
    echo "Get started:"
    echo "  lrm --help"
    echo "  lrm validate --path /path/to/resources"
else
    echo -e "${RED}Error: Installation verification failed${NC}"
    exit 1
fi
