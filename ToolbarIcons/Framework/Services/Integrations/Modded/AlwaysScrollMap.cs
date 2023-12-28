namespace StardewMods.ToolbarIcons.Framework.Services.Integrations.Modded;

using StardewMods.ToolbarIcons.Framework.Interfaces;

/// <inheritdoc />
internal sealed class AlwaysScrollMap : IActionIntegration
{
    private readonly IReflectionHelper reflectionHelper;

    /// <summary>Initializes a new instance of the <see cref="AlwaysScrollMap" /> class.</summary>
    /// <param name="reflectionHelper">Dependency used for accessing inaccessible code.</param>
    public AlwaysScrollMap(IReflectionHelper reflectionHelper) => this.reflectionHelper = reflectionHelper;

    /// <inheritdoc />
    public string ModId => "bcmpinc.AlwaysScrollMap";

    /// <inheritdoc />
    public int Index => 6;

    /// <inheritdoc />
    public string HoverText => I18n.Button_AlwaysScrollMap();

    /// <inheritdoc />
    public Action? GetAction(IMod mod)
    {
        var config = mod.GetType().GetField("config")?.GetValue(mod);
        if (config is null)
        {
            return null;
        }

        var enabledIndoors = this.reflectionHelper.GetField<bool>(config, "EnabledIndoors", false);
        var enabledOutdoors = this.reflectionHelper.GetField<bool>(config, "EnabledOutdoors", false);
        return () =>
        {
            if (Game1.currentLocation.IsOutdoors)
            {
                enabledOutdoors.SetValue(!enabledOutdoors.GetValue());
            }
            else
            {
                enabledIndoors.SetValue(!enabledIndoors.GetValue());
            }
        };
    }
}