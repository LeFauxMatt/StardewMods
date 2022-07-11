namespace StardewMods.Common.Integrations.ToolbarIcons;

/// <summary>
///     A single Toolbar Icon.
/// </summary>
public interface IToolbarIcon
{
    /// <summary>
    ///     Gets or sets a value indicating whether the Toolbar Icon is enabled.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    ///     Gets the Id of the Toolbar Icon.
    /// </summary>
    public string Id { get; }
}