# Directional Power Shafts

Adds construction order arrows to Timberborn's vanilla **Power Shaft** and
**Vertical Power Shaft**, for both Folktails and Iron Teeth. Tested in game on
Timberborn **1.1.2.4** under Steam Proton.

The Timberborn manifest `Id` is `directional-power-shafts`. It identifies the
mod to Timberborn and is separate from the Steam Workshop `ItemId`, which Steam
assigns to a Workshop item.

## What it does

- Choose the arrow direction with the game's rotate control (normally **R**).
  The direction shown while hovering stays the same when a drag begins, just
  as with terrain blocks. Press R during the drag to turn all preview arrows
  in 90-degree steps. Two turns reverse the direction.
- An unfinished shaft waits for an adjacent unfinished shaft behind its arrow
  when their directions form a link. When that shaft finishes or is removed,
  the waiting site becomes buildable. This works across the two shaft types at
  the same height. Completed shafts stop showing arrows and transfer power as
  usual.
- Every shaft in one drag gets the selected direction, including a drag with a
  corner. A line only builds in sequence when its arrows point along that line
  and adjacent unfinished shafts form a matching link. Arrows pointing across
  a line do not order that line's construction.
- Shaft sites already planned before installing the mod keep their original
  independent construction behavior. Directional behavior is enabled for new
  sites and saved with them.

This borrows the **behavior** of Timberborn's terrain block: an orientation
controls a visible arrow and the construction site's blocked state. It uses
the game's `BlockableObject` mechanism and its built-in `DirectionalBlocking`
status icon and text. It does **not** copy the terrain block's model, texture,
source code, or blueprint. The arrow mesh is created by this mod at runtime.

## How it works

| Part | Purpose |
| --- | --- |
| Four `Buildings/Power/...blueprint.json` patches | Add `DirectionalShaftSpec` to the two vanilla shaft types for both factions. |
| `DirectionalShaftConfigurator.cs` | Attaches the `DirectionalShaft` component to those templates through Bindito. |
| `DirectionalShaft.cs` | Tracks unfinished sites, saves whether the directional feature is enabled, links matching neighbors, blocks and unblocks construction, and shows the game's waiting status. |
| `ShaftArrow.cs` | Draws the orange arrow on previews and enabled unfinished sites. |
| `PlacementPatches.cs` and `ShaftLineDirections.cs` | Apply the current tool rotation to every shaft in a drag and enable the feature on newly created construction sites. |

Timberborn's [mod loader](https://github.com/mechanistry/timberborn-modding/wiki/Coding-basics)
loads `DirectionalPowerShafts.dll` and calls `ModStarter.StartMod()`. The starter
registers two Harmony **postfix** patches:

1. `AreaPicker.GetPlacements`: applies the selected tool orientation only to
   the four named shaft templates with the game's two-segment-line layout.
   Both preview and final placement pass through this method.
2. `ConstructionFactory.CreateAsUnfinished`: enables directional behavior on
   a new shaft site. It has no effect on other construction sites.

The patches run after the game's methods. No installed game files are edited.

## Compatibility and dependencies

- **Verified game version:** 1.1.2.4. `MinimumGameVersion` in `manifest.json`
  is a loader minimum, not proof that newer game versions work. Recheck and
  rebuild for each new Timberborn version before claiming support.
- **Platforms:** tested on Linux with Steam Proton. The mod uses game DLLs and
  managed C# code; native Linux and Windows game runs are not yet verified.
- **Other enabled mods:** none required. The package includes its own
  `0Harmony.dll`. Mods that patch either game method or replace the same shaft
  blueprints may affect behavior; test those combinations in game.
- **Saves:** newly placed directional shaft sites store one enabled flag.
  Older shaft sites without that flag stay independent. Keep the mod enabled
  when loading a save that contains its directional sites.
- **Scope:** construction order is only linked between adjacent unfinished
  shafts at the same height whose directions match. Vertical Power Shaft is a
  supported *building type*; the mod does not order shafts between height
  levels.

### Licenses

This project's original code is available under the
**Non-Commercial Attribution License** in [LICENSE](LICENSE). You may use,
modify, and share it for free with credit to adikovec; you may not sell it or
charge for access to it or a derivative.

