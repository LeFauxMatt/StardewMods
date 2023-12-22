namespace StardewMods.ToolbarIcons.Framework;

using Microsoft.Xna.Framework;
using StardewMods.Common.Services.Integrations.ToolbarIcons;
using StardewMods.ToolbarIcons.Framework.Services;

/// <inheritdoc />
public sealed class ToolbarIconsApi : IToolbarIconsApi
{
    private readonly string prefix;
    private readonly ToolbarHandler toolbar;

    private EventHandler<string>? toolbarIconPressed;

    /// <summary>Initializes a new instance of the <see cref="ToolbarIconsApi" /> class.</summary>
    /// <param name="mod">Mod info from the calling mod.</param>
    /// <param name="customEvents">Dependency used for custom events.</param>
    /// <param name="toolbar">Dependency for managing the toolbar icons.</param>
    internal ToolbarIconsApi(IModInfo mod, EventsManager customEvents, ToolbarHandler toolbar)
    {
        // Init
        this.prefix = $"{mod.Manifest.UniqueID}/";
        this.toolbar = toolbar;

        // Events
        customEvents.ToolbarIconPressed += this.OnToolbarIconPressed;
    }

    /// <summary>Raised after a toolbar icon is pressed.</summary>
    public event EventHandler<string> ToolbarIconPressed
    {
        add => this.toolbarIconPressed += value;
        remove => this.toolbarIconPressed -= value;
    }

    /// <inheritdoc />
    public void AddToolbarIcon(string id, string texturePath, Rectangle? sourceRect, string? hoverText) =>
        this.toolbar.AddToolbarIcon($"{this.prefix}{id}", texturePath, sourceRect, hoverText);

    /// <inheritdoc />
    public void RemoveToolbarIcon(string id) => this.toolbar.RemoveToolbarIcon($"{this.prefix}{id}");

    private void OnToolbarIconPressed(object? sender, string id)
    {
        if (this.toolbarIconPressed is null || !id.StartsWith(this.prefix, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        id = id[this.prefix.Length..];
        foreach (var handler in this.toolbarIconPressed.GetInvocationList())
        {
            try
            {
                handler.DynamicInvoke(this, id);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
