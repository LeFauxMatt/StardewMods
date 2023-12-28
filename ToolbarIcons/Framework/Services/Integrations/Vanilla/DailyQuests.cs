namespace StardewMods.ToolbarIcons.Framework.Services.Integrations.Vanilla;

using StardewMods.ToolbarIcons.Framework.Interfaces;
using StardewValley.Menus;

/// <inheritdoc />
internal sealed class DailyQuests : IVanillaIntegration
{
    /// <inheritdoc/>
    public int Index => 9;

    /// <inheritdoc/>
    public string HoverText => I18n.Button_DailyQuests();

    /// <inheritdoc/>
    public void DoAction() => Game1.activeClickableMenu = new Billboard(true);
}