namespace StardewMods.FuryCore.Framework;

using SimpleInjector;
using StardewMods.Common.Integrations.FuryCore;

/// <inheritdoc />
public sealed class FuryCoreApi : IFuryCoreApi
{
    /// <inheritdoc/>
    public Container Container => new();
}
