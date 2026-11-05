# KeinSchmerzmittelFilter (No Painkiller Desaturation)

Removes the grey, desaturated colours you get while on painkillers. **Only the
picture changes**: the pain relief and everything else the painkiller does stay
exactly as they are, and it lasts just as long.

> **How this mod was made (AI disclosure):** this mod was **programmed by Claude**,
> an AI assistant made by Anthropic. **Kaan is the tester** and has no programming
> background. No human programmer has reviewed the code.

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

## Building and installing

From the repo's root folder:

```bash
./build.sh --install client-mods/KeinSchmerzmittelFilter
```
