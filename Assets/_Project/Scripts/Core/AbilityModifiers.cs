using System;

namespace Game.Core
{
    /// <summary>
    /// Ability values stored directly as modifiers (start at 0, may be negative, no cap).
    /// </summary>
    public struct AbilityModifiers
    {
        public const int Count = 7;

        private int _str;
        private int _dex;
        private int _con;
        private int _int;
        private int _wis;
        private int _cha;
        private int _lck;

        public int this[Ability ability]
        {
            get => ability switch
            {
                Ability.Str => _str,
                Ability.Dex => _dex,
                Ability.Con => _con,
                Ability.Int => _int,
                Ability.Wis => _wis,
                Ability.Cha => _cha,
                Ability.Lck => _lck,
                _ => throw new ArgumentOutOfRangeException(nameof(ability), ability, null),
            };
            set
            {
                switch (ability)
                {
                    case Ability.Str: _str = value; break;
                    case Ability.Dex: _dex = value; break;
                    case Ability.Con: _con = value; break;
                    case Ability.Int: _int = value; break;
                    case Ability.Wis: _wis = value; break;
                    case Ability.Cha: _cha = value; break;
                    case Ability.Lck: _lck = value; break;
                    default: throw new ArgumentOutOfRangeException(nameof(ability), ability, null);
                }
            }
        }
    }
}
