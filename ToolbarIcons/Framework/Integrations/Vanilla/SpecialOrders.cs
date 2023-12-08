namespace StardewMods.ToolbarIcons.Framework.Integrations.Vanilla;

using StardewValley.Menus;

/// <inheritdoc />
internal sealed class SpecialOrders : ICustomIntegration
{
    private readonly ComplexIntegration complex;

    /// <summary>Initializes a new instance of the <see cref="SpecialOrders" /> class.</summary>
    /// <param name="complex">Dependency for adding a complex mod integration.</param>
    public SpecialOrders(ComplexIntegration complex) => this.complex = complex;

    /// <inheritdoc />
    public void AddIntegration() => this.complex.AddCustomAction(8, I18n.Button_SpecialOrders(), SpecialOrders.Action);

    private static void Action() => Game1.activeClickableMenu = new SpecialOrdersBoard();
}
