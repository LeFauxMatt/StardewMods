namespace StardewMods.TooManyAnimals.Framework.Models;

using StardewMods.TooManyAnimals.Framework.Interfaces;

/// <inheritdoc />
internal sealed class DefaultConfig : IModConfig
{
    /// <inheritdoc />
    public int AnimalShopLimit { get; set; } = 30;

    /// <inheritdoc />
    public Controls ControlScheme { get; set; } = new();
}