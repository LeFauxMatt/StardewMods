namespace StardewMods.ToolbarIcons.Framework.Services.Integrations.Vanilla;

using StardewMods.ToolbarIcons.Framework.Interfaces;
using StardewValley.Menus;

/// <inheritdoc />
internal sealed class SpecialOrders : ICustomIntegration
{
    private readonly ComplexIntegration complexIntegration;

    /// <summary>Initializes a new instance of the <see cref="SpecialOrders" /> class.</summary>
    /// <param name="complexIntegration">Dependency for adding a complex mod integration.</param>
    public SpecialOrders(ComplexIntegration complexIntegration) => this.complexIntegration = complexIntegration;

    /// <inheritdoc />
    public void AddIntegration() =>
        this.complexIntegration.AddCustomAction(8, I18n.Button_SpecialOrders(), SpecialOrders.Action);

    private static void Action() => Game1.activeClickableMenu = new SpecialOrdersBoard();
}