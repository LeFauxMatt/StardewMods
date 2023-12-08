namespace StardewMods.ToolbarIcons.Framework.Integrations.Mods;

/// <inheritdoc />
internal sealed class AlwaysScrollMap : ICustomIntegration
{
    private const string ModId = "bcmpinc.AlwaysScrollMap";

    private readonly ComplexIntegration complex;
    private readonly IReflectionHelper reflection;

    /// <summary>Initializes a new instance of the <see cref="AlwaysScrollMap" /> class.</summary>
    /// <param name="complex">Dependency for adding a complex mod integration.</param>
    /// <param name="reflection">Dependency used for accessing inaccessible code.</param>
    public AlwaysScrollMap(ComplexIntegration complex, IReflectionHelper reflection)
    {
        this.complex = complex;
        this.reflection = reflection;
    }

    /// <inheritdoc />
    public void AddIntegration() =>
        this.complex.AddCustomAction(AlwaysScrollMap.ModId, 6, I18n.Button_AlwaysScrollMap(), this.GetAction);

    private Action? GetAction(IMod mod)
    {
        var config = mod.GetType().GetField("config")?.GetValue(mod);
        if (config is null)
        {
            return null;
        }

        var enabledIndoors = this.reflection.GetField<bool>(config, "EnabledIndoors", false);
        var enabledOutdoors = this.reflection.GetField<bool>(config, "EnabledOutdoors", false);
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
