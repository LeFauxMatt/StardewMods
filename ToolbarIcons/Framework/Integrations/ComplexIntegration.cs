namespace StardewMods.ToolbarIcons.Framework.Integrations;

using StardewMods.ToolbarIcons.Framework.Services;

/// <inheritdoc />
internal sealed class ComplexIntegration : BaseIntegration
{
    /// <summary>Initializes a new instance of the <see cref="ComplexIntegration" /> class.</summary>
    /// <param name="customEvents">Dependency used for custom events.</param>
    /// <param name="gameContent">Dependency used for loading game assets.</param>
    /// <param name="modRegistry">Dependency for fetching metadata about loaded mods.</param>
    /// <param name="reflection">Dependency used for accessing inaccessible code.</param>
    /// <param name="toolbar">Dependency for managing the toolbar icons.</param>
    public ComplexIntegration(EventsManager customEvents, IGameContentHelper gameContent, IModRegistry modRegistry, IReflectionHelper reflection, ToolbarHandler toolbar)
        : base(customEvents, gameContent, modRegistry, reflection, toolbar)
    {
        // Nothing
    }

    /// <summary>Adds a complex integration for vanilla.</summary>
    /// <param name="index">The index of the mod icon.</param>
    /// <param name="hoverText">The text to display.</param>
    /// <param name="action">Function which returns the action to perform.</param>
    /// <returns>Returns true if the icon was added.</returns>
    public bool AddCustomAction(int index, string hoverText, Action action)
    {
        this.AddIntegration(string.Empty, index, hoverText, action);
        return true;
    }

    /// <summary>Adds a complex mod integration.</summary>
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
        if (action is null)
        {
            return false;
        }

        this.AddIntegration(modId, index, hoverText, () => action.Invoke());
        return true;
    }

    /// <summary>Adds a simple mod integration for a method with parameters.</summary>
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

        var action = this.Reflection.GetMethod(mod, method, false);
        if (action is null)
        {
            return false;
        }

        this.AddIntegration(modId, index, hoverText, () => action.Invoke(arguments));
        return true;
    }
}
