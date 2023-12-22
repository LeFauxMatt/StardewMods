namespace StardewMods.FuryCore.Framework.Interfaces;

/// <summary>Handles palette swaps for theme compatibility.</summary>
public interface IThemeHelper
{
    /// <summary>Adds the specified asset names to the existing set of asset names.</summary>
    /// <param name="assetNames">The asset names to add.</param>
    public void AddAssets(params string[] assetNames);
}
