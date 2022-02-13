namespace StardewMods.EasyAccess.Interfaces.Config;

using System.Collections.Generic;
using StardewMods.EasyAccess.Enums;

/// <summary>
///     Producer data related to EasyAccess features.
/// </summary>
internal interface IProducerData
{
    /// <summary>
    ///     Gets or sets a value indicating the distance in tiles that the producer can be collected from.
    /// </summary>
    public int CollectOutputDistance { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating what categories of items can be collected from the producer.
    /// </summary>
    public HashSet<string> CollectOutputItems { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating if the producer can be collected from.
    /// </summary>
    public FeatureOptionRange CollectOutputs { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating the distance in tiles that the producer can be dispensed into.
    /// </summary>
    public int DispenseInputDistance { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating what categories of items can be dispensed into the producer.
    /// </summary>
    public HashSet<string> DispenseInputItems { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating the priority that producers will be dispensed into.
    /// </summary>
    public int DispenseInputPriority { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating if the producer can be dispensed into.
    /// </summary>
    public FeatureOptionRange DispenseInputs { get; set; }

    /// <summary>
    ///     Copies data from one <see cref="IProducerData" /> to another.
    /// </summary>
    /// <param name="other">The <see cref="IProducerData" /> to copy values to.</param>
    /// <typeparam name="TOther">The class/type of the other <see cref="IProducerData" />.</typeparam>
    public void CopyTo<TOther>(TOther other)
        where TOther : IProducerData
    {
        other.CollectOutputs = this.CollectOutputs;
        other.CollectOutputDistance = this.CollectOutputDistance;
        other.CollectOutputItems = this.CollectOutputItems;
        other.DispenseInputs = this.DispenseInputs;
        other.DispenseInputDistance = this.DispenseInputDistance;
        other.DispenseInputItems = this.DispenseInputItems;
        other.DispenseInputPriority = this.DispenseInputPriority;
    }
}