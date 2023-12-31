namespace StardewMods.GarbageDay.Framework.Models;

using Microsoft.Xna.Framework;

/// <summary>Represents a pending garbage can object on a map.</summary>
/// <param name="whichCan">The name of the garbage can.</param>
/// <param name="assetName">The asset name of the map containing the garbage can.</param>
/// <param name="x">The x-coordinate of the garbage can.</param>
/// <param name="y">The y-coordinate of the garbage can.</param>
internal sealed class FoundGarbageCan(string whichCan, IAssetName assetName, int x, int y)
{
    /// <summary>Gets the asset name of the map containing the garbage can.</summary>
    public IAssetName AssetName { get; } = assetName;

    /// <summary>Gets the tile position of the garbage can.</summary>
    public Vector2 TilePosition { get; } = new(x, y);

    /// <summary>Gets the name of the garbage can.</summary>
    public string WhichCan { get; } = whichCan;
}