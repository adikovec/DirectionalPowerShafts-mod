#!/usr/bin/env python3
"""Check the mod package against the installed game's target blueprints."""

import json
import os
import sys
import zipfile
from pathlib import Path

package = Path(sys.argv[1])
game_dir = os.environ.get("TIMBERBORN_GAME_DIR")
if not game_dir:
    raise SystemExit("Set TIMBERBORN_GAME_DIR to your Timberborn installation directory.")
game_blueprints = (
    Path(game_dir) / "Timberborn_Data/StreamingAssets/Modding/Blueprints.zip"
)
if not game_blueprints.is_file():
    raise SystemExit(f"Timberborn blueprint archive not found: {game_blueprints}")
expected = [
    f"Buildings/Power/{building}/{building}.{faction}.blueprint.json"
    for building in ("PowerShaft", "VerticalPowerShaft")
    for faction in ("Folktails", "IronTeeth")
]

manifest = json.loads((package / "manifest.json").read_text())
assert manifest["Id"] == "directional-power-shafts"
assert manifest["MinimumGameVersion"] == "1.1.2.4"
assert (package / "DirectionalPowerShafts.dll").read_bytes()[:2] == b"MZ"
assert (package / "0Harmony.dll").read_bytes()[:2] == b"MZ"

with zipfile.ZipFile(game_blueprints) as game:
    for relative in expected:
        assert relative in game.namelist(), f"Game blueprint absent: {relative}"
        patch = json.loads((package / relative).read_text())
        assert patch == {"DirectionalShaftSpec": {}}, relative

packaged_files = {
    path.relative_to(package).as_posix()
    for path in package.rglob("*")
    if path.is_file()
}
assert packaged_files == set(expected) | {
    "manifest.json",
    "DirectionalPowerShafts.dll",
    "0Harmony.dll",
    "Licenses/Harmony-LICENSE",
    "Licenses/DirectionalPowerShafts-LICENSE",
}, packaged_files
print("Package contents, blueprint paths, and target version: OK")
