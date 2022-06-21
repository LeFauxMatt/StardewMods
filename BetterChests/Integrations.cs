namespace StardewMods.BetterChests;

using StardewModdingAPI;

/// <summary>
///     Handles integrations with other mods.
/// </summary>
internal class Integrations
{
    private readonly AutomateIntegration _automate;
    private readonly BetterCraftingIntegration _betterCrafting;
    private readonly ToolbarIconsIntegration _toolbarIcons;

    private Integrations(IModHelper helper)
    {
        this._automate = new(helper.ModRegistry);
        this._betterCrafting = new(helper.ModRegistry);
        this._toolbarIcons = new(helper.ModRegistry);
    }

    /// <summary>
    ///     Gets Automate integration.
    /// </summary>
    public static AutomateIntegration Automate
    {
        get => Integrations.Instance!._automate;
    }

    /// <summary>
    ///     Gets Better Craft integration.
    /// </summary>
    public static BetterCraftingIntegration BetterCrafting
    {
        get => Integrations.Instance!._betterCrafting;
    }

    /// <summary>
    ///     Gets Toolbar Icons integration.
    /// </summary>
    public static ToolbarIconsIntegration ToolbarIcons
    {
        get => Integrations.Instance!._toolbarIcons;
    }

    private static Integrations? Instance { get; set; }

    /// <summary>
    ///     Initializes <see cref="Integrations" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="Integrations" /> class.</returns>
    public static Integrations Init(IModHelper helper)
    {
        return Integrations.Instance ??= new(helper);
    }
}