#nullable disable

namespace StardewMods.EasyAccess.Interfaces.Config;

using StardewModdingAPI.Utilities;

/// <summary>
///     Controls config data.
/// </summary>
internal interface IControlScheme
{
    /// <summary>
    ///     Gets or sets controls to collect items from producers.
    /// </summary>
    public KeybindList CollectItems { get; set; }

    /// <summary>
    ///     Gets or sets controls to dispense items into producers.
    /// </summary>
    public KeybindList DispenseItems { get; set; }

    /// <summary>
    ///     Copies data from one <see cref="IConfigData" /> to another.
    /// </summary>
    /// <param name="other">The <see cref="IConfigData" /> to copy values to.</param>
    /// <typeparam name="TOther">The class/type of the other <see cref="IConfigData" />.</typeparam>
    public void CopyTo<TOther>(TOther other)
        where TOther : IControlScheme
    {
        other.CollectItems = this.CollectItems;
        other.DispenseItems = this.DispenseItems;
    }
}