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
    private static Integrations instance;
#nullable enable

    private readonly BetterChestsIntegration betterChests;
    private readonly BetterCraftingIntegration betterCrafting;
    private readonly GenericModConfigMenuIntegration genericModConfigMenu;

    private Integrations(IModRegistry modRegistry)
    {
        this.betterChests = new(modRegistry);
        this.betterCrafting = new(modRegistry);
        this.genericModConfigMenu = new(modRegistry);
    }

    /// <summary>
    ///     Gets Better Chests Integration.
    /// </summary>
    public static BetterChestsIntegration BetterChests => Integrations.instance.betterChests;

    /// <summary>
    ///     Gets Better Crafting Integration.
    /// </summary>
    public static BetterCraftingIntegration BetterCrafting => Integrations.instance.betterCrafting;

    /// <summary>
    ///     Gets Generic Mod Config Menu Integration.
    /// </summary>
    public static GenericModConfigMenuIntegration GenericModConfigMenu => Integrations.instance.genericModConfigMenu;

    /// <summary>
    ///     Initializes <see cref="Integrations" />.
    /// </summary>
    /// <param name="modRegistry">SMAPI's mod registry.</param>
    /// <returns>Returns an instance of the <see cref="Integrations" /> class.</returns>
    public static Integrations Init(IModRegistry modRegistry)
    {
        return Integrations.instance ??= new(modRegistry);
    }
}