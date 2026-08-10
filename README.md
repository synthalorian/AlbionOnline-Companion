# Statistics Analysis Tool - Linux

A Linux-native port of the [Albion Online Statistics Analysis Tool](https://github.com/Triky313/AlbionOnline-StatisticsAnalysis) — rebuilt with [Avalonia UI](https://avaloniaui.net/) for cross-platform compatibility.

## Features

- **Damage Meter** — Real-time DPS/HPS tracking with party breakdown
- **Dungeon Tracker** — Entry timers, chest tracking, fame per hour
- **Loot Logger** — Complete loot history with filtering and search
- **Crafting Calculator** — Profit analysis for crafting recipes
- **Map History** — Zone visit tracking
- **Player Info** — Player search and statistics
- **Guild Management** — Guild member tracking

## Requirements

- **Linux** (any modern distribution)
- **.NET 10.0 Runtime** or later
- **Root/CAP_NET_RAW** for packet capture (or run with `sudo`)

## Installation

### From Source

```bash
# Clone the repository
git clone https://github.com/synthalorian/AlbionOnline-StatisticsTool-Linux.git
cd AlbionOnline-StatisticsTool-Linux

# Build
dotnet build -c Release

# Run (requires root for packet capture)
sudo dotnet run --project StatisticsAnalysisTool
```

### Pre-built Binary

Download the latest release from the [Releases](https://github.com/synthalorian/AlbionOnline-StatisticsTool-Linux/releases) page.

```bash
chmod +x StatisticsAnalysisTool
sudo ./StatisticsAnalysisTool
```

## Why Linux?

The original tool is built on WPF (Windows Presentation Foundation), which is Windows-only. This port uses Avalonia UI — a cross-platform XAML framework — to bring the same great features to Linux gamers.

## Architecture

```
StatisticsAnalysisTool/          # Avalonia UI (Linux-native)
├── Views/                       # XAML views
├── ViewModels/                  # MVVM ViewModels
├── Models/                      # Data models
├── Network/                     # Linux packet capture (raw sockets)
└── Common/                      # Converters, helpers

StatisticsAnalysisTool.Network/  # Photon protocol parser (shared)
StatisticsAnalysisTool.PhotonPackageParser/  # Photon package parsing (shared)
StatisticsAnalysisTool.Protocol18/           # Protocol definitions (shared)
StatisticsAnalysisTool.Abstractions/         # Interfaces (shared)
StatisticsAnalysisTool.Diagnostics/          # Debug console (shared)
StatisticAnalysisTool.Extractor/             # Game data extraction (shared)
```

## Packet Capture

This tool uses raw sockets on Linux (no Npcap required). You need either:
- Run as root: `sudo ./StatisticsAnalysisTool`
- Or grant CAP_NET_RAW: `sudo setcap cap_net_raw+ep ./StatisticsAnalysisTool`

## Original Project

This is a port of [Triky313's AlbionOnline-StatisticsAnalysis](https://github.com/Triky313/AlbionOnline-StatisticsAnalysis). All credit for the original design, protocol reverse engineering, and game logic goes to the original authors.

## License

Same license as the original project. See [LICENSE](LICENSE) for details.

## Contributing

Contributions welcome! Please open an issue or PR on GitHub.

---

*"Stay retro, stay futuristic."* 🎹🦞
