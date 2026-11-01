namespace Game.Gameplay.Commands
{
    /// <summary>Slide key (Ctrl) state; the slide lasts while it is held.</summary>
    public readonly struct SlideCommand
    {
        public SlideCommand(bool isHeld)
        {
            IsHeld = isHeld;
        }

        public bool IsHeld { get; }
    }
}
