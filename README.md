# SciLor's Mashed Trainer v0.2.1

A trainer for the PC game **Mashed: Fully Loaded**. Change scores, weapons, damage and positions for every player, tweak the camera, and add fun modifiers like random weapons and drive-over revive.

---

## Screenshots

### Players Tab
![Players Tab](screenshots/tab_Players.png)

### Game Tab
![Game Tab](screenshots/tab_Game.png)

---

## Downloads & Requirements

The trainer is distributed in two builds on each release:

1. **Standalone (Zero Prerequisites)**: Self-contained single executable (`MashedTrainer-Standalone-win-x86.zip`). Requires no .NET runtime installed.
2. **Tiny (Framework-Dependent)**: Ultra-compact build (~3 MB) (`MashedTrainer-Tiny-win-x86.zip`). Requires the [.NET 8 Desktop Runtime (x86)](https://dotnet.microsoft.com/download/dotnet/8.0).

> **Note**: I recommend using [DXWnd](https://sourceforge.net/projects/dxwnd/) to run Mashed in windowed mode alongside the trainer.

---

## Features

### General
- **Icon Packs**: Switch weapon icons between the in-game look (default) and the classic set via the *Icons* menu
- **Status Bar**: Shows connection state and tells you when memory writes fail (run the trainer as administrator in that case); controls stay disabled until Mashed is found

### Player (Per-Player Controls for P1–P4)
- **Score**: Change and freeze points
- **Revive**: Instant respawn and revive toggle
- **Teleport & Freeze**: View and set coordinates (X, Y, Z) and freeze axes individually
- **Controls**: Disable/enable player vehicle controls
- **Weapons**: Equip any of the 9 weapons instantly, or drop active weapon
- **Damage**: Adjust front and back damage sliders, toggle individual damage parts (Hood, Trunk, Glass), and 1-click Full Repair
- **Car Physics**: 1-click **Flip** (rights an upside-down car) and **Turn 180°**
- **God Mode**: Option in the Damage block that keeps the car fully repaired while alive
- **Bot Badge**: Bot-controlled players are marked with a BOT tag

### Game & Camera
- **Destroy Distance**: Customize warning threshold and maximum elimination distance
- **Maximum Damage**: Set maximum damage cap (normally 50%)
- **Camera Tuning**: Adjust camera tilt multiplier, height distance divider, height add, height distance factor, and zoom limit

### Fun Modifiers
- **Random Weapon Equip**: Periodically equips randomized weapons for all players or bots
- **Drive-Over Revive**: Drive over eliminated opponents to bring them back into the race, automatically flipping cars on their roof

---

## Building & Testing

The trainer is written in C# on .NET 8 with Avalonia UI. It comes with automated headless GUI tests and a built-in game simulator, so it can be developed and tested without the game.

### Requirements
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Building
```bash
# Build trainer
dotnet build MashedTrainer.sln -c Release

# Run automated tests and capture headless GUI screenshots
dotnet test MashedTrainer.sln -c Release
```

### Branches & Releases
Development happens on `develop` (CI builds and tests every push). A release is a pull request from `develop` to `master`. After merging, tag the merge commit on `master` (`git tag v0.2.1 && git push origin v0.2.1`); pushing the tag makes GitHub Actions build the Tiny and Standalone zips and create a draft release. Bump the version in `MainWindow.axaml.cs`, `MainWindow.axaml` and this README before opening the release PR.

### Simulator
Start the trainer with `--simulate` to run against a built-in fake game (`MockMemoryTarget`) instead of MFL.exe. This allows developing, testing and rendering the GUI without Windows or the game.

---

## ChangeLog

### v0.2.1 (unreleased)
- Ported from .NET Framework 4.5 / WPF to .NET 8 / Avalonia
- New dark theme, separate Game tab, Flip / Turn 180° actions, camera height factor and zoom limit
- Added icon packs (in-game, classic) selectable in the *Icons* menu
- Added per-player god mode and a BOT badge for bot players
- Game info (round state, players, points to win) is shown in the status bar
- Game tab uses numeric spinners with limits
- Controls stay disabled until Mashed is attached; failed memory writes are reported in the status bar
- Simulator for development is started with `--simulate`
- Fixed position setting (both matrix buffers are written now)
- Patches target the specific instructions instead of shared constants (no more AI/physics side effects)
- Fixed wrong target address in `ChangeWeapon.asm` (`0x467E6` -> `0x467E60`)
- Fixed handler leak on reconnect; MFL.exe is polled once per second while detached
- Added GitHub Actions build, test and release workflows

### v0.1.0 (2017-11-20)
- Initial Release

---

## Author & Support

- **Author**: SciLor
- **Website**: [scilor.com](http://www.scilor.com/)
- **Companion Tool**: [Mashed and Mashed Fully Loaded Runner](https://github.com/mashed-reverse-engineering/MashedRunner)
- **Donate**: If you enjoy this trainer, [donations are warmly appreciated](http://www.scilor.com/donate.html)!
