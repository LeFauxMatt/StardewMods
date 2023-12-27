namespace StardewMods.ToolbarIcons.Framework.Services.Integrations.Vanilla;

using StardewMods.ToolbarIcons.Framework.Interfaces;
using StardewValley.Menus;

/// <inheritdoc />
internal sealed class DailyQuests : ICustomIntegration
{
    private readonly ComplexIntegration complexIntegration;

    /// <summary>Initializes a new instance of the <see cref="DailyQuests" /> class.</summary>
    /// <param name="complexIntegration">Dependency for adding a complex mod integration.</param>
    public DailyQuests(ComplexIntegration complexIntegration) => this.complexIntegration = complexIntegration;

    /// <inheritdoc />
    public void AddIntegration() =>
        this.complexIntegration.AddCustomAction(9, I18n.Button_DailyQuests(), DailyQuests.Action);

    private static void Action() => Game1.activeClickableMenu = new Billboard(true);
}