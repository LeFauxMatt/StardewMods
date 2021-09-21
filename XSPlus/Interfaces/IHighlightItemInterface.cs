namespace XSPlus.Interfaces
{
    using StardewValley;

    /// <summary>
    /// Interface for classes that can handle a HighlightItemsEvent.
    /// </summary>
    internal interface IHighlightItemInterface
    {
        /// <summary>Highlight an item based on a set of internally managed conditions.</summary>
        /// <param name="item">The item to check.</param>
        /// <returns>Returns true if item should be highlighted.</returns>
        bool HighlightMethod(Item item);
    }
}