namespace StardewMods.EasyAccess;

using System.Globalization;
using System.Text;

/// <summary>
///     Mod config data.
/// </summary>
internal sealed class ModConfig
{
    /// <summary>
    ///     Gets or sets a value indicating the distance in tiles that the producer can be collected from.
    /// </summary>
    public int CollectOutputDistance { get; set; } = 15;

    /// <summary>
    ///     Gets or sets the control scheme.
    /// </summary>
    public Controls ControlScheme { get; set; } = new();

    /// <summary>
    ///     Gets or sets a value indicating the distance in tiles that the producer can be dispensed into.
    /// </summary>
    public int DispenseInputDistance { get; set; } = 15;

    /// <summary>
    ///     Gets or sets a value indicating whether CollectItems will grab from dig spots.
    /// </summary>
    public bool DoDigSpots { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether CollectItems will drop forage as debris.
    /// </summary>
    public bool DoForage { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether CollectItems will collect from machines.
    /// </summary>
    public bool DoMachines { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether CollectItems will interact with Terrain features such as bushes and trees.
    /// </summary>
    public bool DoTerrain { get; set; } = true;

    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"CollectOutputDistance: {this.CollectOutputDistance.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"DispenseInputDistance: {this.DispenseInputDistance.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"DoDigSpots: {this.DoDigSpots.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"DoForage: {this.DoForage.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"DoMachines: {this.DoMachines.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"DoTerrain: {this.DoTerrain.ToString(CultureInfo.InvariantCulture)}");
        return sb.ToString();
    }
}