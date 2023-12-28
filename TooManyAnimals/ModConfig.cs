namespace StardewMods.TooManyAnimals;

using StardewMods.Common.Enums;
using StardewMods.Common.Services.Integrations.FuryCore;

/// <summary>Mod config data for Too Many Animals.</summary>
internal sealed class ModConfig : IConfigWithLogLevel
{
    /// <summary>Gets or sets a value indicating how many animals will be shown in the Animal Purchase menu at once.</summary>
    public int AnimalShopLimit { get; set; } = 30;

    /// <summary>Gets or sets the control scheme.</summary>
    public Controls ControlScheme { get; set; } = new();

    /// <inheritdoc />
    public SimpleLogLevel LogLevel { get; set; } = SimpleLogLevel.Less;
}