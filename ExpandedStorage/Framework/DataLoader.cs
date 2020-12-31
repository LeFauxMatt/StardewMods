using System;
using System.IO;
using StardewModdingAPI;

namespace ExpandedStorage
{
    internal class DataLoader
    {
        private static IModHelper Helper;

        public static void Init(IModHelper helper, IJsonAsssetsApi jsonAssetsApi)
        {
            Helper = helper;
            foreach (var contentPack in Helper.ContentPacks.GetOwned())
            {
                
            }
            jsonAssetsApi.LoadAssets(Path.Combine(Helper.DirectoryPath, "assets"));
        }
    }
}