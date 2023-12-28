namespace StardewMods.GarbageDay;

using System.ComponentModel;
using System.Globalization;
using System.Text;

/// <summary>Mod config data for Garbage Day.</summary>
internal sealed class ModConfig
{
    /// <summary>Gets or sets the day of the week that garbage is collected.</summary>
    public DayOfWeek GarbageDay { get; set; } = DayOfWeek.Monday;

    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder();
        var dow = this.GarbageDay switch
        {
            DayOfWeek.Sunday => "Sunday",
            DayOfWeek.Monday => "Monday",
            DayOfWeek.Tuesday => "Tuesday",
            DayOfWeek.Wednesday => "Wednesday",
            DayOfWeek.Thursday => "Thursday",
            DayOfWeek.Friday => "Friday",
            DayOfWeek.Saturday => "Saturday",
            _ => throw new InvalidEnumArgumentException(),
        };

        sb.AppendLine(CultureInfo.InvariantCulture, $"GarbageDay: {dow}");
        return sb.ToString();
    }
}