namespace StardewMods.EasyAccess.Models.ManagedObjects;

using System;
using System.Collections.Generic;
using System.Linq;
using StardewMods.EasyAccess.Enums;
using StardewMods.EasyAccess.Helpers;
using StardewMods.EasyAccess.Interfaces.Config;
using StardewMods.EasyAccess.Interfaces.ManagedObjects;
using StardewMods.FuryCore.Helpers;
using StardewMods.FuryCore.Interfaces.GameObjects;
using StardewMods.FuryCore.Models.GameObjects;
using StardewValley;

/// <inheritdoc cref="StardewMods.EasyAccess.Interfaces.ManagedObjects.IManagedProducer" />
internal class ManagedProducer : GameObject, IManagedProducer
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ManagedProducer" /> class.
    /// </summary>
    /// <param name="producer">The producer.</param>
    /// <param name="data">The <see cref="IProducerData" /> for this type of producer.</param>
    /// <param name="qualifiedItemId">A unique Id associated with this producer type.</param>
    public ManagedProducer(IProducer producer, IProducerData data, string qualifiedItemId)
        : base(producer.Context)
    {
        this.Data = data;
        this.Producer = producer;
        this.QualifiedItemId = qualifiedItemId;

        foreach (var tag in this.CollectOutputItems)
        {
            this.ItemMatcherOut.Add(tag);
        }

        foreach (var tag in this.DispenseInputItems)
        {
            this.ItemMatcherIn.Add(tag);
        }
    }

    /// <inheritdoc />
    public int CollectOutputDistance
    {
        get => this.ModData.TryGetValue($"{EasyAccess.ModUniqueId}/CollectOutputDistance", out var value) && int.TryParse(value, out var distance)
            ? distance switch
            {
                0 => this.Data.CollectOutputDistance,
                _ => distance,
            }
            : this.Data.CollectOutputDistance;
        set => this.ModData[$"{EasyAccess.ModUniqueId}/CollectOutputDistance"] = value.ToString();
    }

    /// <inheritdoc />
    public HashSet<string> CollectOutputItems
    {
        get => this.ModData.TryGetValue($"{EasyAccess.ModUniqueId}/CollectOutputItems", out var value) && !string.IsNullOrWhiteSpace(value)
            ? new(this.Data.CollectOutputItems.Concat(value.Split(',')))
            : this.Data.CollectOutputItems;
        set => this.ModData[$"{EasyAccess.ModUniqueId}/CollectOutputItems"] = string.Join(",", value);
    }

    /// <inheritdoc />
    public FeatureOptionRange CollectOutputs
    {
        get => this.ModData.TryGetValue($"{EasyAccess.ModUniqueId}/CollectOutputs", out var value) && Enum.TryParse(value, out FeatureOptionRange range)
            ? range switch
            {
                FeatureOptionRange.Default => this.Data.CollectOutputs,
                _ => range,
            }
            : this.Data.CollectOutputs;
        set => this.ModData[$"{EasyAccess.ModUniqueId}/CollectOutputs"] = FormatHelper.GetRangeString(value);
    }

    /// <inheritdoc />
    public int DispenseInputDistance
    {
        get => this.ModData.TryGetValue($"{EasyAccess.ModUniqueId}/DispenseInputDistance", out var value) && int.TryParse(value, out var distance)
            ? distance switch
            {
                0 => this.Data.DispenseInputDistance,
                _ => distance,
            }
            : this.Data.DispenseInputDistance;
        set => this.ModData[$"{EasyAccess.ModUniqueId}/DispenseInputDistance"] = value.ToString();
    }

    /// <inheritdoc />
    public HashSet<string> DispenseInputItems
    {
        get => this.ModData.TryGetValue($"{EasyAccess.ModUniqueId}/DispenseInputItems", out var value) && !string.IsNullOrWhiteSpace(value)
            ? new(this.Data.DispenseInputItems.Concat(value.Split(',')))
            : this.Data.DispenseInputItems;
        set => this.ModData[$"{EasyAccess.ModUniqueId}/DispenseInputItems"] = string.Join(",", value);
    }

    /// <inheritdoc />
    public int DispenseInputPriority
    {
        get => this.ModData.TryGetValue($"{EasyAccess.ModUniqueId}/DispenseInputPriority", out var value) && int.TryParse(value, out var distance)
            ? distance switch
            {
                0 => this.Data.DispenseInputPriority,
                _ => distance,
            }
            : this.Data.DispenseInputPriority;
        set => this.ModData[$"{EasyAccess.ModUniqueId}/DispenseInputPriority"] = value.ToString();
    }

    /// <inheritdoc />
    public FeatureOptionRange DispenseInputs
    {
        get => this.ModData.TryGetValue($"{EasyAccess.ModUniqueId}/DispenseInputs", out var value) && Enum.TryParse(value, out FeatureOptionRange range)
            ? range switch
            {
                FeatureOptionRange.Default => this.Data.DispenseInputs,
                _ => range,
            }
            : this.Data.DispenseInputs;
        set => this.ModData[$"{EasyAccess.ModUniqueId}/DispenseInputs"] = FormatHelper.GetRangeString(value);
    }

    /// <inheritdoc />
    public ItemMatcher ItemMatcherIn { get; } = new(true);

    /// <inheritdoc />
    public ItemMatcher ItemMatcherOut { get; } = new(true);

    /// <inheritdoc />
    public override ModDataDictionary ModData
    {
        get => this.Producer.ModData;
    }

    /// <inheritdoc cref="IProducer.OutputItem" />
    public Item OutputItem
    {
        get => this.Producer.OutputItem;
    }

    /// <inheritdoc />
    public string QualifiedItemId { get; }

    private IProducerData Data { get; }

    private IProducer Producer { get; }

    /// <inheritdoc cref="IProducer.TryGetOutput" />
    public bool TryGetOutput(out Item item)
    {
        if (this.OutputItem is not null && this.ItemMatcherOut.Matches(this.OutputItem))
        {
            return this.Producer.TryGetOutput(out item);
        }

        item = null;
        return false;
    }

    /// <inheritdoc cref="IProducer.TrySetInput" />
    public bool TrySetInput(Item item)
    {
        return this.ItemMatcherIn.Matches(item) && this.Producer.TrySetInput(item);
    }
}