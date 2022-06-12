#nullable disable

namespace StardewMods.EasyAccess.Models.Config;

using System.Collections.Generic;
using StardewMods.EasyAccess.Interfaces.Config;

/// <inheritdoc />
internal class ProducerData : IProducerData
{
    /// <inheritdoc />
    public HashSet<string> CollectOutputItems { get; set; } = new();

    /// <inheritdoc />
    public HashSet<string> DispenseInputItems { get; set; } = new();

    /// <inheritdoc />
    public int DispenseInputPriority { get; set; }
}