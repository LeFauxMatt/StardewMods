namespace StardewMods.BetterChests;

using StardewMods.BetterChests.Storages;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

internal static class Extensions
{
    public static bool TryGetStorage(this Item item, out BaseStorage? storage)
    {
        switch (item)
        {
            case Chest chest:
                storage = new ChestStorage(chest);
                return true;
            case SObject { ParentSheetIndex: 165, heldObject.Value: Chest } heldObj:
                storage = new ObjectStorage(heldObj);
                return true;
            default:
                storage = default;
                return false;
        }
    }
}