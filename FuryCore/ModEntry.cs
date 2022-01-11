namespace FuryCore;

using Common.Helpers;
using FuryCore.Interfaces;
using FuryCore.Services;
using StardewModdingAPI;

/// <inheritdoc />
public class ModEntry : Mod
{
    private ServiceCollection Services { get; } = new();

    /// <inheritdoc/>
    public override void Entry(IModHelper helper)
    {
        Log.Init(this.Monitor);

        this.Services.AddRange(
            new IService[]
            {
                new CustomMenuComponents(this.Helper, this.Services),
                new CustomEvents(this.Helper, this.Services),
                new HarmonyHelper(this.ModManifest),
            });

        this.Services.ForceEvaluation();
    }

    /// <inheritdoc />
    public override object GetApi()
    {
        return new FuryCoreApi(this.Services);
    }
}