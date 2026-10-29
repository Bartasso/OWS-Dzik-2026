namespace Game.Core
{
    public readonly struct ValidationError
    {
        public ValidationError(string source, string message, object context)
        {
            Source = source;
            Message = message;
            Context = context;
        }

        /// <summary>Name of the asset or system that failed, e.g. "GameBalance".</summary>
        public string Source { get; }

        public string Message { get; }

        /// <summary>The object to highlight in the editor (a Unity asset), or null.</summary>
        public object Context { get; }

        public override string ToString()
        {
            return $"{Source}: {Message}";
        }
    }
}
