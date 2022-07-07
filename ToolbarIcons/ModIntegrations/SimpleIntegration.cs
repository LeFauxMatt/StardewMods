namespace StardewMods.ToolbarIcons.ModIntegrations;

using StardewModdingAPI;
using StardewMods.Common.Integrations.ToolbarIcons;

/// <inheritdoc />
internal class SimpleIntegration : BaseIntegration
{
    private SimpleIntegration(IModHelper helper, IToolbarIconsApi api)
        : base(helper, api)
    {
    }

    private static SimpleIntegration? Instance { get; set; }

    /// <summary>
    ///     Initializes <see cref="SimpleIntegration" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="api">API to add icons above or below the toolbar.</param>
    /// <returns>Returns an instance of the <see cref="SimpleIntegration" /> class.</returns>
    public static SimpleIntegration Init(IModHelper helper, IToolbarIconsApi api)
    {
        return SimpleIntegration.Instance ??= new(helper, api);
    }

    /// <summary>
    ///     Adds a simple mod integration.
    /// </summary>
    /// <param name="modId">The id of the mod.</param>
    /// <param name="index">The index of the mod icon.</param>
    /// <param name="hoverText">The text to display.</param>
    /// <param name="method">The method to run.</param>
    /// <param name="arguments">The arguments to pass to the method.</param>
    /// <returns>Returns true if the icon was added.</returns>
    public bool AddIntegration(string modId, int index, string hoverText, string method, params object?[] arguments)
    {
        if (!this.TryGetMod(modId, out var mod))
        {
            return false;
        }

        var action = this.Helper.Reflection.GetMethod(mod, method);
        if (action is not null)
        {
            this.AddIntegration(
                modId,
                index,
                hoverText,
                () => action.Invoke(arguments));
            return true;
        }

        return false;
    }
}