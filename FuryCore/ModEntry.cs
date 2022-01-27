namespace FuryCore;

using Common.Helpers;
using FuryCore.Interfaces;
using FuryCore.Services;
using StardewModdingAPI;

/// <inheritdoc />
public class ModEntry : Mod
{
    /// <summary>
    /// Gets the unique Mod Id.
    /// </summary>
    internal static string ModUniqueId { get; private set; }

    private ServiceCollection Services { get; } = new();

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        ModEntry.ModUniqueId = this.ModManifest.UniqueID;
        Log.Monitor = this.Monitor;

        this.Services.AddRange(
            new IService[]
            {
                new MenuComponents(this.Helper, this.Services), new CustomEvents(this.Helper, this.Services), new HarmonyHelper(), new MenuItems(this.Helper.Events, this.Services),
            });

        this.Services.ForceEvaluation();
    }

    /// <inheritdoc />
    public override object GetApi()
    {
        return new FuryCoreApi(this.Services);
    }
}