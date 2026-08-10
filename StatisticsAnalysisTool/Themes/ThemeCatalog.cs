using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Themes;

public static class ThemeCatalog
{
    public static readonly List<ThemeDefinition> All = new()
    {
        // ═══════════════════════════════════════
        // SYNTHWAVE COLLECTION
        // ═══════════════════════════════════════
        new ThemeDefinition
        {
            Name = "synthwave84",
            DisplayName = "Synthwave '84",
            Emoji = "🌆",
            Description = "The OG. Purple sunsets, chrome grids, VHS static.",
            IsDark = true,
            Background = "#FF0D0221",       // Deep purple-black
            Surface = "#FF1A0B2E",          // Dark purple
            SurfaceAlt = "#FF12081F",       // Darker purple
            Border = "#FF3D1A5C",           // Medium purple
            Foreground = "#FFE0AAFF",       // Light lavender
            ForegroundDim = "#FF7B5EA7",    // Muted purple
            Accent = "#FFFF71CE",           // Hot pink
            AccentSecondary = "#FF01CDFE",  // Cyan
            Success = "#FF05FFA1",          // Neon green
            Warning = "#FFFFFB96",          // Warm yellow
            Error = "#FFFF2E63",            // Hot red
            Info = "#FF01CDFE",             // Cyan
            Orange = "#FFFF9F1C",           // Neon orange
            Purple = "#FFB967FF",           // Bright purple
            ButtonBackground = "#FF2D1B4E",
            ButtonHover = "#FF3D2A6E",
            ButtonForeground = "#FFE0AAFF",
            InputBackground = "#FF1A0B2E",
            InputBorder = "#FF3D1A5C",
            InputForeground = "#FFE0AAFF",
            ChartLine1 = "#FFFF71CE",
            ChartLine2 = "#FF01CDFE",
            ChartLine3 = "#FF05FFA1",
            ChartLine4 = "#FFFFFB96",
            ChartLine5 = "#FFB967FF",
        },
        new ThemeDefinition
        {
            Name = "neon-nights",
            DisplayName = "Neon Nights",
            Emoji = "🌃",
            Description = "Tokyo at midnight. Cyan and magenta bleed into wet asphalt.",
            IsDark = true,
            Background = "#FF0A0A0F",
            Surface = "#FF14141F",
            SurfaceAlt = "#FF0D0D14",
            Border = "#FF2A2A3F",
            Foreground = "#FFE0E0FF",
            ForegroundDim = "#FF6A6A8F",
            Accent = "#FF00FFFF",           // Electric cyan
            AccentSecondary = "#FFFF00FF",  // Magenta
            Success = "#FF00FF88",
            Warning = "#FFFFAA00",
            Error = "#FFFF0055",
            Info = "#FF00CCFF",
            Orange = "#FFFF6600",
            Purple = "#FFAA00FF",
            ButtonBackground = "#FF1A1A2E",
            ButtonHover = "#FF2A2A4E",
            ButtonForeground = "#FFE0E0FF",
            InputBackground = "#FF14141F",
            InputBorder = "#FF2A2A3F",
            InputForeground = "#FFE0E0FF",
            ChartLine1 = "#FF00FFFF",
            ChartLine2 = "#FFFF00FF",
            ChartLine3 = "#FF00FF88",
            ChartLine4 = "#FFFFAA00",
            ChartLine5 = "#FFAA00FF",
        },
        new ThemeDefinition
        {
            Name = "outrun",
            DisplayName = "Outrun",
            Emoji = "🔥",
            Description = "Ferrari Testarossa, sunset strip, palm trees silhouetted.",
            IsDark = true,
            Background = "#FF0C0A1D",
            Surface = "#FF1B1633",
            SurfaceAlt = "#FF100E24",
            Border = "#FF3D2E5C",
            Foreground = "#FFFFE0B3",
            ForegroundDim = "#FF8F7A5E",
            Accent = "#FFFF6B35",           // Sunset orange
            AccentSecondary = "#FFFF004D",  // Deep pink
            Success = "#FF00DD88",
            Warning = "#FFFFDD00",
            Error = "#FFFF1744",
            Info = "#FF40C4FF",
            Orange = "#FFFF9100",
            Purple = "#FF7C4DFF",
            ButtonBackground = "#FF2A1F4E",
            ButtonHover = "#FF3D2E6E",
            ButtonForeground = "#FFFFE0B3",
            InputBackground = "#FF1B1633",
            InputBorder = "#FF3D2E5C",
            InputForeground = "#FFFFE0B3",
            ChartLine1 = "#FFFF6B35",
            ChartLine2 = "#FFFF004D",
            ChartLine3 = "#FF00DD88",
            ChartLine4 = "#FFFFDD00",
            ChartLine5 = "#FF7C4DFF",
        },
        new ThemeDefinition
        {
            Name = "vaporwave",
            DisplayName = "Vaporwave",
            Emoji = "💜",
            Description = "A E S T H E T I C. Marble busts, Windows 95, mall soft.",
            IsDark = true,
            Background = "#FF1A0A2E",
            Surface = "#FF2D1B4E",
            SurfaceAlt = "#FF1F1038",
            Border = "#FF4E2E7A",
            Foreground = "#FFFFB3D9",
            ForegroundDim = "#FF9E6EBF",
            Accent = "#FFFF71CE",           // Pastel pink
            AccentSecondary = "#FF01CDFE",  // Vapor teal
            Success = "#FF7FFFB0",
            Warning = "#FFFFFF99",
            Error = "#FFFF6B9D",
            Info = "#FF00E5FF",
            Orange = "#FFFFAB91",
            Purple = "#FFCE93D8",
            ButtonBackground = "#FF3D2566",
            ButtonHover = "#FF4E3580",
            ButtonForeground = "#FFFFB3D9",
            InputBackground = "#FF2D1B4E",
            InputBorder = "#FF4E2E7A",
            InputForeground = "#FFFFB3D9",
            ChartLine1 = "#FFFF71CE",
            ChartLine2 = "#FF01CDFE",
            ChartLine3 = "#FF7FFFB0",
            ChartLine4 = "#FFFFFF99",
            ChartLine5 = "#FFCE93D8",
        },

        // ═══════════════════════════════════════
        // ALBION COLLECTION
        // ═══════════════════════════════════════
        new ThemeDefinition
        {
            Name = "caerleon",
            DisplayName = "Caerleon",
            Emoji = "⚔️",
            Description = "The Outlaw City. Blood and iron, red and black.",
            IsDark = true,
            Background = "#FF0D0A0A",
            Surface = "#FF1A1212",
            SurfaceAlt = "#FF120D0D",
            Border = "#FF3D1F1F",
            Foreground = "#FFE8D0D0",
            ForegroundDim = "#FF8F6B6B",
            Accent = "#FFDC143C",           // Crimson
            AccentSecondary = "#FFFF6347",  // Tomato red
            Success = "#FF228B22",
            Warning = "#FFDAA520",
            Error = "#FFFF0000",
            Info = "#FF4682B4",
            Orange = "#FFFF4500",
            Purple = "#FF8B008B",
            ButtonBackground = "#FF2A1515",
            ButtonHover = "#FF3D2020",
            ButtonForeground = "#FFE8D0D0",
            InputBackground = "#FF1A1212",
            InputBorder = "#FF3D1F1F",
            InputForeground = "#FFE8D0D0",
            ChartLine1 = "#FFDC143C",
            ChartLine2 = "#FFFF6347",
            ChartLine3 = "#FF228B22",
            ChartLine4 = "#FFDAA520",
            ChartLine5 = "#FF8B008B",
        },
        new ThemeDefinition
        {
            Name = "lymhurst",
            DisplayName = "Lymhurst",
            Emoji = "🌿",
            Description = "Deep forest. Ancient trees, moss, and druid circles.",
            IsDark = true,
            Background = "#FF0A120A",
            Surface = "#FF121F12",
            SurfaceAlt = "#FF0D170D",
            Border = "#FF1F3D1F",
            Foreground = "#FFD0E8D0",
            ForegroundDim = "#FF6B8F6B",
            Accent = "#FF32CD32",           // Lime green
            AccentSecondary = "#FF00FA9A",  // Spring green
            Success = "#FF00FF7F",
            Warning = "#FFADFF2F",
            Error = "#FFFF4444",
            Info = "#FF40E0D0",
            Orange = "#FFFF8C00",
            Purple = "#FF9370DB",
            ButtonBackground = "#FF1A2E1A",
            ButtonHover = "#FF254025",
            ButtonForeground = "#FFD0E8D0",
            InputBackground = "#FF121F12",
            InputBorder = "#FF1F3D1F",
            InputForeground = "#FFD0E8D0",
            ChartLine1 = "#FF32CD32",
            ChartLine2 = "#FF00FA9A",
            ChartLine3 = "#FF00FF7F",
            ChartLine4 = "#FFADFF2F",
            ChartLine5 = "#FF9370DB",
        },
        new ThemeDefinition
        {
            Name = "fort-sterling",
            DisplayName = "Fort Sterling",
            Emoji = "🏔️",
            Description = "Frozen peaks. Ice, steel, and mountain winds.",
            IsDark = true,
            Background = "#FF0A0F14",
            Surface = "#FF121A21",
            SurfaceAlt = "#FF0D1319",
            Border = "#FF1F2E3D",
            Foreground = "#FFD0E0F0",
            ForegroundDim = "#FF6B7F8F",
            Accent = "#FF00BFFF",           // Deep sky blue
            AccentSecondary = "#FF87CEEB",  // Sky blue
            Success = "#FF40E0D0",
            Warning = "#FFF0E68C",
            Error = "#FFDC143C",
            Info = "#FF00CED1",
            Orange = "#FFFFA07A",
            Purple = "#FF9370DB",
            ButtonBackground = "#FF152028",
            ButtonHover = "#FF20303D",
            ButtonForeground = "#FFD0E0F0",
            InputBackground = "#FF121A21",
            InputBorder = "#FF1F2E3D",
            InputForeground = "#FFD0E0F0",
            ChartLine1 = "#FF00BFFF",
            ChartLine2 = "#FF87CEEB",
            ChartLine3 = "#FF40E0D0",
            ChartLine4 = "#FFF0E68C",
            ChartLine5 = "#FF9370DB",
        },
        new ThemeDefinition
        {
            Name = "bridgewatch",
            DisplayName = "Bridgewatch",
            Emoji = "🏜️",
            Description = "Desert sands. Golden dunes and scorching sun.",
            IsDark = true,
            Background = "#FF14100A",
            Surface = "#FF211A12",
            SurfaceAlt = "#FF181410",
            Border = "#FF3D3020",
            Foreground = "#FFF0E0C8",
            ForegroundDim = "#FF8F7F60",
            Accent = "#FFDAA520",           // Goldenrod
            AccentSecondary = "#FFFF8C00",  // Dark orange
            Success = "#FF6B8E23",
            Warning = "#FFFFD700",
            Error = "#FFDC143C",
            Info = "#FF5F9EA0",
            Orange = "#FFFF7F50",
            Purple = "#FFBA55D3",
            ButtonBackground = "#FF2A2015",
            ButtonHover = "#FF3D3020",
            ButtonForeground = "#FFF0E0C8",
            InputBackground = "#FF211A12",
            InputBorder = "#FF3D3020",
            InputForeground = "#FFF0E0C8",
            ChartLine1 = "#FFDAA520",
            ChartLine2 = "#FFFF8C00",
            ChartLine3 = "#FF6B8E23",
            ChartLine4 = "#FFFFD700",
            ChartLine5 = "#FFBA55D3",
        },
        new ThemeDefinition
        {
            Name = "martlock",
            DisplayName = "Martlock",
            Emoji = "💀",
            Description = "Highland city. Blue steel and mountain frost.",
            IsDark = true,
            Background = "#FF0A1214",
            Surface = "#FF121F21",
            SurfaceAlt = "#FF0D1719",
            Border = "#FF1F3D3A",
            Foreground = "#FFC8E8E0",
            ForegroundDim = "#FF608880",
            Accent = "#FF20B2AA",           // Light sea green
            AccentSecondary = "#FF40E0D0",  // Turquoise
            Success = "#FF00FA9A",
            Warning = "#FFF0E68C",
            Error = "#FFFF4466",
            Info = "#FF48D1CC",
            Orange = "#FFFFA07A",
            Purple = "#FF9370DB",
            ButtonBackground = "#FF152A28",
            ButtonHover = "#FF203D3A",
            ButtonForeground = "#FFC8E8E0",
            InputBackground = "#FF121F21",
            InputBorder = "#FF1F3D3A",
            InputForeground = "#FFC8E8E0",
            ChartLine1 = "#FF20B2AA",
            ChartLine2 = "#FF40E0D0",
            ChartLine3 = "#FF00FA9A",
            ChartLine4 = "#FFF0E68C",
            ChartLine5 = "#FF9370DB",
        },
        new ThemeDefinition
        {
            Name = "thetford",
            DisplayName = "Thetford",
            Emoji = "🌊",
            Description = "Swamp waters. Purple depths and hidden dangers.",
            IsDark = true,
            Background = "#FF0E0A14",
            Surface = "#FF181226",
            SurfaceAlt = "#FF120D1A",
            Border = "#FF2A1F40",
            Foreground = "#FFD8D0E8",
            ForegroundDim = "#FF706088",
            Accent = "#FF9932CC",           // Dark orchid
            AccentSecondary = "#FFBA55D3",  // Medium orchid
            Success = "#FF20B2AA",
            Warning = "#FFDDA0DD",
            Error = "#FFFF0040",
            Info = "#FF778899",
            Orange = "#FFFF6347",
            Purple = "#FFDA70D6",
            ButtonBackground = "#FF201535",
            ButtonHover = "#FF30204A",
            ButtonForeground = "#FFD8D0E8",
            InputBackground = "#FF181226",
            InputBorder = "#FF2A1F40",
            InputForeground = "#FFD8D0E8",
            ChartLine1 = "#FF9932CC",
            ChartLine2 = "#FFBA55D3",
            ChartLine3 = "#FF20B2AA",
            ChartLine4 = "#FFDDA0DD",
            ChartLine5 = "#FFDA70D6",
        },
        new ThemeDefinition
        {
            Name = "royal",
            DisplayName = "Royal",
            Emoji = "👑",
            Description = "The Crown of Albion. Gold, navy, and imperial majesty.",
            IsDark = true,
            Background = "#FF0A0C18",
            Surface = "#FF141830",
            SurfaceAlt = "#FF0E1220",
            Border = "#FF28305C",
            Foreground = "#FFE8E0C8",
            ForegroundDim = "#FF787060",
            Accent = "#FFFFD700",           // Gold
            AccentSecondary = "#FF4169E1",  // Royal blue
            Success = "#FF32CD32",
            Warning = "#FFFFA500",
            Error = "#FFDC143C",
            Info = "#FF6495ED",
            Orange = "#FFFF7F50",
            Purple = "#FF9370DB",
            ButtonBackground = "#FF1A2040",
            ButtonHover = "#FF28305C",
            ButtonForeground = "#FFE8E0C8",
            InputBackground = "#FF141830",
            InputBorder = "#FF28305C",
            InputForeground = "#FFE8E0C8",
            ChartLine1 = "#FFFFD700",
            ChartLine2 = "#FF4169E1",
            ChartLine3 = "#FF32CD32",
            ChartLine4 = "#FFFFA500",
            ChartLine5 = "#FF9370DB",
        },

        // ═══════════════════════════════════════
        // CLASSIC
        // ═══════════════════════════════════════
        new ThemeDefinition
        {
            Name = "dark",
            DisplayName = "Dark (Mocha)",
            Emoji = "🌑",
            Description = "Catppuccin Mocha. The classic dark theme.",
            IsDark = true,
            // Uses all defaults from ThemeDefinition
        },
        new ThemeDefinition
        {
            Name = "light",
            DisplayName = "Light",
            Emoji = "☀️",
            Description = "Clean and bright. For daylight warriors.",
            IsDark = false,
            Background = "#FFF5F5F5",
            Surface = "#FFFFFFFF",
            SurfaceAlt = "#FFE8E8E8",
            Border = "#FFD0D0D0",
            Foreground = "#FF1A1A2E",
            ForegroundDim = "#FF6C7086",
            Accent = "#FF8839EF",
            AccentSecondary = "#FF1E66F5",
            Success = "#FF40A02B",
            Warning = "#FFDF8E1D",
            Error = "#FFD20F39",
            Info = "#FF04A5E5",
            Orange = "#FFFE640B",
            Purple = "#FF8839EF",
            ButtonBackground = "#FFE0E0E0",
            ButtonHover = "#FFD0D0D0",
            ButtonForeground = "#FF1A1A2E",
            InputBackground = "#FFFFFFFF",
            InputBorder = "#FFD0D0D0",
            InputForeground = "#FF1A1A2E",
            ChartLine1 = "#FF8839EF",
            ChartLine2 = "#FF1E66F5",
            ChartLine3 = "#FF40A02B",
            ChartLine4 = "#FFDF8E1D",
            ChartLine5 = "#FFD20F39",
        },
    };

    public static ThemeDefinition GetByName(string name)
    {
        return All.FirstOrDefault(t => t.Name == name) ?? All.First(t => t.Name == "synthwave84");
    }

    public static ThemeDefinition Default => GetByName("synthwave84");

    public static List<string> Names => All.Select(t => t.FullDisplayName).ToList();
}
