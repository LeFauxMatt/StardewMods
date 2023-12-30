namespace StardewMods.TooManyAnimals.Framework.Interfaces;

using StardewMods.TooManyAnimals.Framework.Models;

/// <summary>Mod config data for Too Many Animals.</summary>
internal interface IModConfig
{
    /// <summary>Gets a value indicating how many animals will be shown in the Animal Purchase menu at once.</summary>
    public int AnimalShopLimit { get; }

    /// <summary>Gets the control scheme.</summary>
    public Controls ControlScheme { get; }
}