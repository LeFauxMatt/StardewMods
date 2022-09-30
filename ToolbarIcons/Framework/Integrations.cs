namespace StardewMods.ToolbarIcons.Framework;

using System;
using System.Collections.Generic;
using System.Reflection;
using StardewModdingAPI.Events;
using StardewMods.Common.Extensions;
using StardewMods.Common.Integrations.GenericModConfigMenu;
using StardewMods.Common.Integrations.ToolbarIcons;
using StardewMods.ToolbarIcons.Framework.IntegrationTypes;
using StardewValley.Menus;

/// <summary>
///     Handles integrations with other mods.
/// </summary>
internal sealed class Integrations
{
    private const string AlwaysScrollMapId = "bcmpinc.AlwaysScrollMap";
    private const string CJBCheatsMenuId = "CJBok.CheatsMenu";
    private const string CJBItemSpawnerId = "CJBok.ItemSpawner";
    private const string DynamicGameAssetsId = "spacechase0.DynamicGameAssets";
    private const string GenericModConfigMenuId = "spacechase0.GenericModConfigMenu";
    private const string StardewAquariumId = "Cherry.StardewAquarium";
    private const string ToDew = "jltaylor-us.ToDew";

#nullable disable
    private static Integrations Instance;
#nullable enable

    private readonly IToolbarIconsApi _api;
    private readonly GenericModConfigMenuIntegration _gmcm;
    private readonly IModHelper _helper;

    private ComplexIntegration? _complexIntegration;
    private SimpleIntegration? _simpleIntegration;
    private EventHandler? _toolbarIconsLoaded;

    private Integrations(IModHelper helper, IToolbarIconsApi api)
    {
        this._helper = helper;
        this._api = api;

        this._gmcm = new(helper.ModRegistry);

        // Events
        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
    }

    /// <summary>
    ///     Raised after Toolbar Icons have been loaded.
    /// </summary>
    public static event EventHandler ToolbarIconsLoaded
    {
        add => Integrations.Instance._toolbarIconsLoaded += value;
        remove => Integrations.Instance._toolbarIconsLoaded -= value;
    }

    /// <summary>
    ///     Gets Generic Mod Config Menu integration.
    /// </summary>
    public static GenericModConfigMenuIntegration GMCM => Integrations.Instance._gmcm;

    /// <summary>
    ///     Gets a value indicating whether the toolbar icons have been loaded.
    /// </summary>
    public static bool IsLoaded { get; private set; }

    /// <summary>
    ///     Initializes <see cref="Integrations" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="api">The Toolbar Icons Api.</param>
    /// <returns>Returns an instance of the <see cref="Integrations" /> class.</returns>
    public static Integrations Init(IModHelper helper, IToolbarIconsApi api)
    {
        return Integrations.Instance ??= new(helper, api);
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        this._simpleIntegration = SimpleIntegration.Init(this._helper, this._api);
        this._complexIntegration = ComplexIntegration.Init(this._helper, this._api);

        // Stardew Aquarium
        this._complexIntegration.AddMethodWithParams(
            Integrations.StardewAquariumId,
            1,
            I18n.Button_StardewAquarium(),
            "OpenAquariumCollectionMenu",
            "aquariumprogress",
            Array.Empty<string>());

        // CJB Cheats Menu
        this._complexIntegration.AddMethodWithParams(
            Integrations.CJBCheatsMenuId,
            2,
            I18n.Button_CheatsMenu(),
            "OpenCheatsMenu",
            0,
            true);

        // Dynamic Game Assets
        this._complexIntegration.AddMethodWithParams(
            Integrations.DynamicGameAssetsId,
            3,
            I18n.Button_DynamicGameAssets(),
            "OnStoreCommand",
            "dga_store",
            Array.Empty<string>());

        // Generic Mod Config Menu
        this._complexIntegration.AddMethodWithParams(
            Integrations.GenericModConfigMenuId,
            4,
            I18n.Button_GenericModConfigMenu(),
            "OpenListMenu",
            0);

        // CJB Item Spawner
        this._complexIntegration.AddCustomAction(
            Integrations.CJBItemSpawnerId,
            5,
            I18n.Button_ItemSpawner(),
            mod =>
            {
                var buildMenu = this._helper.Reflection.GetMethod(mod, "BuildMenu", false);
                return () => { Game1.activeClickableMenu = buildMenu.Invoke<ItemGrabMenu>(); };
            });

        // Always Scroll Map
        this._complexIntegration.AddCustomAction(
            Integrations.AlwaysScrollMapId,
            6,
            I18n.Button_AlwaysScrollMap(),
            mod =>
            {
                var config = mod.GetType().GetField("config")?.GetValue(mod);
                if (config is null)
                {
                    return null;
                }

                var enabledIndoors = this._helper.Reflection.GetField<bool>(config, "EnabledIndoors", false);
                var enabledOutdoors = this._helper.Reflection.GetField<bool>(config, "EnabledOutdoors", false);
                return () =>
                {
                    if (Game1.currentLocation.IsOutdoors)
                    {
                        enabledOutdoors.SetValue(!enabledOutdoors.GetValue());
                    }
                    else
                    {
                        enabledIndoors.SetValue(!enabledIndoors.GetValue());
                    }
                };
            });

        // To-Dew
        this._complexIntegration.AddCustomAction(
            Integrations.ToDew,
            7,
            I18n.Button_ToDew(),
            mod =>
            {
                var modType = mod.GetType();
                var perScreenList = modType.GetField("list", BindingFlags.Instance | BindingFlags.NonPublic)
                                           ?.GetValue(mod);
                var toDoMenu = modType.Assembly.GetType("ToDew.ToDoMenu");
                if (perScreenList is null || toDoMenu is null)
                {
                    return null;
                }

                return () =>
                {
                    var value = perScreenList.GetType().GetProperty("Value")?.GetValue(perScreenList);
                    if (value is null)
                    {
                        return;
                    }

                    var action = toDoMenu.GetConstructor(
                        new[]
                        {
                            modType,
                            value.GetType(),
                        });
                    if (action is null)
                    {
                        return;
                    }

                    var menu = action.Invoke(
                        new[]
                        {
                            mod,
                            value,
                        });
                    Game1.activeClickableMenu = (IClickableMenu)menu;
                };
            });

        // Special Orders
        this._complexIntegration.AddCustomAction(
            8,
            I18n.Button_SpecialOrders(),
            () => { Game1.activeClickableMenu = new SpecialOrdersBoard(); });

        // Daily Quests
        this._complexIntegration.AddCustomAction(
            9,
            I18n.Button_DailyQuests(),
            () => { Game1.activeClickableMenu = new Billboard(true); });
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        if (!Integrations.IsLoaded)
        {
            var toolbarData =
                this._helper.GameContent.Load<IDictionary<string, string>>("furyx639.ToolbarIcons/Toolbar");
            foreach (var (key, data) in toolbarData)
            {
                var info = data.Split('/');
                var modId = key.Split('/')[0];
                var index = int.Parse(info[2]);
                switch (info[3])
                {
                    case "menu":
                        this._simpleIntegration?.AddMenu(modId, index, info[0], info[4], info[1]);
                        break;
                    case "method":
                        this._simpleIntegration?.AddMethod(modId, index, info[0], info[4], info[1]);
                        break;
                    case "keybind":
                        this._simpleIntegration?.AddKeybind(modId, index, info[0], info[4], info[1]);
                        break;
                }
            }
        }

        Integrations.IsLoaded = true;
        this._toolbarIconsLoaded.InvokeAll(this);
    }
}