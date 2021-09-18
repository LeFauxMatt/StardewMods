namespace Common.Integrations
{
    using StardewModdingAPI;

    /// <summary>Provides an integration point for using external mods' APIs.</summary>
    /// <typeparam name="T">Interface for the external mod's API.</typeparam>
    internal abstract class ModIntegration<T>
        where T : class
    {
        private readonly IModRegistry ModRegistry;
        private readonly string ModUniqueId;
        private T ModAPI;

        /// <summary>Initializes a new instance of the <see cref="ModIntegration{T}"/> class.</summary>
        /// <param name="modRegistry">SMAPI's mod registry.</param>
        /// <param name="modUniqueId">The unique id of the external mod.</param>
        internal ModIntegration(IModRegistry modRegistry, string modUniqueId)
        {
            this.ModRegistry = modRegistry;
            this.ModUniqueId = modUniqueId;
        }

        /// <summary>Gets the Mod's API through SMAPI's standard interface.</summary>
        protected internal T API
        {
            get => this.ModAPI ??= this.ModRegistry.GetApi<T>(this.ModUniqueId);
        }

        /// <summary>Gets the loaded status of the mod.</summary>
        protected internal bool IsLoaded
        {
            get => this.ModRegistry.IsLoaded(this.ModUniqueId);
        }
    }
}