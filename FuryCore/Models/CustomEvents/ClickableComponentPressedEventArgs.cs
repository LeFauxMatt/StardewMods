namespace StardewMods.FuryCore.Models.CustomEvents;

using System;
using StardewModdingAPI;
using StardewMods.FuryCore.Interfaces.ClickableComponents;

/// <inheritdoc />
public class ClickableComponentPressedEventArgs : EventArgs
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

    public SButton Button { get; }

    /// <summary>
    ///     Gets the component which was pressed.
    /// </summary>
    public IClickableComponent Component { get; }

    /// <summary>
    ///     Gets a value indicating if the input is currently suppressed.
    /// </summary>
    public Func<bool> IsSuppressed { get; }

    /// <summary>
    ///     Gets an method that will suppress the input.
    /// </summary>
    public Action SuppressInput { get; }
}