namespace MoreChests.Models
{
    using StardewModdingAPI;

    internal record ContentPackFor : IManifestContentPackFor
    {
        public string UniqueID { get; set; }

        public ISemanticVersion MinimumVersion { get; set; }
    }
}