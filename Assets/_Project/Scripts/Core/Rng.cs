namespace Game.Core
{
    /// <summary>
    /// All gameplay randomness for one run. Each stream is derived from the run seed and its own fixed salt,
    /// so drawing more values in one system never changes results in another.
    /// </summary>
    public sealed class Rng
    {
        // Salts are the ASCII stream names. Never change or reuse them: that would change every seeded run.
        private const ulong CombatSalt = 0x434F4D424154UL;       // "COMBAT"
        private const ulong LootSalt = 0x4C4F4F54UL;             // "LOOT"
        private const ulong CardsSalt = 0x4341524453UL;          // "CARDS"
        private const ulong ShopSalt = 0x53484F50UL;             // "SHOP"
        private const ulong SkillChecksSalt = 0x534B494C4CUL;    // "SKILL"
        private const ulong SpawnsSalt = 0x535041574E53UL;       // "SPAWNS"

        public Rng(ulong seed)
        {
            Seed = seed;
            Combat = CreateStream(seed, CombatSalt);
            Loot = CreateStream(seed, LootSalt);
            Cards = CreateStream(seed, CardsSalt);
            Shop = CreateStream(seed, ShopSalt);
            SkillChecks = CreateStream(seed, SkillChecksSalt);
            Spawns = CreateStream(seed, SpawnsSalt);
        }

        public ulong Seed { get; }

        /// <summary>Crits, trinket proc chances, Albert's random spell.</summary>
        public RngStream Combat { get; }

        /// <summary>Drops, chest contents, boosts from enemies.</summary>
        public RngStream Loot { get; }

        /// <summary>Level-up cards, rerolls, talents.</summary>
        public RngStream Cards { get; }

        /// <summary>Shop offer, training options.</summary>
        public RngStream Shop { get; }

        /// <summary>d20 rolls, LCK reroll of natural 1.</summary>
        public RngStream SkillChecks { get; }

        /// <summary>Enemies, elites, skill check site placement.</summary>
        public RngStream Spawns { get; }

        private static RngStream CreateStream(ulong seed, ulong salt)
        {
            return new RngStream(SplitMix64(seed ^ salt), salt);
        }

        private static ulong SplitMix64(ulong value)
        {
            unchecked
            {
                value += 0x9E3779B97F4A7C15UL;
                value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
                value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
                return value ^ (value >> 31);
            }
        }
    }
}
