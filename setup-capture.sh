#!/bin/bash
# Albion Online Companion — One-time packet capture setup
# This grants the .NET runtime permission to create raw sockets without sudo.

set -e

echo "🔐 Albion Online Companion — Packet Capture Setup"
echo ""

# Find the real dotnet binary
DOTNET_PATH=$(readlink -f $(which dotnet))
echo "Found dotnet: $DOTNET_PATH"

# Check current capabilities
CURRENT_CAPS=$(getcap "$DOTNET_PATH" 2>/dev/null || echo "none")
echo "Current capabilities: $CURRENT_CAPS"

if echo "$CURRENT_CAPS" | grep -q "cap_net_raw"; then
    echo "✅ cap_net_raw already set — packet capture should work!"
    exit 0
fi

echo ""
echo "Setting cap_net_raw+ep on dotnet..."
sudo setcap cap_net_raw+ep "$DOTNET_PATH"

# Verify
NEW_CAPS=$(getcap "$DOTNET_PATH" 2>/dev/null)
echo "New capabilities: $NEW_CAPS"

if echo "$NEW_CAPS" | grep -q "cap_net_raw"; then
    echo ""
    echo "✅ Packet capture enabled! You can now run the app without sudo."
    echo "   Just click 'Start Tracking' and it will work."
else
    echo ""
    echo "❌ Failed to set capabilities. Try running the app with sudo instead."
fi
