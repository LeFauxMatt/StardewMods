namespace StardewMods.PortableHoles;

using System.Globalization;
using System.Text;

/// <summary>
///     Mod config data.
/// </summary>
public sealed class ModConfig
{
    /// <summary>
    ///     Gets or sets a value indicating whether damage while falling will be negated.
    /// </summary>
    public bool SoftFall { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether to unlock the recipe automatically.
    /// </summary>
    public bool UnlockAutomatically { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"SoftFall: {this.SoftFall.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"UnlockAutomatically: {this.UnlockAutomatically.ToString(CultureInfo.InvariantCulture)}");
        return sb.ToString();
    }
}