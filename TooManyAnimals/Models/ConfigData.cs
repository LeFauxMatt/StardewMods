#nullable disable

namespace StardewMods.TooManyAnimals.Models;

using StardewMods.TooManyAnimals.Interfaces;

/// <inheritdoc />
internal class ConfigData : IConfigData
{
    /// <inheritdoc />
    public int AnimalShopLimit { get; set; } = 30;

    /// <inheritdoc />
    public ControlScheme ControlScheme { get; set; } = new();
}