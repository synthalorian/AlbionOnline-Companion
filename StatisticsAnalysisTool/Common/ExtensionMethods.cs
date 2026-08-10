using System;

namespace StatisticsAnalysisTool.Common;

public static class ExtensionMethods
{
    public static long? ObjectToLong(this object? obj)
    {
        if (obj == null) return null;
        try { return Convert.ToInt64(obj); } catch { return null; }
    }

    public static double ObjectToDouble(this object? obj)
    {
        if (obj == null) return 0;
        try { return Convert.ToDouble(obj); } catch { return 0; }
    }

    public static short ObjectToShort(this object? obj)
    {
        if (obj == null) return 0;
        try { return Convert.ToInt16(obj); } catch { return 0; }
    }

    public static int ObjectToInt(this object? obj)
    {
        if (obj == null) return 0;
        try { return Convert.ToInt32(obj); } catch { return 0; }
    }

    public static bool ObjectToBool(this object? obj)
    {
        if (obj == null) return false;
        try { return Convert.ToBoolean(obj); } catch { return false; }
    }

    public static string ObjectToString(this object? obj)
    {
        return obj?.ToString() ?? string.Empty;
    }
}
