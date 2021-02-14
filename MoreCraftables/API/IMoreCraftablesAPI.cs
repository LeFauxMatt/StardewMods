using System;
using System.Collections.Generic;
using StardewModdingAPI;

namespace MoreCraftables.API
{
    public interface IMoreCraftablesAPI
    {
        void AddHandledObject(string name, IHandledObject handledObject);
        IDictionary<string, int> GetAllObjectIds();
        IDictionary<string, int> GetAllBigCraftableIds();
        IDictionary<string, int> GetAllFurnitureIds();
        int GetBigCraftableId(string name);
        int GetObjectId(string name);
        int GetFurnitureId(string name);
        bool LoadContentPack(string path);
        bool LoadContentPack(IContentPack contentPack);
        event EventHandler ReadyToLoad;
        event EventHandler IdsLoaded;
    }
}