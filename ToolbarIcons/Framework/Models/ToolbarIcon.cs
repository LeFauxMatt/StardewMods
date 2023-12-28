namespace StardewMods.ToolbarIcons.Framework.Models;

/// <summary>A single Toolbar Icon.</summary>
public sealed class ToolbarIcon
{
    /// <summary>Initializes a new instance of the <see cref="ToolbarIcon" /> class.</summary>
    /// <param name="id">The id of the toolbar icon.</param>
    /// <param name="enabled">Whether the toolbar icon is enabled.</param>
    public ToolbarIcon(string id, bool enabled = true)
    {
        this.Id = id;
        this.Enabled = enabled;
    }

    /// <summary>Gets or sets a value indicating whether the Toolbar Icon is enabled.</summary>
    public bool Enabled { get; set; }

    /// <summary>Gets or sets the Id of the Toolbar Icon.</summary>
    public string Id { get; set; }
}