namespace Game.Gameplay.Combat
{
    public readonly struct HitResult
    {
        public HitResult(float damage, bool isCrit)
        {
            Damage = damage;
            IsCrit = isCrit;
        }

        public float Damage { get; }

        public bool IsCrit { get; }
    }
}
