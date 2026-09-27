#!/usr/bin/env bash
set -euo pipefail

project_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$project_dir"

if [[ -z "${TIMBERBORN_GAME_DIR:-}" ]]; then
  echo "Set TIMBERBORN_GAME_DIR to your Timberborn installation directory." >&2
  exit 1
fi
if [[ ! -d "$TIMBERBORN_GAME_DIR/Timberborn_Data/Managed" || ! -f "$TIMBERBORN_GAME_DIR/Timberborn_Data/StreamingAssets/Modding/Blueprints.zip" ]]; then
  echo "TIMBERBORN_GAME_DIR must point to a Timberborn installation with game assemblies and Blueprints.zip." >&2
  exit 1
fi

dotnet restore tests/DirectionLogicTests.csproj -p:NuGetAudit=false
dotnet build DirectionalPowerShafts.csproj -c Release --no-restore
dotnet run --project tests/DirectionLogicTests.csproj -c Release --no-restore

package_dir="$project_dir/dist/DirectionalPowerShafts"
nuget_packages="${NUGET_PACKAGES:-$HOME/.nuget/packages}"
mkdir -p "$package_dir/Buildings" "$package_dir/Licenses"
cp -a Buildings/Power "$package_dir/Buildings/"
cp manifest.json "$package_dir/manifest.json"
cp bin/Release/netstandard2.1/DirectionalPowerShafts.dll "$package_dir/DirectionalPowerShafts.dll"
cp "$nuget_packages/lib.harmony/2.4.2/lib/net472/0Harmony.dll" "$package_dir/0Harmony.dll"
cp "$nuget_packages/lib.harmony/2.4.2/LICENSE" "$package_dir/Licenses/Harmony-LICENSE"
cp LICENSE "$package_dir/Licenses/DirectionalPowerShafts-LICENSE"

python3 tests/check_package.py "$package_dir"
cd "$project_dir/dist"
zip -q -r DirectionalPowerShafts-0.1.4.zip DirectionalPowerShafts
echo "Package: $project_dir/dist/DirectionalPowerShafts-0.1.4.zip"
