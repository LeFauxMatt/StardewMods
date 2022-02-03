namespace StardewMods.FuryCore;

using Common.Helpers;
using StardewModdingAPI;
using StardewMods.FuryCore.Services;

/// <inheritdoc />
public class FuryCore : Mod
{
    /// <summary>
    ///     Gets the unique Mod Id.
    /// </summary>
    internal static string ModUniqueId { get; private set; }

    private ModServices Services { get; } = new();

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        FuryCore.ModUniqueId = this.ModManifest.UniqueID;
        Log.Monitor = this.Monitor;
        this.Services.Add(
            new MenuComponents(this.Helper, this.Services),
            new CustomEvents(this.Helper, this.Services),
            new HarmonyHelper(),
            new MenuItems(this.Helper.Events, this.Services));
    }

    /// <inheritdoc />
    public override object GetApi()
    {
        return new FuryCoreApi(this.Services);
    }
}