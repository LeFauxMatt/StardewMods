namespace Common.Helpers
{
    using StardewModdingAPI;

    /// <summary>
    /// </summary>
    internal static class Locale
    {
        /// <inheritdoc cref="ITranslationHelper" />
        private static ITranslationHelper Helper { get; set; } = null!;

        /// <summary>Initializes the <see cref="Locale" /> class.</summary>
        /// <param name="helper">The instance of ITranslationHelper from the Mod.</param>
        public static void Init(ITranslationHelper helper)
        {
            Locale.Helper = helper;
        }

        public static Translation Get(string key)
        {
            return Locale.Helper.Get(key);
        }
    }
}