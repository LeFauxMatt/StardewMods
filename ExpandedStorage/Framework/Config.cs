namespace StardewMods.ExpandedStorage.Framework;

using System.Collections.Generic;
using System.Linq;
using StardewMods.Common.Integrations.ExpandedStorage;
using StardewMods.Common.Integrations.GenericModConfigMenu;
using StardewMods.ExpandedStorage.Models;

/// <summary>
///     Config helper for Expanded Storage.
/// </summary>
internal sealed class Config
{
#nullable disable
    private static Config Instance;
#nullable enable
    private readonly ModConfig _config;

    private readonly IModHelper _helper;
    private readonly IManifest _manifest;

    private Config(IModHelper helper, IManifest manifest)
    {
        this._helper = helper;
        this._manifest = manifest;
        this._config = this._helper.ReadConfig<ModConfig>();
    }

    private static IGenericModConfigMenuApi GMCM => Integrations.GenericModConfigMenu.API!;

    private static ModConfig ModConfig => Config.Instance._config;

    private static IManifest ModManifest => Config.Instance._manifest;

    /// <summary>
    ///     Initializes <see cref="Config" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="manifest">A manifest to describe the mod.</param>
    /// <returns>Returns an instance of the <see cref="Config" /> class.</returns>
    public static Config Init(IModHelper helper, IManifest manifest)
    {
        return Config.Instance ??= new(helper, manifest);
    }

    /// <summary>
    ///     Setup Generic Mod Config Options menu.
    /// </summary>
    /// <param name="storages">The storages to add to the mod config menu.</param>
    public static void SetupConfig(IDictionary<string, ICustomStorage> storages)
    {

        var configStorages = storages
                             .Where(storage => storage.Value.PlayerConfig)
                             .OrderBy(storage => storage.Value.DisplayName)
                             .ToArray();

        foreach (var (id, storage) in configStorages)
        {
            if (storage.BetterChestsData is not BetterChestsData betterChestsData)
            {
                continue;
            }

            if (!Config.ModConfig.Config.TryGetValue(id, out var config))
            {
                config = new();
                Config.ModConfig.Config.Add(id, config);
            }

            config.BetterChestsData = betterChestsData;
        }

        if (!Integrations.GenericModConfigMenu.IsLoaded)
        {
            return;
        }

        Integrations.GenericModConfigMenu.Register(Config.ModManifest, Config.Reset, Config.Save);

        if (!Integrations.BetterChests.IsLoaded)
        {
            return;
        }

        foreach (var (id, storage) in configStorages)
        {
            if (storage.BetterChestsData is null)
            {
                continue;
            }

            Config.GMCM.AddPageLink(Config.ModManifest, id, () => storage.DisplayName, () => storage.Description);
        }

        foreach (var (id, storage) in configStorages)
        {
            if (storage.BetterChestsData is null)
            {
                continue;
            }

            Config.GMCM.AddPage(Config.ModManifest, id, () => storage.DisplayName);
            Integrations.BetterChests.API.AddConfigOptions(Config.ModManifest, storage.BetterChestsData);
        }
    }

    private static void Reset()
    {
        var config = new ModConfig();
        config.CopyTo(Config.ModConfig);
    }

    private static void Save()
    {
        Config.Instance._helper.WriteConfig(Config.ModConfig);
    }
}