namespace StardewMods.EasyAccess.Models.Config;

using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewMods.EasyAccess.Interfaces.Config;

/// <inheritdoc />
internal class ControlScheme : IControlScheme
{
    /// <inheritdoc />
    public KeybindList CollectItems { get; set; } = new(SButton.Delete);

    /// <inheritdoc />
    public KeybindList DispenseItems { get; set; } = new(SButton.Insert);
}