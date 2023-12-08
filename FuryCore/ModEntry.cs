namespace StardewMods.FuryCore;

using SimpleInjector;
using StardewMods.FuryCore.Framework;

/// <inheritdoc />
public sealed class ModEntry : Mod
{
#nullable disable
    private Container container;
#nullable enable

    /// <inheritdoc />
    public override void Entry(IModHelper helper) => this.container = new();

    /// <inheritdoc />
    public override object GetApi(IModInfo mod) => new FuryCoreApi();
}
