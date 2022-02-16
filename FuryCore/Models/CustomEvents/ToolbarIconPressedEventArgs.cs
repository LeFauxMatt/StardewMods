namespace StardewMods.FuryCore.Models.CustomEvents;

using System;
using StardewMods.FuryCore.Interfaces.MenuComponents;

/// <inheritdoc />
public class ToolbarIconPressedEventArgs : EventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ToolbarIconPressedEventArgs" /> class.
    /// </summary>
    /// <param name="component">The component which was pressed.</param>
    /// <param name="suppressInput">A method that will suppress the input.</param>
    /// <param name="isSuppressed">Indicates if the input is currently suppressed.</param>
    internal ToolbarIconPressedEventArgs(IMenuComponent component, Action suppressInput, Func<bool> isSuppressed)
    {
        this.Component = component;
        this.SuppressInput = suppressInput;
        this.IsSuppressed = isSuppressed;
    }

    /// <summary>
    ///     Gets the component which was pressed.
    /// </summary>
    public IMenuComponent Component { get; }

    /// <summary>
    ///     Gets a value indicating if the input is currently suppressed.
    /// </summary>
    public Func<bool> IsSuppressed { get; }

    /// <summary>
    ///     Gets an method that will suppress the input.
    /// </summary>
    public Action SuppressInput { get; }
}