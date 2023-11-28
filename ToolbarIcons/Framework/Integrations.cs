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
    private static Integrations instance;
#nullable enable

    private readonly IToolbarIconsApi api;
    private readonly GenericModConfigMenuIntegration gmcm;
    private readonly IModHelper helper;

    private ComplexIntegration? complexIntegration;
    private SimpleIntegration? simpleIntegration;
    private EventHandler? toolbarIconsLoaded;

    private Integrations(IModHelper helper, IToolbarIconsApi api)
    {
        this.helper = helper;
        this.api = api;

        this.gmcm = new(helper.ModRegistry);

        // Events
        helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
    }

    /// <summary>
    ///     Raised after Toolbar Icons have been loaded.
    /// </summary>
    public static event EventHandler ToolbarIconsLoaded
    {
        add => Integrations.instance.toolbarIconsLoaded += value;
        remove => Integrations.instance.toolbarIconsLoaded -= value;
    }

    /// <summary>
    ///     Gets Generic Mod Config Menu integration.
    /// </summary>
    public static GenericModConfigMenuIntegration GMCM => Integrations.instance.gmcm;

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
        return Integrations.instance ??= new(helper, api);
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        this.simpleIntegration = SimpleIntegration.Init(this.helper, this.api);
        this.complexIntegration = ComplexIntegration.Init(this.helper, this.api);

        // Stardew Aquarium
        this.complexIntegration.AddMethodWithParams(
            Integrations.StardewAquariumId,
            1,
            I18n.Button_StardewAquarium(),
            "OpenAquariumCollectionMenu",
            "aquariumprogress",
            Array.Empty<string>());

        // CJB Cheats Menu
        this.complexIntegration.AddMethodWithParams(
            Integrations.CJBCheatsMenuId,
            2,
            I18n.Button_CheatsMenu(),
            "OpenCheatsMenu",
            0,
            true);

        // Dynamic Game Assets
        this.complexIntegration.AddMethodWithParams(
            Integrations.DynamicGameAssetsId,
            3,
            I18n.Button_DynamicGameAssets(),
            "OnStoreCommand",
            "dga_store",
            Array.Empty<string>());

        // Generic Mod Config Menu
        this.complexIntegration.AddMethodWithParams(
            Integrations.GenericModConfigMenuId,
            4,
            I18n.Button_GenericModConfigMenu(),
            "OpenListMenu",
            0);

        // CJB Item Spawner
        this.complexIntegration.AddCustomAction(
            Integrations.CJBItemSpawnerId,
            5,
            I18n.Button_ItemSpawner(),
            mod =>
            {
                var buildMenu = this.helper.Reflection.GetMethod(mod, "BuildMenu", false);
                return () => { Game1.activeClickableMenu = buildMenu.Invoke<ItemGrabMenu>(); };
            });

        // Always Scroll Map
        this.complexIntegration.AddCustomAction(
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

                var enabledIndoors = this.helper.Reflection.GetField<bool>(config, "EnabledIndoors", false);
                var enabledOutdoors = this.helper.Reflection.GetField<bool>(config, "EnabledOutdoors", false);
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
        this.complexIntegration.AddCustomAction(
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
        this.complexIntegration.AddCustomAction(
            8,
            I18n.Button_SpecialOrders(),
            () => { Game1.activeClickableMenu = new SpecialOrdersBoard(); });

        // Daily Quests
        this.complexIntegration.AddCustomAction(
            9,
            I18n.Button_DailyQuests(),
            () => { Game1.activeClickableMenu = new Billboard(true); });
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        if (!Integrations.IsLoaded)
        {
            var toolbarData =
                this.helper.GameContent.Load<IDictionary<string, string>>("furyx639.ToolbarIcons/Toolbar");
            foreach (var (key, data) in toolbarData)
            {
                var info = data.Split('/');
                var modId = key.Split('/')[0];
                var index = int.Parse(info[2]);
                switch (info[3])
                {
                    case "menu":
                        this.simpleIntegration?.AddMenu(modId, index, info[0], info[4], info[1]);
                        break;
                    case "method":
                        this.simpleIntegration?.AddMethod(modId, index, info[0], info[4], info[1]);
                        break;
                    case "keybind":
                        this.simpleIntegration?.AddKeybind(modId, index, info[0], info[4], info[1]);
                        break;
                }
            }
        }

        Integrations.IsLoaded = true;
        this.toolbarIconsLoaded.InvokeAll(this);
    }
}