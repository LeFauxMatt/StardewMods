namespace StardewMods.ToolbarIcons.Framework.Services;

using StardewMods.Common.Extensions;
using StardewMods.Common.Services.Integrations.ToolbarIcons;
using StardewMods.ToolbarIcons.Framework.Models;

/// <summary>Service for managing custom events.</summary>
internal sealed class EventsManager
{
    private EventHandler<IIconPressedEventArgs>? iconPressed;
    private EventHandler? toolbarIconsChanged;
    private EventHandler? toolbarIconsLoaded;

    /// <summary>Raised after a toolbar icon is pressed.</summary>
    public event EventHandler<IIconPressedEventArgs> IconPressed
    {
        add => this.iconPressed += value;
        remove => this.iconPressed -= value;
    }

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

    /// <summary>Invokes the IconPressed event.</summary>
    /// <param name="id">The id of the toolbar icon pressed.</param>
    /// <param name="button">The button that was pressed.</param>
    public void InvokeToolbarIconPressed(string id, SButton button) =>
        this.iconPressed.InvokeAll(this, new IconPressedEventArgs(id, button));

    /// <summary>Invokes the ToolbarIconsChanged event.</summary>
    public void InvokeToolbarIconsChanged() => this.toolbarIconsChanged.InvokeAll(this);

    /// <summary>Invokes the ToolbarIconsLoaded event.</summary>
    public void InvokeToolbarIconsLoaded() => this.toolbarIconsLoaded.InvokeAll(this);
}