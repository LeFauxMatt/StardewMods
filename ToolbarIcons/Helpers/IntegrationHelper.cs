namespace StardewMods.ToolbarIcons.Helpers;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.Common.Integrations.ToolbarIcons;
using StardewValley;
using StardewValley.Menus;

/// <summary>
///     Handles integrations with other mods.
/// </summary>
internal class IntegrationHelper
{
    private const string AlwaysScrollMapId = "bcmpinc.AlwaysScrollMap";
    private const string ChestsAnywhereId = "Pathoschild.ChestsAnywhere";
    private const string CJBCheatsMenuId = "CJBok.CheatsMenu";
    private const string CJBItemSpawnerId = "CJBok.ItemSpawner";
    private const string DynamicGameAssetsId = "spacechase0.DynamicGameAssets";
    private const string InstantBuildingId = "BitwiseJonMods.InstantBuildings";
    private const string LookupAnythingId = "Pathoschild.LookupAnything";
    private const string StardewAquariumId = "Cherry.StardewAquarium";

    private IReflectedField<bool>? _alwaysScrollMapEnabledIndoors;
    private IReflectedField<bool>? _alwaysScrollMapEnabledOutdoors;
    private IReflectedMethod? _chestsAnywhereOpenMenu;
    private IReflectedMethod? _cjbCheatsMenuOpenCheatsMenu;
    private IReflectedMethod? _cjbItemSpawnerBuildMenu;
    private IReflectedMethod? _dynamicGameAssetsStoreCommand;
    private IReflectedMethod? _instantBuildingsInstantBuild;
    private IReflectedMethod? _instantBuildingsInstantUpgrade;
    private IReflectedMethod? _lookupAnythingTryToggleSearch;
    private IReflectedMethod? _stardewAquariumOpenCollectionsMenu;

    private IntegrationHelper(IModHelper helper, IToolbarIconsApi api)
    {
        this.Helper = helper;
        this.API = api;

        // Events
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        this.API.ToolbarIconPressed += this.OnToolbarIconPressed;
    }

    private static IntegrationHelper? Instance { get; set; }

    private IToolbarIconsApi API { get; }

    private IModHelper Helper { get; }

    private Dictionary<string, Action> Icons { get; } = new();

    /// <summary>
    ///     Initializes <see cref="IntegrationHelper" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="api">API to add icons above or below the toolbar.</param>
    /// <returns>Returns an instance of the <see cref="IntegrationHelper" /> class.</returns>
    public static IntegrationHelper Init(IModHelper helper, IToolbarIconsApi api)
    {
        return IntegrationHelper.Instance ??= new(helper, api);
    }

    private void AlwaysScrollMap_Toggle()
    {
        void Toggle()
        {
            if (Game1.currentLocation.IsOutdoors)
            {
                this._alwaysScrollMapEnabledOutdoors?.SetValue(!this._alwaysScrollMapEnabledOutdoors.GetValue());
            }
            else
            {
                this._alwaysScrollMapEnabledIndoors?.SetValue(!this._alwaysScrollMapEnabledIndoors.GetValue());
            }
        }

        if (this._alwaysScrollMapEnabledIndoors is not null && this._alwaysScrollMapEnabledOutdoors is not null)
        {
            Toggle();
            return;
        }

        if (this.TryGetMod(IntegrationHelper.AlwaysScrollMapId, out var mod))
        {
            var config = mod.GetType().GetField("config")?.GetValue(mod);
            if (config is null)
            {
                return;
            }

            this._alwaysScrollMapEnabledIndoors = this.Helper.Reflection.GetField<bool>(config, "EnabledIndoors");
            this._alwaysScrollMapEnabledOutdoors = this.Helper.Reflection.GetField<bool>(config, "EnabledOutdoors");
            Toggle();
        }
    }

    private void ChestsAnywhere_OpenMenu()
    {
        if (this._chestsAnywhereOpenMenu is not null)
        {
            this._chestsAnywhereOpenMenu.Invoke();
            return;
        }

        if (this.TryGetMod(IntegrationHelper.ChestsAnywhereId, out var mod))
        {
            this._chestsAnywhereOpenMenu = this.Helper.Reflection.GetMethod(mod, "OpenMenu");
            this._chestsAnywhereOpenMenu.Invoke();
        }
    }

    private void CJBCheatsMenu_OpenCheatsMenu()
    {
        if (this._cjbCheatsMenuOpenCheatsMenu is not null)
        {
            this._cjbCheatsMenuOpenCheatsMenu.Invoke(0, true);
            return;
        }

        if (this.TryGetMod(IntegrationHelper.CJBCheatsMenuId, out var mod))
        {
            this._cjbCheatsMenuOpenCheatsMenu = this.Helper.Reflection.GetMethod(mod, "OpenCheatsMenu");
            this._cjbCheatsMenuOpenCheatsMenu.Invoke(0, true);
        }
    }

    private void CJBItemSpawner_ShowMenu()
    {
        void ShowMenu()
        {
            Game1.activeClickableMenu = this._cjbItemSpawnerBuildMenu.Invoke<ItemGrabMenu>();
        }

        if (this._cjbItemSpawnerBuildMenu is not null)
        {
            ShowMenu();
            return;
        }

        if (this.TryGetMod(IntegrationHelper.CJBItemSpawnerId, out var mod))
        {
            this._cjbItemSpawnerBuildMenu = this.Helper.Reflection.GetMethod(mod, "BuildMenu");
            ShowMenu();
        }
    }

