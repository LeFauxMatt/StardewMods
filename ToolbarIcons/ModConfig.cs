namespace StardewMods.ToolbarIcons;

using System.Collections.Generic;
using System.Globalization;
using System.Text;
using StardewMods.ToolbarIcons.Models;

/// <summary>
///     Mod config data.
/// </summary>
internal sealed class ModConfig
{
    /// <summary>
    ///     Gets or sets a value of the detected toolbar icons.
    /// </summary>
    public List<ToolbarIcon> Icons { get; set; } = new();

    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Icons:");
        foreach (var icon in this.Icons)
        {
            sb.AppendLine($"{icon.Id}: {icon.Enabled.ToString(CultureInfo.InvariantCulture)}");
        }

        return sb.ToString();
    }
}