namespace StardewMods.ToolbarIcons.Framework.Services.Integrations.Modded;

using StardewMods.ToolbarIcons.Framework.Interfaces;

/// <inheritdoc />
internal sealed class AlwaysScrollMap : ICustomIntegration
{
    private const string ModId = "bcmpinc.AlwaysScrollMap";

    private readonly ComplexIntegration complexIntegration;
    private readonly IReflectionHelper reflectionHelper;

    /// <summary>Initializes a new instance of the <see cref="AlwaysScrollMap" /> class.</summary>
    /// <param name="complexIntegration">Dependency for adding a complex mod integration.</param>
    /// <param name="reflectionHelper">Dependency used for accessing inaccessible code.</param>
    public AlwaysScrollMap(ComplexIntegration complexIntegration, IReflectionHelper reflectionHelper)
    {
        this.complexIntegration = complexIntegration;
        this.reflectionHelper = reflectionHelper;
    }

    /// <inheritdoc />
    public void AddIntegration() =>
        this.complexIntegration.AddCustomAction(
            AlwaysScrollMap.ModId,
            6,
            I18n.Button_AlwaysScrollMap(),
            this.GetAction);

    private Action? GetAction(IMod mod)
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