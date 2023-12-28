namespace StardewMods.ToolbarIcons.Framework.Services.Integrations.Vanilla;

using StardewMods.ToolbarIcons.Framework.Interfaces;
using StardewValley.Menus;

/// <inheritdoc />
internal sealed class SpecialOrders : IVanillaIntegration
{
    /// <inheritdoc/>
    public int Index => 8;

    /// <inheritdoc/>
    public string HoverText => I18n.Button_SpecialOrders();

    /// <inheritdoc/>
    public void DoAction() => Game1.activeClickableMenu = new SpecialOrdersBoard();
}