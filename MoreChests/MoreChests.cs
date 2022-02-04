namespace StardewMods.MoreChests;

using Common.Helpers;
using StardewModdingAPI;
using StardewMods.FuryCore.Services;
using StardewMods.MoreChests.Services;

/// <inheritdoc />
public class MoreChests : Mod
{
    /// <summary>
    /// Gets the unique Mod Id.
    /// </summary>
    internal static string ModUniqueId { get; private set; }

    private ModServices Services { get; } = new();

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        MoreChests.ModUniqueId = this.ModManifest.UniqueID;
        Log.Monitor = this.Monitor;

        // Services
        this.Services.Add(
            new AssetHandler(this.Services));
    }

    /// <inheritdoc />
    public override object GetApi()
    {
        return new MoreChestsApi(this.Services);
    }
}