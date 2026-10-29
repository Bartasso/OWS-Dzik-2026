namespace Game.Core
{
    /// <summary>How an effect grows with trinket copies. See StatFormulas.StackValue.</summary>
    public enum StackCurve
    {
        /// <summary>value × stacks</summary>
        Linear = 0,

        /// <summary>1 − 1 / (1 + value × stacks): diminishing returns, never reaches 1.</summary>
        Hyperbolic = 1,
    }
}
