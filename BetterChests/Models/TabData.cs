namespace BetterChests.Models;

using System.Collections.Generic;
using StardewValley.Menus;

/// <summary>
/// Custom tabs added to <see cref="ItemGrabMenu" /> which filter the currently displayed items.
/// </summary>
internal class TabData
{
    /// <summary>
    /// Gets or sets the tab name used in the localization key.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the context tags that define what items are shown on this tab.
    /// </summary>
    public List<string> Tags { get; set; }
}