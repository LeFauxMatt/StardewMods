namespace StardewMods.EasyAccess;

using StardewModdingAPI.Utilities;

/// <summary>
///     Controls config data.
/// </summary>
internal class Controls
{
    /// <summary>
    ///     Gets or sets controls to collect items from producers.
    /// </summary>
    public KeybindList CollectItems { get; set; } = new(SButton.Delete);

    /// <summary>
    ///     Gets or sets controls to dispense items into producers.
    /// </summary>
    public KeybindList DispenseItems { get; set; } = new(SButton.Insert);
}