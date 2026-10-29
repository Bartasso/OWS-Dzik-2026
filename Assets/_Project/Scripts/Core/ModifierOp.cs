namespace Game.Core
{
    /// <summary>
    /// Final = (base + Σ Flat) × (1 + Σ PercentAdd) × Π (1 + PercentMult), then clamped.
    /// Percent values are fractions: 0.25 means +25%.
    /// </summary>
    public enum ModifierOp
    {
        Flat = 0,
        PercentAdd = 1,
        PercentMult = 2,
    }
}