The project references **Lib.Harmony 2.4.2** in
`DirectionalPowerShafts.csproj` and ships its `0Harmony.dll` so players do not
have to enable another mod. Harmony is by Andreas Pardeike and is licensed
under the **MIT License**. The complete license notice is included in the ZIP
as `DirectionalPowerShafts/Licenses/Harmony-LICENSE`. See the
[Harmony project](https://github.com/pardeike/Harmony) and
[license](https://github.com/pardeike/Harmony/blob/master/LICENSE).

## Build from source

### Bazzite

Bazzite is an immutable Fedora-based system. One convenient way to install
build tools is in a Fedora Distrobox, which keeps them separate from the host.
Create and enter a build container, then install the required tools:

```bash
distrobox create --name timberborn-mod-build --image fedora:latest
distrobox enter timberborn-mod-build
sudo dnf install dotnet-sdk-10.0 python3 zip unzip
```

See [Bazzite's Distrobox guide](https://docs.bazzite.gg/Installing_and_Managing_Software/Distrobox/)
for more information about containers.

Run the build commands from inside the container. Distrobox shares your home
directory by default, so a repository and Steam library under your home folder
are available there.

### Ubuntu

On Ubuntu 24.04 or newer, install the .NET 10 SDK and the packaging tools:

```bash
sudo apt update
sudo apt install dotnet-sdk-10.0 python3 zip unzip
```

For other Ubuntu releases or derivatives where that package is unavailable,
follow [Microsoft's Ubuntu installation guide](https://learn.microsoft.com/en-us/dotnet/core/install/linux-ubuntu-install)
for your specific release.

### Build the mod

Both setups use the same build command. Requirements are the .NET 10 SDK,
Python 3, `zip`/`unzip`, a local Timberborn installation, and NuGet access to
restore Lib.Harmony 2.4.2. Set `TIMBERBORN_GAME_DIR` to the game installation
directory (the directory containing `Timberborn_Data`), then run:

```bash
cd /path/to/DirectionalPowerShafts-mod
export TIMBERBORN_GAME_DIR=/path/to/Timberborn
bash package.sh
```

Confirm the SDK is available with `dotnet --list-sdks`; it should list a 10.x
SDK. If your Steam library is outside your home directory when building inside
Distrobox, make sure that path is available in the container.

`package.sh` restores dependencies, builds the mod, runs the local tests,
checks the package contents, and creates
`dist/DirectionalPowerShafts-0.1.4.zip`. The ZIP contains the mod DLL, four
small blueprint patches, `0Harmony.dll`, both license notices, and
`manifest.json`.
Game DLLs are referenced for compilation; the game blueprint archive is
checked during packaging. Neither is included in the mod ZIP.

## Tests and in-game verification

The tests cover two different things:

- `tests/DirectionLogicTests.cs` checks that the selected arrow direction is
  kept for a hovered shaft, a straight drag, and a corner drag, for all four
  rotations. It also checks that coordinates and flip modes are preserved.
  It does not run Timberborn's preview UI or construction simulation.
- `tests/check_package.py` checks the mod ID and minimum version, confirms
  the four blueprint paths exist in the installed game's `Blueprints.zip`,
  validates the patch JSON and DLL headers, and rejects unexpected files in
  the package. It does not prove that the game can load or execute the mod.

The in-game check is therefore required for a new release:

1. In a new or disposable colony, hover a shaft, then drag a line. Check that
   the arrow direction does not change when the drag begins. Rotate before and
   during the drag, then check that placed arrows match the preview.
2. Check forward and reverse order on straight lines of each shaft type, an
   L-shaped preview, mixed Power Shaft/Vertical Power Shaft sites at the same
   height, and a single rotated shaft.
3. Check both factions. Save and reload while a line is partly built. Remove
   an unfinished shaft that blocks another and check that the next site
   becomes buildable.
4. Confirm completed shafts have no arrow and still transmit power. If any
   step fails, collect Timberborn's `Player.log` and the exact placement steps.

## Updating for a new Timberborn version

1. Keep a copy of a working game installation and a disposable test save.
   Update the local Timberborn installation used for development, then read
   `Timberborn_Data/StreamingAssets/VersionNumbers.json` to get its actual
   `CurrentVersion`.
2. Inspect `Timberborn_Data/StreamingAssets/Modding/Blueprints.zip`: confirm
   all four shaft blueprint paths still exist, still represent the intended
   buildings, and still use the expected placement layout. Check that the
   minimal `DirectionalShaftSpec` patches still merge correctly.
3. Compare the new `Timberborn_Data/Managed` APIs with those used in `src/`.
   In particular check `AreaPicker.GetPlacements` and
   `ConstructionFactory.CreateAsUnfinished` (Harmony targets),
   `PreviewPlacement` rotation, `BlockObject` orientation, `BlockableObject`,
   construction-state events, save/load interfaces, the `DirectionalBlocking`
   icon, and the Bindito template decorator. Update code if their behavior or
   signatures changed.
4. Point `TIMBERBORN_GAME_DIR` at the new installation.
   Update `MinimumGameVersion` in `manifest.json` if older versions are no
   longer supported, and update the matching assertion in
   `tests/check_package.py`. Bump the mod's `Version` in `manifest.json`, the
   ZIP name in `package.sh`, and the version references in this README.
5. Run `bash package.sh`, then `unzip -t` on the resulting ZIP. Run every
   in-game check above on the new game version before distributing it.

A successful compile and package check only show that the referenced symbols
and expected files exist. Runtime behavior and compatibility still need the
in-game check.

## Install locally

Extract the ZIP into Timberborn's local `Mods` directory so it contains
`Mods/DirectionalPowerShafts/manifest.json`. The directory is under Timberborn's
user data folder; its location depends on your operating system and, for Steam
Proton, your Steam library and compatibility prefix. Restart the game and
enable the mod.

If the game fails to load the mod or a test behaves differently, include the
`Player.log` from Timberborn's user-data directory and the test step.
