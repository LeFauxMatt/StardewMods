namespace StardewMods.ToolbarIcons.Framework.Models;

using StardewMods.ToolbarIcons.Framework.Interfaces;

/// <inheritdoc />
internal sealed class DefaultConfig : IModConfig
{
    /// <inheritdoc />
    public List<ToolbarIcon> Icons { get; set; } = [];

    /// <inheritdoc />
    public float Scale { get; set; } = 2;
}