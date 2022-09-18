namespace StardewMods.ToolbarIcons;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.Common.Enums;
using StardewMods.Common.Helpers;
using StardewMods.Common.Integrations.GenericModConfigMenu;
using StardewMods.ToolbarIcons.ModIntegrations;
using StardewMods.ToolbarIcons.UI;
using StardewValley.Menus;

// TODO: Center Toolbar Icons

/// <inheritdoc />
public sealed class ToolbarIcons : Mod
{
    private const string AlwaysScrollMapId = "bcmpinc.AlwaysScrollMap";
    private const string CJBCheatsMenuId = "CJBok.CheatsMenu";
    private const string CJBItemSpawnerId = "CJBok.ItemSpawner";
    private const string DynamicGameAssetsId = "spacechase0.DynamicGameAssets";
    private const string GenericModConfigMenuId = "spacechase0.GenericModConfigMenu";
    private const string StardewAquariumId = "Cherry.StardewAquarium";
    private const string ToDew = "jltaylor-us.ToDew";

    private readonly PerScreen<ToolbarIconsApi?> _api = new();
    private readonly PerScreen<ComponentArea> _area = new(() => ComponentArea.Custom);
    private readonly PerScreen<ClickableComponent?> _button = new();
    private readonly PerScreen<string> _hoverText = new();
    private readonly PerScreen<Toolbar?> _toolbar = new();

    private ModConfig? _config;

    private ToolbarIconsApi Api => this._api.Value ??= new(this.Helper, this.Config.Icons, this.Components);

    private ComponentArea Area
    {
        get => this._area.Value;
        set => this._area.Value = value;
    }

    private ClickableComponent? Button
    {
        get
        {
            var toolbar = Game1.onScreenMenus.OfType<Toolbar>().FirstOrDefault();
            if (this.Toolbar is not null && ReferenceEquals(toolbar, this.Toolbar))
            {
                return this._button.Value;
            }

            if (toolbar is null)
            {
                return null;
            }

            this.Toolbar = toolbar;
            var buttons = this.Helper.Reflection.GetField<List<ClickableComponent>>(toolbar, "buttons").GetValue();
            this._button.Value = buttons.First();
            return this._button.Value;
        }
    }

    private ComplexIntegration? ComplexIntegration { get; set; }

    private Dictionary<string, ClickableTextureComponent> Components { get; } = new();

    private ModConfig Config => this._config ??= CommonHelpers.GetConfig<ModConfig>(this.Helper);

    private string HoverText
    {
        get => this._hoverText.Value;
        set => this._hoverText.Value = value;
    }

    private bool Loaded { get; set; }

    private bool ShowToolbar => this.Loaded
                             && Game1.displayHUD
                             && Context.IsPlayerFree
                             && Game1.activeClickableMenu is null
                             && Game1.onScreenMenus.OfType<Toolbar>().Any();

    private SimpleIntegration? SimpleIntegration { get; set; }

    private Toolbar? Toolbar
    {
        get => this._toolbar.Value;
        set => this._toolbar.Value = value;
    }

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        Log.Monitor = this.Monitor;
        I18n.Init(this.Helper.Translation);
        ThemeHelper.Init(this.Helper, "furyx639.ToolbarIcons/Icons", "furyx639.ToolbarIcons/Arrows");

