namespace StardewMods.ToolbarIcons.Models;

using StardewMods.Common.Integrations.ToolbarIcons;

/// <inheritdoc />
public class ToolbarIcon : IToolbarIcon
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ToolbarIcon" /> class.
    /// </summary>
    /// <param name="id">The id of the toolbar icon.</param>
    /// <param name="enabled">Whether the toolbar icon is enabled.</param>
    public ToolbarIcon(string id, bool enabled = true)
    {
        this.Id = id;
        this.Enabled = enabled;
    }

    /// <inheritdoc />
    public bool Enabled { get; set; }

    /// <inheritdoc />
    public string Id { get; set; }
}