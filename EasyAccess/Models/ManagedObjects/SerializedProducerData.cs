namespace StardewMods.EasyAccess.Models.ManagedObjects;

using System;
using System.Collections.Generic;
using System.Linq;
using StardewMods.EasyAccess.Enums;
using StardewMods.EasyAccess.Helpers;
using StardewMods.EasyAccess.Interfaces.Config;

/// <inheritdoc />
internal class SerializedProducerData : IProducerData
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SerializedProducerData" /> class.
    /// </summary>
    /// <param name="data">The Dictionary of string keys/values representing the <see cref="IProducerData" />.</param>
    public SerializedProducerData(IDictionary<string, string> data)
    {
        this.Data = data;
    }

    /// <inheritdoc />
    public int CollectOutputDistance
    {
        get => this.Data.TryGetValue("CollectOutputDistance", out var value) && int.TryParse(value, out var distance)
            ? distance
            : 0;
        set => this.Data["CollectOutputDistance"] = value == 0 ? string.Empty : value.ToString();
    }

    /// <inheritdoc />
    public HashSet<string> CollectOutputItems
    {
        get => this.Data.TryGetValue("CollectOutputItems", out var value) && !string.IsNullOrWhiteSpace(value)
            ? new(value.Split(','))
            : new();
        set => this.Data["CollectOutputItems"] = !value.Any() ? string.Empty : string.Join(",", value);
    }

    /// <inheritdoc />
    public FeatureOptionRange CollectOutputs
    {
        get => this.Data.TryGetValue("CollectOutputs", out var value) && Enum.TryParse(value, out FeatureOptionRange range)
            ? range
            : FeatureOptionRange.Default;
        set => this.Data["CollectOutputs"] = value == FeatureOptionRange.Default ? string.Empty : FormatHelper.GetRangeString(value);
    }

    /// <inheritdoc />
    public int DispenseInputDistance
    {
        get => this.Data.TryGetValue("DispenseInputDistance", out var value) && int.TryParse(value, out var distance)
            ? distance
            : 0;
        set => this.Data["DispenseInputDistance"] = value == 0 ? string.Empty : value.ToString();
    }

    /// <inheritdoc />
    public HashSet<string> DispenseInputItems
    {
        get => this.Data.TryGetValue("DispenseInputItems", out var value) && !string.IsNullOrWhiteSpace(value)
            ? new(value.Split(','))
            : new();
        set => this.Data["DispenseInputItems"] = !value.Any() ? string.Empty : string.Join(",", value);
    }

    /// <inheritdoc />
    public int DispenseInputPriority
    {
        get => this.Data.TryGetValue("DispenseInputPriority", out var value) && int.TryParse(value, out var distance)
            ? distance
            : 0;
        set => this.Data["DispenseInputPriority"] = value == 0 ? string.Empty : value.ToString();
    }

    /// <inheritdoc />
    public FeatureOptionRange DispenseInputs
    {
        get => this.Data.TryGetValue("DispenseInputs", out var value) && Enum.TryParse(value, out FeatureOptionRange range)
            ? range
            : FeatureOptionRange.Default;
        set => this.Data["DispenseInputs"] = value == FeatureOptionRange.Default ? string.Empty : FormatHelper.GetRangeString(value);
    }

    private IDictionary<string, string> Data { get; }

    /// <summary>
    ///     Converts a <see cref="IProducerData" /> instance into a dictionary representation.
    /// </summary>
    /// <param name="data">The <see cref="IProducerData" /> to create a data dictionary out of.</param>
    /// <returns>A dictionary of string keys/values representing the <see cref="IProducerData" />.</returns>
    public static IDictionary<string, string> GetData(IProducerData data)
    {
        var outDict = new Dictionary<string, string>();
        data.CopyTo(new SerializedProducerData(outDict));
        return outDict;
    }
}