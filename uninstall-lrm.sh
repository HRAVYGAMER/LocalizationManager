#!/bin/bash
# Uninstallation script for Localization Resource Manager (LRM)
# Usage: curl -sSL https://raw.githubusercontent.com/nickprotop/LocalizationManager/main/uninstall-lrm.sh | bash
# Or: bash uninstall-lrm.sh

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${BLUE}╔════════════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║   Localization Resource Manager (LRM) Uninstaller              ║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════════════════════════════╝${NC}"
echo

# Remove binary
echo "Removing installed components..."
if [ -f "$HOME/.local/bin/lrm" ]; then
    rm -f "$HOME/.local/bin/lrm"
    echo -e "  ${GREEN}✓${NC} Removed binary: ~/.local/bin/lrm"
else
    echo "  • Binary not found (already removed?)"
fi

# Remove bash completion
BASH_COMP="$HOME/.local/share/bash-completion/completions/lrm"
if [ -f "$BASH_COMP" ]; then
    rm -f "$BASH_COMP"
    echo -e "  ${GREEN}✓${NC} Removed bash completion"
    # Remove directory if empty
    rmdir "$HOME/.local/share/bash-completion/completions" 2>/dev/null || true
    rmdir "$HOME/.local/share/bash-completion" 2>/dev/null || true
fi

# Remove zsh completion
ZSH_COMP="$HOME/.zsh/completions/_lrm"
if [ -f "$ZSH_COMP" ]; then
    rm -f "$ZSH_COMP"
    echo -e "  ${GREEN}✓${NC} Removed zsh completion"
    # Remove directory if empty
    rmdir "$HOME/.zsh/completions" 2>/dev/null || true
fi

echo
echo -e "${GREEN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${GREEN}✓ Uninstallation complete!${NC}"
echo -e "${GREEN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo
echo "Optional manual cleanup:"
echo
echo "1. Remove PATH configuration (if added):"
echo "   Edit ~/.bashrc or ~/.zshrc and remove:"
echo "   ${YELLOW}export PATH=\"\$HOME/.local/bin:\$PATH\"${NC}"
echo
echo "2. Remove zsh fpath configuration (if added):"
echo "   Edit ~/.zshrc and remove:"
echo "   ${YELLOW}fpath=(~/.zsh/completions \$fpath)${NC}"
echo
echo "3. Restart your shell to apply changes:"
echo "   ${BLUE}exec bash${NC}  or  ${BLUE}exec zsh${NC}"
echo
