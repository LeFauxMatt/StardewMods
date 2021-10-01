namespace Common.Helpers
{
    using StardewModdingAPI;

    internal static class Content
    {
        /// <inheritdoc cref="IMonitor" />
        private static IContentHelper Helper { get; set; } = null!;

        /// <summary>Initializes the <see cref="Content" /> class.</summary>
        /// <param name="contentHelper">The instance of IContentHelper from the Mod.</param>
        public static void Init(IContentHelper contentHelper)
        {
            Content.Helper = contentHelper;
        }

        public static T FromMod<T>(string key)
        {
            return Content.Helper.Load<T>(key);
        }

        public static T FromGame<T>(string key)
        {
            return Content.Helper.Load<T>(key, ContentSource.GameContent);
        }
    }
}