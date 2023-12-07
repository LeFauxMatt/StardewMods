namespace StardewMods.FuryCore;

using StardewMods.FuryCore.Framework;

/// <inheritdoc />
public sealed class ModEntry : Mod
{
    /// <inheritdoc/>
    public override void Entry(IModHelper helper)
    {
    }

    /// <inheritdoc/>
    public override object GetApi(IModInfo mod) => new FuryCoreApi();
}
