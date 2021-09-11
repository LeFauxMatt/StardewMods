using StardewValley;

namespace XSLite
{
    internal static class CommonHelper
    {
        public static bool TryGetStorage(this Item item, out Storage storage)
        {
            if (!item.modData.TryGetValue($"{XSLite.ModPrefix}/Storage", out var storageName))
                storageName = item.Name;
            return XSLite.Storages.TryGetValue(storageName, out storage) && item.Category == -9;
        }
    }
}