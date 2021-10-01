namespace Common.Helpers
{
    using StardewModdingAPI;

    /// <summary>
    /// </summary>
    internal static class Translations
    {
        /// <inheritdoc cref="ITranslationHelper" />
        private static ITranslationHelper Helper { get; set; }

        /// <summary>Initializes the <see cref="Translations" /> class.</summary>
        /// <param name="helper">The instance of ITranslationHelper from the Mod.</param>
        public static void Init(ITranslationHelper helper)
        {
            Translations.Helper = helper;
        }

        public static Translation Get(string key)
        {
            return Translations.Helper.Get(key);
        }
    }
}