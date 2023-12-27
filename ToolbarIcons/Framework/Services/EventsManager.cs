namespace StardewMods.ToolbarIcons.Framework.Services;

using StardewMods.Common.Extensions;

/// <summary>Service for managing custom events.</summary>
internal sealed class EventsManager
{
    private EventHandler<string>? toolbarIconPressed;
    private EventHandler? toolbarIconsChanged;
    private EventHandler? toolbarIconsLoaded;

    /// <summary>Event raised after Toolbar Icons have changed.</summary>
    public event EventHandler ToolbarIconsChanged
    {
        add => this.toolbarIconsChanged += value;
        remove => this.toolbarIconsChanged -= value;
    }

    /// <summary>Event raised after Toolbar Icons have been loaded.</summary>
    public event EventHandler ToolbarIconsLoaded
    {
        add => this.toolbarIconsLoaded += value;
        remove => this.toolbarIconsLoaded -= value;
    }

    /// <summary>Event raised after a toolbar icon is pressed.</summary>
    public event EventHandler<string> ToolbarIconPressed
    {
        add => this.toolbarIconPressed += value;
        remove => this.toolbarIconPressed -= value;
    }

    /// <summary>Invokes the ToolbarIconsChanged event.</summary>
    public void InvokeToolbarIconsChanged() => this.toolbarIconsChanged.InvokeAll(this);

    /// <summary>Invokes the ToolbarIconsLoaded event.</summary>
    public void InvokeToolbarIconsLoaded() => this.toolbarIconsLoaded.InvokeAll(this);

    /// <summary>Invokes the ToolbarIconPressed event.</summary>
    /// <param name="id">The id of the toolbar icon pressed.</param>
    public void InvokeToolbarIconPressed(string id) => this.toolbarIconPressed.InvokeAll(this, id);
}