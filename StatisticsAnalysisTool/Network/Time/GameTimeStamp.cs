using System;

namespace StatisticsAnalysisTool.Network.Time;

public readonly struct GameTimeStamp : IEquatable<GameTimeStamp>
{
    public long Value { get; }

    public GameTimeStamp(long value)
    {
        Value = value;
    }

    public DateTime ToDateTime()
    {
        // Albion timestamps are in ticks since some epoch
        // This is an approximation - adjust based on actual protocol
        return DateTime.UnixEpoch.AddTicks(Value);
    }

    public bool Equals(GameTimeStamp other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is GameTimeStamp other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(GameTimeStamp left, GameTimeStamp right) => left.Equals(right);
    public static bool operator !=(GameTimeStamp left, GameTimeStamp right) => !left.Equals(right);
}
