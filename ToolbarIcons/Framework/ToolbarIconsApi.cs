namespace StardewMods.ToolbarIcons.Framework;

using Microsoft.Xna.Framework;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.Common.Services.Integrations.ToolbarIcons;
using StardewMods.ToolbarIcons.Framework.Models.Events;
using StardewMods.ToolbarIcons.Framework.Services;

/// <inheritdoc />
public sealed class ToolbarIconsApi : IToolbarIconsApi
{
    private readonly BaseEventManager eventManager;
    private readonly string prefix;
    private readonly ILog log;
    private readonly IModInfo modInfo;
    private readonly ToolbarManager toolbarManager;

    private EventHandler<string>? toolbarIconPressed;

    /// <summary>Initializes a new instance of the <see cref="ToolbarIconsApi" /> class.</summary>
    /// <param name="eventSubscriber">Dependency used for subscribing to events.</param>
    /// <param name="log">Dependency used for monitoring and logging.</param>
    /// <param name="modInfo">Mod info from the calling mod.</param>
    /// <param name="toolbarManager">Dependency for managing the toolbar icons.</param>
    internal ToolbarIconsApi(
        IEventSubscriber eventSubscriber,
        ILog log,
        IModInfo modInfo,
        ToolbarManager toolbarManager)
    {
        // Init
        this.log = log;
        this.modInfo = modInfo;
        this.prefix = this.modInfo.Manifest.UniqueID + "/";
        this.toolbarManager = toolbarManager;
        this.eventManager = new BaseEventManager(log, modInfo.Manifest);

        // Events
        eventSubscriber.Subscribe<IconPressedEventArgs>(this.OnIconPressed);
    }

    /// <inheritdoc />
    public event EventHandler<string> ToolbarIconPressed
    {
        add
        {
            this.log.WarnOnce(
                "{0} uses deprecated code. {1} event is deprecated. Please subscribe to the {2} event instead.",
                this.modInfo.Manifest.Name,
                nameof(this.ToolbarIconPressed),
                nameof(IIconPressedEventArgs));

            this.toolbarIconPressed += value;
        }
        remove => this.toolbarIconPressed -= value;
    }

    /// <inheritdoc />
    public void AddToolbarIcon(string id, string texturePath, Rectangle? sourceRect, string? hoverText) =>
        this.toolbarManager.AddToolbarIcon($"{this.prefix}{id}", texturePath, sourceRect, hoverText);

    /// <inheritdoc />
    public void RemoveToolbarIcon(string id) => this.toolbarManager.RemoveToolbarIcon($"{this.prefix}{id}");

    /// <inheritdoc />
    public void Subscribe<TEventArgs>(Action<TEventArgs> handler) => this.eventManager.Subscribe(handler);

    /// <inheritdoc />
    public void Unsubscribe<TEventArgs>(Action<TEventArgs> handler) => this.eventManager.Unsubscribe(handler);

    private void OnIconPressed(IconPressedEventArgs e)
    {
        if (!e.Id.StartsWith(this.prefix, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var id = e.Id[this.prefix.Length..];
        this.eventManager.Publish<IIconPressedEventArgs, IconPressedEventArgs>(new IconPressedEventArgs(id, e.Button));

        if (this.toolbarIconPressed is null)
        {
            return;
        }

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
                    this.modInfo.Manifest.Name,
                    nameof(this.ToolbarIconPressed),
                    ex.Message);
            }
        }
    }
}