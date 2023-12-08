namespace StardewMods.ToolbarIcons.Framework.Integrations.Vanilla;

using StardewValley.Menus;

/// <inheritdoc />
internal sealed class DailyQuests : ICustomIntegration
{
    private readonly ComplexIntegration complex;

    /// <summary>Initializes a new instance of the <see cref="DailyQuests" /> class.</summary>
    /// <param name="complex">Dependency for adding a complex mod integration.</param>
    public DailyQuests(ComplexIntegration complex) => this.complex = complex;

    /// <inheritdoc />
    public void AddIntegration() => this.complex.AddCustomAction(9, I18n.Button_DailyQuests(), DailyQuests.Action);

    private static void Action() => Game1.activeClickableMenu = new Billboard(true);
}
