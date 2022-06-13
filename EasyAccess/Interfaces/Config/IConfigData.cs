#nullable disable

namespace StardewMods.EasyAccess.Interfaces.Config;

using StardewMods.EasyAccess.Models.Config;

/// <summary>
///     Mod config data.
/// </summary>
internal interface IConfigData
{
    /// <summary>
    ///     Gets or sets a value indicating the distance in tiles that the producer can be collected from.
    /// </summary>
    public int CollectOutputDistance { get; set; }

    /// <summary>
    ///     Gets or sets the control scheme.
    /// </summary>
    ControlScheme ControlScheme { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating the distance in tiles that the producer can be dispensed into.
    /// </summary>
    public int DispenseInputDistance { get; set; }

    /// <summary>
    ///     Copies data from one <see cref="IConfigData" /> to another.
    /// </summary>
    /// <param name="other">The <see cref="IConfigData" /> to copy values to.</param>
    /// <typeparam name="TOther">The class/type of the other <see cref="IConfigData" />.</typeparam>
    public void CopyTo<TOther>(TOther other)
        where TOther : IConfigData
    {
        other.CollectOutputDistance = this.CollectOutputDistance;
        other.DispenseInputDistance = this.DispenseInputDistance;
        ((IControlScheme)other.ControlScheme).CopyTo(this.ControlScheme);
    }
}