namespace StardewMods.EasyAccess.Models.Config;

using StardewMods.EasyAccess.Enums;
using StardewMods.EasyAccess.Interfaces.Config;

/// <inheritdoc />
internal class ConfigData : IConfigData
{
    /// <inheritdoc />
    public ControlScheme ControlScheme { get; set; } = new();

    /// <inheritdoc />
    public ProducerData DefaultProducer { get; set; } = new()
    {
        CollectOutputs = FeatureOptionRange.Location,
        CollectOutputDistance = 15,
        DispenseInputs = FeatureOptionRange.Location,
        DispenseInputDistance = 15,
    };
}