namespace Common.Helpers
{
    using StardewModdingAPI.Events;

    /// <summary>
    ///     Provides mod events across mods.
    /// </summary>
    internal static class Events
    {
        public static IDisplayEvents Display
        {
            get => Events.ModEvents.Display;
        }

        public static IInputEvents Input
        {
            get => Events.ModEvents.Input;
        }

        public static IMultiplayerEvents Multiplayer
        {
            get => Events.ModEvents.Multiplayer;
        }

        public static IPlayerEvents Player
        {
            get => Events.ModEvents.Player;
        }

        public static ISpecializedEvents Specialized
        {
            get => Events.ModEvents.Specialized;
        }

        public static IWorldEvents World
        {
            get => Events.ModEvents.World;
        }

        public static IGameLoopEvents GameLoop
        {
            get => Events.ModEvents.GameLoop;
        }

        /// <inheritdoc cref="IModEvents" />
        private static IModEvents ModEvents { get; set; }

        /// <summary>Initializes the <see cref="Events" /> class.</summary>
        /// <param name="events">The instance of IModEvents from the Mod.</param>
        public static void Init(IModEvents events)
        {
            Events.ModEvents = events;
        }
    }
}