#!/bin/bash

echo "Killing all .NET processes..."

# Kill all dotnet processes
pkill -9 dotnet 2>/dev/null || true

# Kill OptimalyChat processes specifically
pkill -9 "OptimalyC" 2>/dev/null || true

# Kill specific .NET related processes
pkill -9 "Microsoft.CodeAnalysis" 2>/dev/null || true
pkill -9 "rzls" 2>/dev/null || true
pkill -9 "Microsoft.VisualStudio" 2>/dev/null || true

# Kill by process IDs if any remain
ps aux | grep -E "(dotnet|Microsoft\.|OptimalyC)" | grep -v grep | awk '{print $2}' | xargs kill -9 2>/dev/null || true

# Kill anything on port 5020
lsof -ti:5020 | xargs kill -9 2>/dev/null || true

# Clean up any locks
rm -f /tmp/*.lock 2>/dev/null || true

echo "Done. All .NET processes should be terminated."
echo ""
echo "Note: VS Code may restart some language server processes automatically."
echo "If you're debugging, use F5 in VS Code which will handle ports correctly."