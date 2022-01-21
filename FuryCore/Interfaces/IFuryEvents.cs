namespace FuryCore.Interfaces;

using System;
using FuryCore.Models;
using StardewModdingAPI.Events;

/// <summary>
/// </summary>
public interface IFuryEvents
{
    /// <summary>
    /// </summary>
    public event EventHandler<ItemGrabMenuChangedEventArgs> ItemGrabMenuChanged;

    /// <summary>
    /// </summary>
    public event EventHandler<MenuComponentPressedEventArgs> MenuComponentPressed;

    /// <summary>
    /// </summary>
    public event EventHandler<RenderedActiveMenuEventArgs> RenderedItemGrabMenu;

    /// <summary>
    /// </summary>
    public event EventHandler<RenderingActiveMenuEventArgs> RenderingItemGrabMenu;
}