#nullable disable

namespace StardewMods.FuryCore.Models.CustomEvents;

using System;
using StardewModdingAPI;
using StardewMods.FuryCore.Interfaces.ClickableComponents;
using StardewMods.FuryCore.Interfaces.CustomEvents;

/// <inheritdoc cref="StardewMods.FuryCore.Interfaces.CustomEvents.IClickableComponentPressedEventArgs" />
public class ClickableComponentPressedEventArgs : EventArgs, IClickableComponentPressedEventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickableComponentPressedEventArgs" /> class.
    /// </summary>
    /// <param name="button">The button that was pressed.</param>
    /// <param name="component">The component which was pressed.</param>
    /// <param name="suppressInput">A method that will suppress the input.</param>
    /// <param name="isSuppressed">Indicates if the input is currently suppressed.</param>
    internal ClickableComponentPressedEventArgs(SButton button, IClickableComponent component, Action suppressInput, Func<bool> isSuppressed)
    {
        this.Button = button;
        this.Component = component;
        this.SuppressInput = suppressInput;
        this.IsSuppressed = isSuppressed;
    }

    /// <inheritdoc />
    public SButton Button { get; }

    /// <inheritdoc />
    public IClickableComponent Component { get; }

    /// <inheritdoc />
    public Func<bool> IsSuppressed { get; }

    /// <inheritdoc />
    public Action SuppressInput { get; }
}