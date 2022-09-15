namespace StardewMods.GarbageDay;

using System.Text;
using StardewMods.Common.Enums;

/// <summary>
///     Mod config data.
/// </summary>
internal sealed class ModConfig
{
    /// <summary>
    ///     Gets or sets the day of the week that garbage is collected.
    /// </summary>
    public DayOfWeek GarbageDay { get; set; } = DayOfWeek.Monday;

    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"GarbageDay: {this.GarbageDay.ToStringFast()}");
        return sb.ToString();
    }
}