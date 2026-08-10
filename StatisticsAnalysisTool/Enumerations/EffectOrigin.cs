namespace StatisticsAnalysisTool.Enumerations;

public enum EffectOrigin : byte
{
    None = 0,
    Spell = 1,
    Attack = 2,
    Item = 3,
    Consumable = 4,
    Equipment = 5,
    Mount = 6,
    Pet = 7,
    Buff = 8,
    Debuff = 9,
    Environment = 10,
    Zone = 11,
    Global = 12,
    Unknown = 255
}
