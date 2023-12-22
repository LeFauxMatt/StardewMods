namespace StardewMods.ToolbarIcons.Framework.Integrations;

using System.Reflection;
using StardewMods.ToolbarIcons.Framework.Services;
using StardewValley.Menus;

/// <inheritdoc />
internal sealed class SimpleIntegration : BaseIntegration
{
    private readonly MethodInfo overrideButtonReflected;

    /// <summary>Initializes a new instance of the <see cref="SimpleIntegration" /> class.</summary>
    /// <param name="customEvents">Dependency used for custom events.</param>
    /// <param name="gameContent">Dependency used for loading game assets.</param>
    /// <param name="modRegistry">Dependency for fetching metadata about loaded mods.</param>
    /// <param name="reflection">Dependency used for accessing inaccessible code.</param>
    /// <param name="toolbar">API to add icons above or below the toolbar.</param>
    public SimpleIntegration(EventsManager customEvents, IGameContentHelper gameContent, IModRegistry modRegistry, IReflectionHelper reflection, ToolbarHandler toolbar)
        : base(customEvents, gameContent, modRegistry, reflection, toolbar) =>
        this.overrideButtonReflected = Game1.input.GetType().GetMethod("OverrideButton") ?? throw new MethodAccessException("Unable to access OverrideButton");

    /// <summary>Adds a simple mod integration for a keybind.</summary>
    /// <param name="modId">The id of the mod.</param>
    /// <param name="index">The index of the mod icon.</param>
    /// <param name="hoverText">The text to display.</param>
    /// <param name="keybinds">The method to run.</param>
    /// <param name="texturePath">The texture path of the icon.</param>
    /// <returns>Returns true if the icon was added.</returns>
    public bool AddKeybind(string modId, int index, string hoverText, string keybinds, string? texturePath = null)
    {
        if (!this.ModRegistry.IsLoaded(modId))
        {
            return false;
        }

        var keys = keybinds.Trim().Split(' ');
        IList<SButton> buttons = new List<SButton>();
        foreach (var key in keys)
        {
            if (Enum.TryParse(key, out SButton button))
            {
                buttons.Add(button);
            }
        }

        this.AddIntegration(
            modId,
            index,
            hoverText,
            () =>
            {
                foreach (var button in buttons)
                {
                    this.OverrideButton(button, true);
                }
            },
            texturePath);

        return true;
    }

    /// <summary>Adds a simple mod integration for a parameterless menu.</summary>
    /// <param name="modId">The id of the mod.</param>
    /// <param name="index">The index of the mod icon.</param>
    /// <param name="hoverText">The text to display.</param>
    /// <param name="fullName">The full name to the menu class.</param>
    /// <param name="texturePath">The texture path of the icon.</param>
    /// <returns>Returns true if the icon was added.</returns>
    public bool AddMenu(string modId, int index, string hoverText, string fullName, string? texturePath = null)
    {
        if (!this.TryGetMod(modId, out var mod))
        {
            return false;
        }

        var action = mod.GetType().Assembly.GetType(fullName)?.GetConstructor(Array.Empty<Type>());
        if (action is null)
        {
            return false;
        }

        this.AddIntegration(
            modId,
            index,
            hoverText,
            () =>
            {
                var menu = action.Invoke(Array.Empty<object>());
                Game1.activeClickableMenu = (IClickableMenu)menu;
            },
            texturePath);

        return true;
    }

    /// <summary>Adds a simple mod integration for a parameterless method.</summary>
    /// <param name="modId">The id of the mod.</param>
    /// <param name="index">The index of the mod icon.</param>
    /// <param name="hoverText">The text to display.</param>
    /// <param name="method">The method to run.</param>
    /// <param name="texturePath">The texture path of the icon.</param>
    /// <returns>Returns true if the icon was added.</returns>
    public bool AddMethod(string modId, int index, string hoverText, string method, string? texturePath = null)
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

        this.AddIntegration(modId, index, hoverText, () => action.Invoke(), texturePath);
        return true;
    }

    private void OverrideButton(SButton button, bool inputState) => this.overrideButtonReflected.Invoke(Game1.input, new object[] { button, inputState });
}
