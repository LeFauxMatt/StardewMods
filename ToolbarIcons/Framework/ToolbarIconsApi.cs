namespace StardewMods.ToolbarIcons.Framework;

using Microsoft.Xna.Framework;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.Common.Services.Integrations.ToolbarIcons;
using StardewMods.ToolbarIcons.Framework.Models;
using StardewMods.ToolbarIcons.Framework.Services;

/// <inheritdoc />
public sealed class ToolbarIconsApi : IToolbarIconsApi
{
    private readonly string prefix;
    private readonly ILog log;
    private readonly IModInfo modInfo;
    private readonly ToolbarManager toolbarManager;

    private EventHandler<IIconPressedEventArgs>? iconPressed;
    private EventHandler<string>? toolbarIconPressed;

    /// <summary>Initializes a new instance of the <see cref="ToolbarIconsApi" /> class.</summary>
    /// <param name="eventsManager">Dependency used for custom events.</param>
    /// <param name="log">Dependency used for monitoring and logging.</param>
    /// <param name="modInfo">Mod info from the calling mod.</param>
    /// <param name="toolbarManager">Dependency for managing the toolbar icons.</param>
    internal ToolbarIconsApi(EventsManager eventsManager, ILog log, IModInfo modInfo, ToolbarManager toolbarManager)
    {
        // Init
        this.log = log;
        this.modInfo = modInfo;
        this.prefix = this.modInfo.Manifest.UniqueID + "/";
        this.toolbarManager = toolbarManager;

        // Events
        eventsManager.IconPressed += this.OnIconPressed;
    }

    /// <inheritdoc />
    public event EventHandler<IIconPressedEventArgs> IconPressed
    {
        add => this.iconPressed += value;
        remove => this.iconPressed -= value;
    }

    /// <inheritdoc />
    public event EventHandler<string> ToolbarIconPressed
    {
        add
        {
            this.log.WarnOnce(
                "{0} uses deprecated code. {1} event is deprecated. Please use the {2} event instead.",
                [this.modInfo.Manifest.Name, nameof(this.ToolbarIconPressed), nameof(this.IconPressed)]);

            this.toolbarIconPressed += value;
        }
        remove => this.toolbarIconPressed -= value;
    }

    /// <inheritdoc />
    public void AddToolbarIcon(string id, string texturePath, Rectangle? sourceRect, string? hoverText) =>
        this.toolbarManager.AddToolbarIcon($"{this.prefix}{id}", texturePath, sourceRect, hoverText);

    /// <inheritdoc />
    public void RemoveToolbarIcon(string id) => this.toolbarManager.RemoveToolbarIcon($"{this.prefix}{id}");

    private void OnIconPressed(object? sender, IIconPressedEventArgs e)
    {
        if (!e.Id.StartsWith(this.prefix, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var id = e.Id[this.prefix.Length..];
        if (this.iconPressed is not null)
        {
            foreach (var handler in this.iconPressed.GetInvocationList())
            {
                try
                {
                    handler.DynamicInvoke(this, new IconPressedEventArgs(id, e.Button));
                }
                catch (Exception ex)
                {
                    this.log.Error(
                        "{0} failed in {1}: {2}",
                        [this.modInfo.Manifest.Name, nameof(this.IconPressed), ex.Message]);
                }
            }
        }

        if (this.toolbarIconPressed is not null)
        {
            foreach (var handler in this.toolbarIconPressed.GetInvocationList())
            {
                try
                {
                    handler.DynamicInvoke(this, id);
                }
                catch (Exception ex)
                {
                    this.log.Error(
                        "{0} failed in {1}: {2}",
                        [this.modInfo.Manifest.Name, nameof(this.IconPressed), ex.Message]);
                }
            }
        }
    }
}