    private void DynamicGameAssets_StoreCommand()
    {
        if (this._dynamicGameAssetsStoreCommand is not null)
        {
            this._dynamicGameAssetsStoreCommand.Invoke("dga_store", Array.Empty<string>());
            return;
        }

        if (this.TryGetMod(IntegrationHelper.DynamicGameAssetsId, out var mod))
        {
            this._dynamicGameAssetsStoreCommand = this.Helper.Reflection.GetMethod(mod, "OnStoreCommand");
            this._dynamicGameAssetsStoreCommand.Invoke("dga_store", Array.Empty<string>());
        }
    }

    private void InstantBuildings_InstantBuild()
    {
        if (this._instantBuildingsInstantBuild is not null)
        {
            this._instantBuildingsInstantBuild.Invoke();
            return;
        }

        if (this.TryGetMod(IntegrationHelper.InstantBuildingId, out var mod))
        {
            this._instantBuildingsInstantBuild = this.Helper.Reflection.GetMethod(mod, "HandleInstantBuildButtonClick");
            this._instantBuildingsInstantBuild.Invoke();
        }
    }

    private void InstantBuildings_InstantUpgrade()
    {
        if (this._instantBuildingsInstantUpgrade is not null)
        {
            this._instantBuildingsInstantUpgrade.Invoke();
            return;
        }

        if (this.TryGetMod(IntegrationHelper.InstantBuildingId, out var mod))
        {
            this._instantBuildingsInstantUpgrade = this.Helper.Reflection.GetMethod(mod, "HandleInstantUpgradeButtonClick");
            this._instantBuildingsInstantUpgrade.Invoke();
        }
    }

    private void LookupAnything_TryToggleSearch()
    {
        if (this._lookupAnythingTryToggleSearch is not null)
        {
            this._lookupAnythingTryToggleSearch.Invoke();
            return;
        }

        if (this.TryGetMod(IntegrationHelper.LookupAnythingId, out var mod))
        {
            this._lookupAnythingTryToggleSearch = this.Helper.Reflection.GetMethod(mod, "TryToggleSearch");
            this._lookupAnythingTryToggleSearch.Invoke();
        }
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        this.TryAdd(IntegrationHelper.AlwaysScrollMapId, 2, this.AlwaysScrollMap_Toggle);
        this.TryAdd(IntegrationHelper.ChestsAnywhereId, 3, this.ChestsAnywhere_OpenMenu);
        this.TryAdd(IntegrationHelper.CJBCheatsMenuId, 4, this.CJBCheatsMenu_OpenCheatsMenu);
        this.TryAdd(IntegrationHelper.CJBItemSpawnerId, 5, this.CJBItemSpawner_ShowMenu);
        this.TryAdd(IntegrationHelper.DynamicGameAssetsId, 6, this.DynamicGameAssets_StoreCommand);
        this.TryAdd(IntegrationHelper.InstantBuildingId, 7, this.InstantBuildings_InstantBuild, this.Helper.Translation.Get($"button.{IntegrationHelper.InstantBuildingId}.Build"));
        this.TryAdd(IntegrationHelper.InstantBuildingId, 8, this.InstantBuildings_InstantUpgrade, this.Helper.Translation.Get($"button.{IntegrationHelper.InstantBuildingId}.Upgrade"));
        this.TryAdd(IntegrationHelper.LookupAnythingId, 9, this.LookupAnything_TryToggleSearch);
        this.TryAdd(IntegrationHelper.StardewAquariumId, 1, this.StardewAquarium_OpenCollectionsMenu);
    }

    private void OnToolbarIconPressed(object? sender, string id)
    {
        if (this.Icons.TryGetValue(id, out var action))
        {
            try
            {
                action.Invoke();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }

    private void StardewAquarium_OpenCollectionsMenu()
    {
        if (this._stardewAquariumOpenCollectionsMenu is not null)
        {
            this._stardewAquariumOpenCollectionsMenu.Invoke("aquariumprogress", Array.Empty<string>());
            return;
        }

        if (this.TryGetMod(IntegrationHelper.StardewAquariumId, out var mod))
        {
            this._stardewAquariumOpenCollectionsMenu = this.Helper.Reflection.GetMethod(mod, "OpenAquariumCollectionMenu");
            this._stardewAquariumOpenCollectionsMenu.Invoke("aquariumprogress", Array.Empty<string>());
        }
    }

    private void TryAdd(string modId, int index, Action action, string? hoverText = null)
    {
        if (this.Helper.ModRegistry.IsLoaded(modId))
        {
            this.API.AddToolbarIcon(
                $"{modId}-{index.ToString(CultureInfo.InvariantCulture)}",
                "furyx639.ToolbarIcons/Icons",
                new(index * 16, 0, 16, 16),
                hoverText ?? this.Helper.Translation.Get($"button.{modId}"));
            this.Icons.Add($"{modId}-{index.ToString(CultureInfo.InvariantCulture)}", action);
        }
    }

    private bool TryGetMod(
        string modId,
        [NotNullWhen(true)] out IMod? mod)
    {
        var modInfo = this.Helper.ModRegistry.Get(modId);
        mod = (IMod?)modInfo?.GetType().GetProperty("Mod")?.GetValue(modInfo);
        return mod is not null;
    }
}