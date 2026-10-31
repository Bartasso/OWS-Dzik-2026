namespace Game.Gameplay.Commands
{
    public readonly struct FireCommand
    {
        public FireCommand(bool isHeld)
        {
            IsHeld = isHeld;
        }

        public bool IsHeld { get; }
    }
}
