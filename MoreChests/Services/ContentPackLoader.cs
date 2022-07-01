#nullable disable

namespace MoreChests.Services;

using System.Collections.Generic;
using System.Linq;
using Common.Helpers;
using Common.Integrations.DynamicGameAssets;
using Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;

internal class ContentPackLoader : BaseService
{
    private readonly IEnumerable<IContentPack> _contentPacks;
    private readonly DynamicGameAssetsIntegration _dynamicGameAssets;
    private CustomChestManager _customChestManager;
    private ModConfigService _modConfig;

    private ContentPackLoader(ServiceLocator serviceLocator)
        : base("ContentPackLoader")
    {
        // Init
        this._contentPacks = serviceLocator.Helper.ContentPacks.GetOwned();
        this._dynamicGameAssets = new(serviceLocator.Helper.ModRegistry);

        // Dependencies
        this.AddDependency<CustomChestManager>(service => this._customChestManager = service as CustomChestManager);
        this.AddDependency<ModConfigService>(service => this._modConfig = service as ModConfigService);

        // Events
        serviceLocator.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    public bool LoadContentPack(IContentPack contentPack)
    {
        Log.Info($"Loading {contentPack.Manifest.Name} {contentPack.Manifest.Version}");

        var chestData = contentPack.ReadJsonFile<IDictionary<string, ChestData>>("expanded-storage.json");
        if (chestData is null)
        {
            Log.Warn($"Nothing to load from {contentPack.Manifest.Name} {contentPack.Manifest.Version}");
            return false;
        }

        // Remove any duplicate storages
        foreach (var name in chestData.Keys.Where(this._customChestManager.Exists))
        {
            Log.Warn($"Duplicate chest {name} in {contentPack.Manifest.UniqueID}.");
            chestData.Remove(name);
        }

        if (chestData.Count == 0)
        {
            Log.Warn($"Nothing to load from {contentPack.Manifest.Name} {contentPack.Manifest.Version}");
            return false;
        }

        // Register/load configs
        if (this._modConfig.RegisterNew(contentPack))
        {
            this._modConfig.AddChests(contentPack, chestData.Where(data => data.Value.PlayerConfig));
        }

        // Load chest data
        this._customChestManager.AddChests(contentPack, chestData);

        foreach (var data in chestData)
        {
            data.Value.DisplayName = contentPack.Translation.Get($"big-craftable.{data.Key}.name");
            data.Value.Description = contentPack.Translation.Get($"big-craftable.{data.Key}.description");
        }

        if (!this._dynamicGameAssets.IsLoaded)
        {
            return true;
        }

        var manifest = new FakeManifest(contentPack.Manifest)
        {
            ContentPackFor = new ContentPackFor
            {
                UniqueID = "spacechase0.DynamicGameAsset",
                MinimumVersion = null,
            },
            ExtraFields = new Dictionary<string, object>
            {
                {
                    "DGA.FormatVersion", "2"
                },
                {
                    "DGA.ConditionsFormatVersion", "1.23.0"
                },
            },
        };

        this._dynamicGameAssets.API.AddEmbeddedPack(manifest, contentPack.DirectoryPath);

        return true;
    }

    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        Log.Info("Loading Content Packs for More Chests");
        foreach (var contentPack in this._contentPacks)
        {
            this.LoadContentPack(contentPack);
        }
    }
}