        // Events
        this.Helper.Events.Content.AssetRequested += ToolbarIcons.OnAssetRequested;
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        this.Helper.Events.Input.CursorMoved += this.OnCursorMoved;
        this.Helper.Events.Display.RenderedHud += this.OnRenderedHud;
        this.Helper.Events.Display.RenderingHud += this.OnRenderingHud;
        this.Helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
    }

    /// <inheritdoc />
    public override object GetApi()
    {
        return this.Api;
    }

    private static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo("furyx639.ToolbarIcons/Icons"))
        {
            e.LoadFromModFile<Texture2D>("assets/icons.png", AssetLoadPriority.Exclusive);
            return;
        }

        if (e.Name.IsEquivalentTo("furyx639.ToolbarIcons/Arrows"))
        {
            e.LoadFromModFile<Texture2D>("assets/arrows.png", AssetLoadPriority.Exclusive);
            return;
        }

        if (e.Name.IsEquivalentTo("furyx639.ToolbarIcons/Toolbar"))
        {
            e.LoadFrom(() => new Dictionary<string, string>(), AssetLoadPriority.Exclusive);
        }
    }

    private void DrawButton(SpriteBatch b, Vector2 pos)
    {
        var label = I18n.Config_OpenMenu_Name();
        var dims = Game1.dialogueFont.MeasureString(I18n.Config_OpenMenu_Name());
        var bounds = new Rectangle((int)pos.X, (int)pos.Y, (int)dims.X + Game1.tileSize, Game1.tileSize);
        if (Game1.activeClickableMenu.GetChildMenu() is null)
        {
            var point = Game1.getMousePosition();
            if (Game1.oldMouseState.LeftButton == ButtonState.Released
             && Mouse.GetState().LeftButton == ButtonState.Pressed
             && bounds.Contains(point))
            {
                Game1.activeClickableMenu.SetChildMenu(new ToolbarIconsMenu(this.Config.Icons, this.Components));
                return;
            }
        }

        IClickableMenu.drawTextureBox(
            b,
            Game1.mouseCursors,
            new(432, 439, 9, 9),
            bounds.X,
            bounds.Y,
            bounds.Width,
            bounds.Height,
            Color.White,
            Game1.pixelZoom,
            false,
            1f);
        Utility.drawTextWithShadow(
            b,
            label,
            Game1.dialogueFont,
            new Vector2(bounds.Left + bounds.Right - dims.X, bounds.Top + bounds.Bottom - dims.Y) / 2f,
            Game1.textColor,
            1f,
            1f,
            -1,
            -1,
            0f);
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!this.ShowToolbar || this.Helper.Input.IsSuppressed(e.Button))
        {
            return;
        }

        if (e.Button is not SButton.MouseLeft or SButton.MouseRight
         && !(e.Button.IsActionButton() || e.Button.IsUseToolButton()))
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        var component =
            this.Components.Values.FirstOrDefault(component => component.visible && component.containsPoint(x, y));
        if (component is null)
        {
            return;
        }

        Game1.playSound("drumkit6");
        this.Api.Invoke(component.name);
        this.Helper.Input.Suppress(e.Button);
    }

    private void OnCursorMoved(object? sender, CursorMovedEventArgs e)
    {
        if (!this.ShowToolbar)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        this.HoverText = string.Empty;
        foreach (var component in this.Components.Values.Where(component => component.visible))
        {
            component.tryHover(x, y);
            if (component.bounds.Contains(x, y))
            {
                this.HoverText = component.hoverText;
            }
        }
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        this.SimpleIntegration = SimpleIntegration.Init(this.Helper, this.Api);
        this.ComplexIntegration = ComplexIntegration.Init(this.Helper, this.Api);

        // Stardew Aquarium
        this.ComplexIntegration.AddMethodWithParams(
            ToolbarIcons.StardewAquariumId,
            1,
            I18n.Button_StardewAquarium(),
            "OpenAquariumCollectionMenu",
            "aquariumprogress",
            Array.Empty<string>());

        // CJB Cheats Menu
        this.ComplexIntegration.AddMethodWithParams(
            ToolbarIcons.CJBCheatsMenuId,
            2,
            I18n.Button_CheatsMenu(),
            "OpenCheatsMenu",
            0,
            true);

        // Dynamic Game Assets
        this.ComplexIntegration.AddMethodWithParams(
            ToolbarIcons.DynamicGameAssetsId,
            3,
            I18n.Button_DynamicGameAssets(),
            "OnStoreCommand",
            "dga_store",
            Array.Empty<string>());

        // Generic Mod Config Menu
        this.ComplexIntegration.AddMethodWithParams(
            ToolbarIcons.GenericModConfigMenuId,
            4,
            I18n.Button_GenericModConfigMenu(),
            "OpenListMenu",
            0);

        // CJB Item Spawner
        this.ComplexIntegration.AddCustomAction(
            ToolbarIcons.CJBItemSpawnerId,
            5,
            I18n.Button_ItemSpawner(),
            mod =>
            {
                var buildMenu = this.Helper.Reflection.GetMethod(mod, "BuildMenu", false);
                return () => { Game1.activeClickableMenu = buildMenu.Invoke<ItemGrabMenu>(); };
            });

        // Always Scroll Map
        this.ComplexIntegration.AddCustomAction(
            ToolbarIcons.AlwaysScrollMapId,
            6,
            I18n.Button_AlwaysScrollMap(),
            mod =>
            {
                var config = mod.GetType().GetField("config")?.GetValue(mod);
                if (config is null)
                {
                    return null;
                }

                var enabledIndoors = this.Helper.Reflection.GetField<bool>(config, "EnabledIndoors", false);
                var enabledOutdoors = this.Helper.Reflection.GetField<bool>(config, "EnabledOutdoors", false);
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
        this.ComplexIntegration.AddCustomAction(
            ToolbarIcons.ToDew,
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
        this.ComplexIntegration.AddCustomAction(
            8,
            I18n.Button_SpecialOrders(),
            () => { Game1.activeClickableMenu = new SpecialOrdersBoard(); });

        // Daily Quests
        this.ComplexIntegration.AddCustomAction(
            9,
            I18n.Button_DailyQuests(),
            () => { Game1.activeClickableMenu = new Billboard(true); });
    }

    private void OnRenderedHud(object? sender, RenderedHudEventArgs e)
    {
        if (!this.ShowToolbar)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(this.HoverText))
        {
            IClickableMenu.drawHoverText(e.SpriteBatch, this.HoverText, Game1.smallFont);
        }
    }

    private void OnRenderingHud(object? sender, RenderingHudEventArgs e)
    {
        if (!this.ShowToolbar)
        {
            return;
        }

        this.ReorientComponents();

        foreach (var component in this.Components.Values.Where(component => component.visible))
        {
            var icons = this.Helper.GameContent.Load<Texture2D>("furyx639.ToolbarIcons/Icons");
            e.SpriteBatch.Draw(
                icons,
                new(component.bounds.X, component.bounds.Y),
                new(0, 0, 16, 16),
                Color.White,
                0f,
                Vector2.Zero,
                2f,
                SpriteEffects.None,
                1f);
            component.draw(e.SpriteBatch);
        }
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        if (!this.Loaded)
        {
            var toolbarData =
                this.Helper.GameContent.Load<IDictionary<string, string>>("furyx639.ToolbarIcons/Toolbar");
            foreach (var (key, data) in toolbarData)
            {
                var info = data.Split('/');
                var modId = key.Split('/')[0];
                var index = int.Parse(info[2]);
                switch (info[3])
                {
                    case "menu":
                        this.SimpleIntegration?.AddMenu(modId, index, info[0], info[4], info[1]);
                        break;
                    case "method":
                        this.SimpleIntegration?.AddMethod(modId, index, info[0], info[4], info[1]);
                        break;
                    case "keybind":
                        this.SimpleIntegration?.AddKeybind(modId, index, info[0], info[4], info[1]);
                        break;
                }
            }
        }

        this.ReorientComponents();
        this.Loaded = true;

        var gmcm = new GenericModConfigMenuIntegration(this.Helper.ModRegistry);
        if (!gmcm.IsLoaded)
        {
            return;
        }

        // Register mod configuration
        gmcm.Register(this.ModManifest, () => this._config = new(), this.SaveConfig);

        gmcm.API.AddComplexOption(
            this.ModManifest,
            I18n.Config_CustomizeToolbar_Name,
            this.DrawButton,
            I18n.Config_CustomizeToolbar_Tooltip,
            height: () => Game1.tileSize);
    }

    private void ReorientComponents()
    {
        if (this.Button is null || this.Toolbar is null || !this.Components.Values.Any(component => component.visible))
        {
            return;
        }

        var xAlign = this.Button.bounds.X < Game1.viewport.Width / 2;
        var yAlign = this.Button.bounds.Y < Game1.viewport.Height / 2;
        ComponentArea area;
        int x;
        int y;
        if (this.Toolbar.width > this.Toolbar.height)
        {
            x = this.Button.bounds.Left;
            if (yAlign)
            {
                area = ComponentArea.Top;
                y = this.Button.bounds.Bottom + 20;
            }
            else
            {
                area = ComponentArea.Bottom;
                y = this.Button.bounds.Top - 52;
            }
        }
        else
        {
            y = this.Button.bounds.Top;
            if (xAlign)
            {
                area = ComponentArea.Left;
                x = this.Button.bounds.Right + 20;
            }
            else
            {
                area = ComponentArea.Right;
                x = this.Button.bounds.Left - 52;
            }
        }

        var firstComponent = this.Components.Values.First(component => component.visible);
        if (area != this.Area || firstComponent.bounds.X != x || firstComponent.bounds.Y != y)
        {
            this.ReorientComponents(area, x, y);
        }
    }

    private void ReorientComponents(ComponentArea area, int x, int y)
    {
        this.Area = area;
        foreach (var icon in this.Config.Icons)
        {
            if (this.Components.TryGetValue(icon.Id, out var component))
            {
                if (!icon.Enabled)
                {
                    component.visible = false;
                    continue;
                }

                component.visible = true;
                component.bounds.X = x;
                component.bounds.Y = y;
                switch (area)
                {
                    case ComponentArea.Top:
                    case ComponentArea.Bottom:
                        x += component.bounds.Width + 4;
                        break;
                    case ComponentArea.Right:
                    case ComponentArea.Left:
                        y += component.bounds.Height + 4;
                        break;
                    case ComponentArea.Custom:
                    default:
                        break;
                }
            }
        }
    }

    private void SaveConfig()
    {
        foreach (var icon in this.Config.Icons)
        {
            if (this.Components.TryGetValue(icon.Id, out var component))
            {
                component.visible = icon.Enabled;
            }
        }

        this.Helper.WriteConfig(this.Config);
        this.ReorientComponents();
    }
}