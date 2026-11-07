# KeinSchmerzmittelFilter (No Painkiller Desaturation)

> **Now part of [Kaan's QoL](https://github.com/Kaanori/spt-kaansqol)** – this module is included there (with your settings migrated automatically). This stand-alone version is no longer updated.

Removes the grey, desaturated colours you get while on painkillers. **Only the
picture changes**: the pain relief and everything else the painkiller does stay
exactly as they are, and it lasts just as long.

## How this mod was made (AI disclosure)

This mod was **programmed by Claude**, an AI assistant made by Anthropic.
**Kaan was the tester**: Kaan has no programming background, played with the mod
on SPT 4.1.6, reported what went wrong, and Claude fixed it. No human programmer
has reviewed the code. It is open source so anyone can check it — if you find a
bug, please open an [issue](https://github.com/Kaanori/spt-keinschmerzmittelfilter/issues).

## Requirements

- SPT 4.1.6 (EFT 0.16.9). BepInEx ships with SPT.

## Installation

1. Download `KeinSchmerzmittelFilter-1.0.0.zip` from the [Releases](https://github.com/Kaanori/spt-keinschmerzmittelfilter/releases) page.
2. Extract it **into your SPT game folder** (the folder that contains `EscapeFromTarkov.exe`).
   The ZIP already has the right layout; the mod ends up in `BepInEx/plugins/`.
   On Windows, make sure "Extract All" does not add an extra subfolder.
3. Restart the game.

## Uninstall

Delete the mod's files from `BepInEx/plugins/` (settings: `BepInEx/config/` for client mods) and restart the game.

## Why it is needed

The game has no setting for this. While a painkiller runs,
`EffectsController.DesaturateMaskAccumulator` moves two values every frame:
`MaskDesaturate` towards 1 and the radius of the mask from 1 down to 0.325.
`CC_Sharpen` / `DesaturateEffect` pass them to the shader as `_desatMask` and
`_radius`.

That accumulator is created for `IPainKiller` only, and nothing else writes
`MaskDesaturate`. So this covers everything that starts the game's painkiller
effect, and no other effect.

## How it works

One Harmony prefix on `DesaturateMaskAccumulator.UpdateAmount`. It sets the two
target values to their resting state, the same ones the accumulator aims for
when no painkiller is active. The game moves the values itself, so switching
the mod on during a raid fades the effect out instead of cutting it.

The health effect itself is not touched. Its run, duration and pain relief live
in the health controller, while `EffectsController` only listens to it from the
camera. The effect also stays registered in the other accumulators, which
matters: `CC_RadialBlurAccumulator` needs it to turn off the blur of pain.

Only names that EFT does not obfuscate are addressed.

## What stays

- **Blur** when a painkiller wears off and when you are in pain: a separate
  effect, not desaturation.
- **Slight desaturation while exhausted:** a separate value in the same shader
  (`HealthDesaturate`), unrelated to painkillers.
- **Desaturation from weather** (`WeatherDesaturate`).

## Settings (F12 → No Painkiller Desaturation)

| Setting | Default | Meaning |
|---|---|---|
| **Enabled** | on | Off = the game's normal effect. Takes effect immediately, also during a raid. |

## Building from source

Needs the .NET 10 SDK and an SPT 4.1.6 installation to build against.

```bash
./build.sh             # Linux: build only
./build.sh --install   # build and copy into the game folder
```
```powershell
.\build.ps1            # Windows
.\build.ps1 -Install
```

The game folder is set in `Directory.Build.props` (Windows default `C:/SPT`)
or via the environment variable `SPT_DIR`.

## Support

Bugs and questions: [GitHub issues](https://github.com/Kaanori/spt-keinschmerzmittelfilter/issues).

## License

MIT — see [LICENSE](LICENSE).
