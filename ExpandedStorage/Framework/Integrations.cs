namespace StardewMods.ExpandedStorage.Framework;

using StardewMods.Common.Integrations.BetterChests;
using StardewMods.Common.Integrations.BetterCrafting;
using StardewMods.Common.Integrations.GenericModConfigMenu;

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
    private readonly GenericModConfigMenuIntegration _genericModConfigMenu;

    private Integrations(IModRegistry modRegistry)
    {
        this._betterChests = new(modRegistry);
        this._betterCrafting = new(modRegistry);
        this._genericModConfigMenu = new(modRegistry);
    }

    /// <summary>
    ///     Gets Better Chests Integration.
    /// </summary>
    public static BetterChestsIntegration BetterChests => Integrations.Instance._betterChests;

    /// <summary>
    ///     Gets Better Crafting Integration.
    /// </summary>
    public static BetterCraftingIntegration BetterCrafting => Integrations.Instance._betterCrafting;

    /// <summary>
    ///     Gets Generic Mod Config Menu Integration.
    /// </summary>
    public static GenericModConfigMenuIntegration GenericModConfigMenu => Integrations.Instance._genericModConfigMenu;

    /// <summary>
    ///     Initializes <see cref="Integrations" />.
    /// </summary>
    /// <param name="modRegistry">SMAPI's mod registry.</param>
    /// <returns>Returns an instance of the <see cref="Integrations" /> class.</returns>
    public static Integrations Init(IModRegistry modRegistry)
    {
        return Integrations.Instance ??= new(modRegistry);
    }
}