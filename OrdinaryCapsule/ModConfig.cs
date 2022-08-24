namespace StardewMods.OrdinaryCapsule;

using System.Globalization;
using System.Text;

/// <summary>
///     Mod config data.
/// </summary>
public class ModConfig
{
    /// <summary>
    ///     Gets or sets a value indicating whether to unlock the recipe automatically.
    /// </summary>
    public bool UnlockAutomatically { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"UnlockAutomatically: {this.UnlockAutomatically.ToString(CultureInfo.InvariantCulture)}");
        return sb.ToString();
    }
}