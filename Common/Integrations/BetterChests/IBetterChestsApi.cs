namespace Common.Integrations.BetterChests;

/// <summary>
/// API for Better Chests.
/// </summary>
public interface IBetterChestsApi
{
    /// <summary>
    /// Registers an Item as a chest based on its name.
    /// </summary>
    /// <param name="name">The name of the chest to register.</param>
    /// <returns>True if the data was successfully saved.</returns>
    public bool RegisterChest(string name);
}