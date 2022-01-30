namespace StardewMods.FuryCore;

using Common.Helpers;
using StardewMods.FuryCore.Services;
using StardewModdingAPI;

/// <inheritdoc />
public class ModEntry : Mod
{
    /// <summary>
    /// Gets the unique Mod Id.
    /// </summary>
    internal static string ModUniqueId { get; private set; }

    private ModServices Services { get; } = new();

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        ModEntry.ModUniqueId = this.ModManifest.UniqueID;
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