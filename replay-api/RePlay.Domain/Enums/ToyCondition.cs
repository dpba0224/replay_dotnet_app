namespace RePlay.Domain.Enums;

/// <summary>
/// 5-tier condition rating system for toys
/// </summary>
public enum ToyCondition
{
    /// <summary>Heavy wear, may have minor damage. Still functional for play.</summary>
    Acceptable = 1,

    /// <summary>Noticeable wear, minor scratches or scuffs. Fully functional.</summary>
    Fair = 2,

    /// <summary>Normal use. Minor cosmetic wear but fully functional.</summary>
    Good = 3,

    /// <summary>Very light use. No visible wear or damage.</summary>
    Excellent = 4,

    /// <summary>Unused or barely used. Original packaging may be included.</summary>
    Mint = 5
}
