#nullable disable

namespace StardewMods.EasyAccess.Interfaces.Config;

using System.Collections.Generic;

/// <summary>
///     Producer data related to EasyAccess features.
/// </summary>
internal interface IProducerData
{
    /// <summary>
    ///     Gets or sets a value indicating what categories of items can be collected from the producer.
    /// </summary>
    public HashSet<string> CollectOutputItems { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating what categories of items can be dispensed into the producer.
    /// </summary>
    public HashSet<string> DispenseInputItems { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating the priority that producers will be dispensed into.
    /// </summary>
    public int DispenseInputPriority { get; set; }

    /// <summary>
    ///     Copies data from one <see cref="IProducerData" /> to another.
    /// </summary>
    /// <param name="other">The <see cref="IProducerData" /> to copy values to.</param>
    /// <typeparam name="TOther">The class/type of the other <see cref="IProducerData" />.</typeparam>
    public void CopyTo<TOther>(TOther other)
        where TOther : IProducerData
    {
        other.CollectOutputItems = this.CollectOutputItems;
        other.DispenseInputItems = this.DispenseInputItems;
        other.DispenseInputPriority = this.DispenseInputPriority;
    }
}