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
    private static Config instance;
#nullable enable
    private readonly ModConfig config;

    private readonly IModHelper helper;
    private readonly IManifest manifest;

    private Config(IModHelper helper, IManifest manifest)
    {
        this.helper = helper;
        this.manifest = manifest;
        this.config = this.helper.ReadConfig<ModConfig>();
    }

    private static IGenericModConfigMenuApi GMCM => Integrations.GenericModConfigMenu.Api!;

    private static ModConfig ModConfig => Config.instance.config;

    private static IManifest ModManifest => Config.instance.manifest;

    /// <summary>
    ///     Gets config data for an Expanded Storage chest type.
    /// </summary>
    /// <param name="id">The id of the config to get.</param>
    /// <returns>Returns storage config data.</returns>
    public static StorageConfig? GetConfig(string id) => Config.ModConfig.Config.TryGetValue(id, out var config) ? config : null;

    /// <summary>
    ///     Initializes <see cref="Config" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="manifest">A manifest to describe the mod.</param>
    /// <returns>Returns an instance of the <see cref="Config" /> class.</returns>
    public static Config Init(IModHelper helper, IManifest manifest) => Config.instance ??= new(helper, manifest);

    /// <summary>
    ///     Setup Generic Mod Config Options menu.
    /// </summary>
    /// <param name="storages">The storages to add to the mod config menu.</param>
    public static void SetupConfig(IDictionary<string, ICustomStorage> storages)
    {
        var configStorages = storages.Where(storage => storage.Value.PlayerConfig)
            .OrderBy(storage => storage.Value.DisplayName)
            .ToArray();

        foreach (var (id, storage) in configStorages)
        {
            if (Config.ModConfig.Config.TryGetValue(id, out var config))
            {
                continue;
            }

            config = new() { BetterChestsData = new() };
            storage.BetterChestsData?.CopyTo(config.BetterChestsData);
            Config.ModConfig.Config.Add(id, config);
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

            var config = Config.GetConfig(id);
            if (config?.BetterChestsData is null)
            {
                continue;
            }

            Config.GMCM.AddPage(Config.ModManifest, id, () => storage.DisplayName);
            Integrations.BetterChests.Api.AddConfigOptions(Config.ModManifest, config.BetterChestsData);
        }
    }

    private static void Reset()
    {
        var config = new ModConfig();
        config.CopyTo(Config.ModConfig);
    }

    private static void Save() => Config.instance.helper.WriteConfig(Config.ModConfig);
}
