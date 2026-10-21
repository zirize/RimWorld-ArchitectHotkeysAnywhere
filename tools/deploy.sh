#!/bin/bash
# Build the mod and copy the loadable parts into RimWorld's Mods folder.
# Usage: tools/deploy.sh [RimWorldDir]   Undo: rm -rf "<RimWorldDir>/Mods/ArchitectHotkeysAnywhere"
set -euo pipefail
cd "$(dirname "$0")/.."
RW="${1:-/mnt/steam/SteamLibrary/steamapps/common/RimWorld}"
export DOTNET_ROOT="$HOME/.dotnet" PATH="$HOME/.dotnet:$PATH" DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1
dotnet build Source/ArchitectHotkeysAnywhere.csproj -c Release -p:RimWorldDir="$RW" -v q -nologo
DEST="$RW/Mods/ArchitectHotkeysAnywhere"
rm -rf "$DEST"
mkdir -p "$DEST"
cp -r About Assemblies "$DEST/"
echo "deployed to $DEST"
