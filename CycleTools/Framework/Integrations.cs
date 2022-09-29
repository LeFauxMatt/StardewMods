namespace StardewMods.CycleTools.Framework;

using StardewMods.Common.Integrations.GenericModConfigMenu;

/// <summary>
///     Handles integrations with other mods.
/// </summary>
internal sealed class Integrations
{
#nullable disable
    private static Integrations Instance;
#nullable enable
    private readonly GenericModConfigMenuIntegration _gmcm;

    private readonly IModHelper _helper;

    private Integrations(IModHelper helper)
    {
        this._helper = helper;
        this._gmcm = new(helper.ModRegistry);
    }

    /// <summary>
    ///     Gets Generic Mod Config Menu integration.
    /// </summary>
    public static GenericModConfigMenuIntegration GMCM => Integrations.Instance._gmcm;

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