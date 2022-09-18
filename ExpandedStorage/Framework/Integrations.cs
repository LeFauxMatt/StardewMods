namespace StardewMods.ExpandedStorage.Framework;

using StardewMods.Common.Integrations.BetterChests;
using StardewMods.Common.Integrations.BetterCrafting;

/// <summary>
///     Handles integrations with other mods.
/// </summary>
internal sealed class Integrations
{
#nullable disable
    private static Integrations Instance;
#nullable enable

    private readonly BetterChestsIntegration _betterChests;
    private readonly BetterCraftingIntegration _betterCrafting;

    private Integrations(IModRegistry modRegistry)
    {
        this._betterChests = new(modRegistry);
        this._betterCrafting = new(modRegistry);
    }

    /// <summary>
    ///     Gets Better Chests Integration.
    /// </summary>
    public static BetterChestsIntegration BetterChests => Integrations.Instance._betterChests;

    /// <summary>
    ///     Gets Better Crafting Integration.
    /// </summary>
    public static BetterCraftingIntegration BetterCrafting => Integrations.Instance._betterCrafting;

    public static Integrations Init(IModRegistry modRegistry)
    {
        return Integrations.Instance ??= new(modRegistry);
    }
}