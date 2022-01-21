namespace FuryCore.Models;

using System;

/// <inheritdoc />
public class MenuComponentPressedEventArgs : EventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MenuComponentPressedEventArgs" /> class.
    /// </summary>
    /// <param name="component"></param>
    /// <param name="suppressInput"></param>
    /// <param name="isSuppressed"></param>
    public MenuComponentPressedEventArgs(MenuComponent component, Action suppressInput, Func<bool> isSuppressed)
    {
        this.Component = component;
        this.SuppressInput = suppressInput;
        this.IsSuppressed = isSuppressed;
    }

    public MenuComponent Component { get; }

    public Action SuppressInput { get; }

    public Func<bool> IsSuppressed { get; }
}