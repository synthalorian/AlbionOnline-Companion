# ⚔️ Albion Online Companion

A **cross-platform** companion app for [Albion Online](https://albiononline.com/) — built with [Avalonia UI](https://avaloniaui.net/) for **Linux, Windows, and macOS**.

Inspired by [Triky313's AlbionOnline-StatisticsAnalysis](https://github.com/Triky313/AlbionOnline-StatisticsAnalysis) (Windows-only WPF), rebuilt from scratch as a true cross-platform companion.

## Features

### 📊 Dashboard
- Real-time fame, silver, might, favor tracking
- Per-hour rates with session statistics
- Kill/death tracking
- Activity charts

### ⚔️ Damage Meter
- Real-time DPS/HPS tracking
- Party damage breakdown with rankings
- Damage snapshots for comparison
- Sort by damage, DPS, healing, HPS

### 🏰 Dungeon Tracker
- Dungeon entry/exit detection
- Close timer countdown
- Fame and silver per dungeon
- Session and daily statistics

### 💰 Loot Logger
- Real-time loot tracking
- Estimated market values
- Loot value per hour
- Loot comparator

### 🔨 Crafting Calculator
- Real-time market prices (Albion Online Data Project API)
- Profit calculation per craft
- Resource cost breakdown
- City-specific pricing

### 🎨 13 Themes
- **Synthwave Collection:** Synthwave '84, Neon Nights, Outrun, Vaporwave
- **Albion Collection:** Caerleon, Lymhurst, Fort Sterling, Bridgewatch, Martlock, Thetford, Royal
- **Classic:** Dark (Mocha), Light
- Live theme switching with color preview

### 🔍 Player Info
- Player search and statistics
- Equipment inspection

## Platforms

| Platform | Status | Binary |
|----------|--------|--------|
| **Linux** (x64) | ✅ Supported | `AlbionCompanion` |
| **Windows** (x64) | ✅ Supported | `AlbionCompanion.exe` |
| **macOS** (x64) | ✅ Supported | `AlbionCompanion` |
| **macOS** (ARM) | ✅ Supported | `AlbionCompanion` |

## Installation

### From Source

```bash
# Clone
git clone https://github.com/synthalorian/AlbionOnline-StatisticsTool-Linux.git
cd AlbionOnline-StatisticsTool-Linux

# Build
dotnet build -c Release

# Run
dotnet run --project StatisticsAnalysisTool

# Or publish for your platform
dotnet publish StatisticsAnalysisTool -r linux-x64 -c Release    # Linux
dotnet publish StatisticsAnalysisTool -r win-x64 -c Release      # Windows
dotnet publish StatisticsAnalysisTool -r osx-arm64 -c Release    # macOS Apple Silicon
```

### Requirements
- **.NET 10.0 Runtime** or later
- **Root/CAP_NET_RAW** for packet capture (Linux)
- **Administrator** for packet capture (Windows)

### Packet Capture Permissions

**Linux:**
```bash
sudo setcap cap_net_raw+ep $(which dotnet)
# Or run with sudo
sudo ./AlbionCompanion
```

**Windows:**
Run as Administrator, or install [Npcap](https://npcap.com/).

## Architecture

```
StatisticsAnalysisTool/              # Avalonia UI (cross-platform)
├── Views/                           # AXAML views (7 views)
├── ViewModels/                      # MVVM ViewModels
├── Themes/                          # 13-theme system
├── Network/                         # Packet capture + event handlers
│   ├── Events/                      # Game event classes
│   ├── Handlers/                    # Event → ViewModel handlers
│   └── PacketProviders/             # Raw socket capture
├── Common/                          # Services (settings, items, API)
└── Models/                          # Data models

StatisticsAnalysisTool.Network/      # Photon protocol parser
StatisticsAnalysisTool.PhotonPackageParser/  # Packet parsing
StatisticsAnalysisTool.Protocol18/           # Protocol definitions
StatisticsAnalysisTool.Abstractions/         # Interfaces
StatisticsAnalysisTool.Diagnostics/          # Debug tools
StatisticAnalysisTool.Extractor/             # Game data extraction
```

## Data Sources

- **Packet Capture:** Raw sockets (Linux) / Npcap (Windows) — reads Albion's Photon protocol
- **Market Prices:** [Albion Online Data Project](https://www.albion-online-data.com/) API
- **Item Database:** [ao-bin-dumps](https://github.com/ao-data/ao-bin-dumps) JSON

## Original Project

This is a cross-platform port of [Triky313's AlbionOnline-StatisticsAnalysis](https://github.com/Triky313/AlbionOnline-StatisticsAnalysis). All credit for the original design, protocol reverse engineering, and game logic goes to the original authors.

## License

See [LICENSE](LICENSE) for details.

---

*"Stay retro, stay futuristic."* 🎹🦞
