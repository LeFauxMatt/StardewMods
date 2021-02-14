using System;
using System.Collections.Generic;
using System.Linq;
using MoreCraftables.API;
using MoreCraftables.Framework;
using MoreCraftables.Framework.HandledObjects;
using MoreCraftables.Framework.HandledObjects.BigCraftables;
using MoreCraftables.Framework.HandledObjects.Furniture;
using MoreCraftables.Framework.HandledObjects.Objects;
using MoreCraftables.Framework.Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace MoreCraftables
{
    public class MoreCraftablesAPI : IMoreCraftablesAPI
    {
        private readonly IList<string> _contentDirs = new List<string>();
        private readonly IDictionary<string, IHandledObject> _handledObjects;
        private readonly IModHelper _helper;
        private readonly IMonitor _monitor;

        private IJsonAssetsAPI _jsonAssetsApi;

        public MoreCraftablesAPI(IModHelper helper, IMonitor monitor, IDictionary<string, IHandledObject> handledObjects)
        {
            _helper = helper;
            _monitor = monitor;
            _handledObjects = handledObjects;

            // Events
            _helper.Events.GameLoop.GameLaunched += OnGameLaunched;
        }

        public event EventHandler ReadyToLoad;
        public event EventHandler IdsLoaded;

        public void AddHandledObject(string name, IHandledObject handledObject)
        {
            _monitor.Log($"Adding Handled Object {name} of Type {handledObject.Type}");
            handledObject.Base = _handledObjects.FirstOrDefault(h => h.Value.Type.Equals(handledObject.Type)).Value;
            _handledObjects.Add(name, handledObject);
        }

        public IDictionary<string, int> GetAllObjectIds()
        {
            return _handledObjects
                .Where(h => h.Value is HandledObject)
                .ToDictionary(h => h.Key, h => ((GenericHandledObject) h.Value).ObjectId);
        }

        public IDictionary<string, int> GetAllBigCraftableIds()
        {
            return _handledObjects
                .Where(h => h.Value is HandledBigCraftable)
                .ToDictionary(h => h.Key, h => ((GenericHandledObject) h.Value).ObjectId);
        }

        public IDictionary<string, int> GetAllFurnitureIds()
        {
            return _handledObjects
                .Where(h => h.Value is HandledFurniture)
                .ToDictionary(h => h.Key, h => ((GenericHandledObject) h.Value).ObjectId);
        }

        public int GetBigCraftableId(string name)
        {
            return GetAllBigCraftableIds().TryGetValue(name, out var bigCraftableId) ? bigCraftableId : 0;
        }

        public int GetObjectId(string name)
        {
            return GetAllObjectIds().TryGetValue(name, out var objectId) ? objectId : 0;
        }

        public int GetFurnitureId(string name)
        {
            return GetAllFurnitureIds().TryGetValue(name, out var furnitureId) ? furnitureId : 0;
        }


        public bool LoadContentPack(string path)
        {
            var temp = _helper.ContentPacks.CreateFake(path);
            var info = temp.ReadJsonFile<ContentPack>("content-pack.json");

            if (info == null)
            {
                _monitor.Log($"Cannot read content-data.json from {path}", LogLevel.Warn);
                return false;
            }
            
            var contentPack = _helper.ContentPacks.CreateTemporary(
                path,
                info.UniqueID,
                info.Name,
                info.Description,
                info.Author,
                new SemanticVersion(info.Version));

            return LoadContentPack(contentPack);
        }

        public bool LoadContentPack(IContentPack contentPack)
        {
            _monitor.Log($"Loading {contentPack.Manifest.Name} {contentPack.Manifest.Version}", LogLevel.Info);
            var contentData = contentPack.ReadJsonFile<IDictionary<string, GenericHandledObject>>("moreCraftables.json");

            foreach (var item in contentData)
            {
                if (item.Value.Type == null || !Enum.TryParse(item.Value.Type, out GenericHandledObject.ItemType itemType))
                {
                    _monitor.Log($"Cannot load {item.Key} from {contentPack.Manifest.Name} {contentPack.Manifest.Version}", LogLevel.Warn);
                    continue;
                }
                
                switch (itemType)
                {
                    case GenericHandledObject.ItemType.Cask:
                        AddHandledObject(item.Key, new HandledCask(item.Value));
                        continue;
                    case GenericHandledObject.ItemType.Chest:
                        AddHandledObject(item.Key, new HandledChest(item.Value));
                        continue;
                    case GenericHandledObject.ItemType.Fence:
                        AddHandledObject(item.Key, new HandledFence(item.Value));
                        continue;
                    default:
                        _monitor.Log($"Cannot load {item.Key} from {contentPack.Manifest.Name} {contentPack.Manifest.Version}", LogLevel.Warn);
                        continue;
                }
            }

            _contentDirs.Add(contentPack.DirectoryPath);
            return true;
        }

        /// <summary>Load More Craftables content packs</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            _jsonAssetsApi = _helper.ModRegistry.GetApi<IJsonAssetsAPI>("spacechase0.JsonAssets");
            _jsonAssetsApi.IdsAssigned += OnIdsAssigned;
            _helper.Events.GameLoop.UpdateTicked += OnUpdateTicked;
        }

        /// <summary>Load Json Asset Ids.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            _helper.Events.GameLoop.UpdateTicked -= OnUpdateTicked;
            _monitor.Log("ReadyToLoad");
            InvokeAll(ReadyToLoad);
            foreach (var contentDir in _contentDirs)
                _jsonAssetsApi.LoadAssets(contentDir);
        }

        /// <summary>Load Json Asset Ids.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnIdsAssigned(object sender, EventArgs e)
        {
            var bigCraftableIds = _jsonAssetsApi.GetAllBigCraftableIds();
            var objectIds = _jsonAssetsApi.GetAllObjectIds();
            var furnitureData = Game1.content.Load<Dictionary<int, string>>("Data\\Furniture")
                .ToDictionary(d => d.Key, d => d.Value.Split('/'));
            foreach (var obj in _handledObjects)
            {
                switch (obj.Value)
                {
                    case HandledBigCraftable handledBigCraftable when bigCraftableIds.TryGetValue(obj.Key, out var bigCraftableId):
                        handledBigCraftable.ObjectId = bigCraftableId;
                        break;
                    case HandledObject handledObject when objectIds.TryGetValue(obj.Key, out var objectId):
                        handledObject.ObjectId = objectId;
                        break;
                    case HandledFurniture handledFurniture:
                        var data = furnitureData.FirstOrDefault(f => f.Value[0].Equals(obj.Key, StringComparison.InvariantCultureIgnoreCase));
                        if (data.Value != null)
                            handledFurniture.ObjectId = data.Key;
                        break;
                }
            }
            _monitor.Log("IdsAssigned");
            InvokeAll(IdsLoaded);
        }

        private void InvokeAll(EventHandler eventHandler)
        {
            if (eventHandler == null)
                return;

            foreach (var @delegate in eventHandler.GetInvocationList()) @delegate.DynamicInvoke(this, null);
        }
    }
}