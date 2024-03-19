namespace StardewMods.FindAnything.Framework.Services.Finders;

using Microsoft.Xna.Framework;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.FindAnything;
using StardewMods.FindAnything.Framework.Models;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;

internal sealed class TerrainFinder
{
    public TerrainFinder(IEventSubscriber eventSubscriber) =>
        eventSubscriber.Subscribe<ISearchSubmitted>(this.OnSearchSubmitted);

    private void OnSearchSubmitted(ISearchSubmitted e)
    {
        foreach (var terrain in e.Location.terrainFeatures.Values)
        {
            if (this.TryGetResult(terrain, e.SearchTerm, out var result))
            {
                e.AddResult(result);
            }
        }

        foreach (var terrain in e.Location.largeTerrainFeatures)
        {
            if (this.TryGetResult(terrain, e.SearchTerm, out var result))
            {
                e.AddResult(result);
            }
        }
    }

    private bool TryGetResult(TerrainFeature terrain, string searchTerm, [NotNullWhen(true)] out ISearchResult? result)
    {
        switch (terrain)
        {
            case FruitTree fruitTree:
                var fruitTreeData = fruitTree.GetData();
                var displayName = TokenParser.ParseText(fruitTreeData?.DisplayName);
                if (displayName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                {
                    result = new SearchResult(displayName, Game1.mouseCursors, Rectangle.Empty, terrain.Tile);
                    return true;
                }

                break;
            case Tree tree:
                var treeData = tree.GetData();

                break;
            case Bush bush: break;
        }

        result = null;
        return false;
    }
}