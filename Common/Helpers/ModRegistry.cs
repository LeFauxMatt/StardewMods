namespace Common.Helpers
{
    using StardewModdingAPI;

    internal static class ModRegistry
    {
        /// <inheritdoc cref="IModRegistry" />
        private static IModRegistry Registry { get; set; }

        /// <summary>Initializes the <see cref="ModRegistry" /> class.</summary>
        /// <param name="registry">The instance of ModRegistry from the Mod.</param>
        public static void Init(IModRegistry registry)
        {
            ModRegistry.Registry = registry;
        }

        public static bool IsLoaded(string uniqueId)
        {
            return ModRegistry.Registry.IsLoaded(uniqueId);
        }
    }
}