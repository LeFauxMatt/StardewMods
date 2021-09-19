namespace XSLite
{
    using StardewValley;

    internal static class CommonHelper
    {
        public static bool TryGetStorage(this Item item, out Storage storage)
        {
            if (!item.modData.TryGetValue($"{XSLite.ModPrefix}/Storage", out string storageName))
            {
                storageName = item.Name;
            }

            return XSLite.Storages.TryGetValue(storageName, out storage) && item.Category == -9;
        }
    }
}