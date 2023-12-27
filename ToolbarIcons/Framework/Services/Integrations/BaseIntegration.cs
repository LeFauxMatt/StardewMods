namespace StardewMods.ToolbarIcons.Framework.Services.Integrations;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.ToolbarIcons.Framework.Services;

/// <summary>Base class for adding toolbar icons for integrated mods.</summary>
internal abstract class BaseIntegration
{
    private readonly IGameContentHelper gameContentHelper;
    private readonly Dictionary<string, Action> icons = new();
    private readonly ToolbarHandler toolbarHandler;

    /// <summary>Initializes a new instance of the <see cref="BaseIntegration" /> class.</summary>
    /// <param name="eventsManager">Dependency used for custom events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="modRegistry">Dependency for fetching metadata about loaded mods.</param>
    /// <param name="reflectionHelper">Dependency used for accessing inaccessible code.</param>
    /// <param name="toolbarHandler">API to add icons above or below the toolbar.</param>
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameterInConstructor", Justification = "Dependency Injection")]
    protected BaseIntegration(
        EventsManager eventsManager,
        IGameContentHelper gameContentHelper,
        IModRegistry modRegistry,
        IReflectionHelper reflectionHelper,
        ToolbarHandler toolbarHandler)
    {
        // Init
        this.gameContentHelper = gameContentHelper;
        this.ModRegistry = modRegistry;
        this.ReflectionHelper = reflectionHelper;
        this.toolbarHandler = toolbarHandler;

        // Events
        eventsManager.ToolbarIconPressed += this.OnToolbarIconPressed;
    }

    /// <summary>Gets helper for fetching metadata about loaded mods.</summary>
    protected IModRegistry ModRegistry { get; }

    /// <summary>Gets helper for accessing inaccessible code.</summary>
    protected IReflectionHelper ReflectionHelper { get; }

    /// <summary>Adds a toolbar icon for an integrated mod.</summary>
    /// <param name="modId">The id of the mod.</param>
    /// <param name="index">The index of the mod icon.</param>
    /// <param name="hoverText">The text to display.</param>
    /// <param name="action">The action to perform for this icon.</param>
    /// <param name="texturePath">The texture path of the icon.</param>
    /// <returns>Returns true if the icon was added.</returns>
    protected bool AddIntegration(string modId, int index, string hoverText, Action action, string? texturePath = null)
    {
        var texture = this.gameContentHelper.Load<Texture2D>(texturePath ?? AssetHandler.IconPath);
        var cols = texture.Width / 16;
        this.toolbarHandler.AddToolbarIcon(
            $"{modId}.{hoverText}",
            texturePath ?? AssetHandler.IconPath,
            new Rectangle(16 * (index % cols), 16 * (index / cols), 16, 16),
            hoverText);

        this.icons.Add($"{modId}.{hoverText}", action);
        return true;
    }

    /// <summary>Tries to get the instance of a mod based on the mod id.</summary>
    /// <param name="modId">The unique id of the mod.</param>
    /// <param name="mod">The mod instance.</param>
    /// <returns>Returns true if the mod instance could be obtained.</returns>
    protected bool TryGetMod(string modId, [NotNullWhen(true)] out IMod? mod)
    {
        if (!this.ModRegistry.IsLoaded(modId))
        {
            mod = null;
            return false;
        }

        var modInfo = this.ModRegistry.Get(modId);
        mod = (IMod?)modInfo?.GetType().GetProperty("Mod")?.GetValue(modInfo);
        return mod is not null;
    }

    private void OnToolbarIconPressed(object? sender, string id)
    {
        if (this.icons.TryGetValue(id, out var action))
        {
            action.Invoke();
        }
    }
}