namespace StardewMods.ToolbarIcons.ModIntegrations;

using System;
using StardewModdingAPI;
using StardewMods.Common.Integrations.ToolbarIcons;

/// <inheritdoc />
internal class ComplexIntegration : BaseIntegration
{
    private ComplexIntegration(IModHelper helper, IToolbarIconsApi api)
        : base(helper, api)
    {
    }

    private static ComplexIntegration? Instance { get; set; }

    /// <summary>
    ///     Initializes <see cref="ComplexIntegration" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="api">API to add icons above or below the toolbar.</param>
    /// <returns>Returns an instance of the <see cref="ComplexIntegration" /> class.</returns>
    public static ComplexIntegration Init(IModHelper helper, IToolbarIconsApi api)
    {
        return ComplexIntegration.Instance ??= new(helper, api);
    }

    /// <summary>
    ///     Adds a complex mod integration.
    /// </summary>
    /// <param name="modId">The id of the mod.</param>
    /// <param name="index">The index of the mod icon.</param>
    /// <param name="hoverText">The text to display.</param>
    /// <param name="getAction">Function which returns the action to perform.</param>
    /// <returns>Returns true if the icon was added.</returns>
    public bool AddCustomAction(string modId, int index, string hoverText, Func<IMod, Action?> getAction)
    {
        if (!this.TryGetMod(modId, out var mod))
        {
            return false;
        }

        var action = getAction(mod);
        if (action is not null)
        {
            this.AddIntegration(modId, index, hoverText, () => action.Invoke());
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Adds a simple mod integration for a method with parameters.
    /// </summary>
    /// <param name="modId">The id of the mod.</param>
    /// <param name="index">The index of the mod icon.</param>
    /// <param name="hoverText">The text to display.</param>
    /// <param name="method">The method to run.</param>
    /// <param name="arguments">The arguments to pass to the method.</param>
    /// <returns>Returns true if the icon was added.</returns>
    public bool AddMethodWithParams(string modId, int index, string hoverText, string method, params object?[] arguments)
    {
        if (!this.TryGetMod(modId, out var mod))
        {
            return false;
        }

        var action = this.Helper.Reflection.GetMethod(mod, method, false);
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