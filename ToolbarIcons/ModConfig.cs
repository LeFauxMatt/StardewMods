namespace StardewMods.ToolbarIcons;

using System.Globalization;
using System.Text;
using StardewMods.ToolbarIcons.Framework.Models;

/// <summary>Mod config data for Toolbar Icons.</summary>
internal sealed class ModConfig
{
    /// <summary>Gets or sets a value containing the toolbar icons.</summary>
    public List<ToolbarIcon> Icons { get; set; } = new();

    /// <summary>Gets or sets the size that icons will be scaled to.</summary>
    public float Scale { get; set; } = 2;

    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine(CultureInfo.InvariantCulture, $"Scale: {this.Scale.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine("Icons:");
        foreach (var icon in this.Icons)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"{icon.Id}: {icon.Enabled.ToString(CultureInfo.InvariantCulture)}");
        }

        return sb.ToString();
    }
}
