namespace StardewMods.FuryCore.Interfaces.ClickableComponents;

/// <summary>
///     Adds icons above/below the items toolbar.
/// </summary>
public interface IHudComponents
{
    /// <summary>
    ///     Add a component to the toolbar.
    /// </summary>
    /// <param name="component">The component to add.</param>
    /// <param name="index">The index to add the component at.</param>
    public void AddToolbarIcon(IClickableComponent component, int index = -1);

    /// <summary>
    ///     Remove a component from the toolbar.
    /// </summary>
    /// <param name="component">The component to remove.</param>
    public void RemoveToolbarIcon(IClickableComponent component);
}