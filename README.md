# Endless Bounty

A BepInEx mod for [Sineus Arena](https://store.steampowered.com/) that adds bonus rewards for surviving the endless post-boss "final wave."

After you defeat the boss, the game keeps spawning enemies forever in the final wave. The game already grants coins for killing them - but it never drops bonus chests, elite items, or building upgrades there. This mod adds those back in as occasional rewards for surviving.

## Features

- A free bonus chest every couple of minutes survived in the final wave
- A random item from the game's own elite drop table (like the Magnet pickup) every minute and a half or so
- A free building-upgrade pickup every few minutes - skipped automatically if every player's buildings are already maxed out, so it's never wasted
- Paced by **time survived**, not kill count - the final wave can hit hundreds of kills per minute, so a kill-counter-based reward would fire constantly and feel spammy
- Fully co-op compatible: reward-granting is handled by the host so nothing desyncs between players
- A small "Endless Bounty" HUD readout above the objective banner, showing time survived and a countdown to each next reward

## Install

1. Install [BepInEx 5.4.23.3 (Mono/x64)](https://github.com/BepInEx/BepInEx/releases) if you haven't already.
2. Grab the latest release from the [Releases page](../../releases) and extract the `BepInEx` folder into your Sineus Arena install directory, merging with your existing `BepInEx` folder.
3. Launch the game through Steam.

Requires [SineusModding.Api](https://github.com/maanu113/SineusModding.Api) (included in the release zip).

## Controls

- **F8** - toggle Endless Bounty on/off

## Configuration

Everything is saved in `BepInEx/config/com.community.sineusarena.endlessbounty.cfg`. You can tune how many minutes between each chest/item/building-upgrade (set any to `0` to disable it entirely).

## Building from source

```
dotnet build -c Release
```

The `.csproj` references game/BepInEx assemblies via `HintPath` pointing at a local Sineus Arena install (`GameDir` in the `.csproj` - edit if yours is elsewhere), and references `lib/SineusModding.Api.dll` (vendored in this repo) for the shared API.

## License

No license specified yet - all rights reserved by default. Contact the author before redistributing.
