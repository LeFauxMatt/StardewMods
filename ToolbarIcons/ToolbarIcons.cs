namespace StardewMods.ToolbarIcons;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.Common.Enums;
using StardewMods.Common.Helpers;
using StardewMods.Common.Integrations.GenericModConfigMenu;
using StardewMods.ToolbarIcons.ModIntegrations;
using StardewMods.ToolbarIcons.UI;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc />
public class ToolbarIcons : Mod
{
    private const string AlwaysScrollMapId = "bcmpinc.AlwaysScrollMap";
    private const string ChestsAnywhereId = "Pathoschild.ChestsAnywhere";
    private const string CJBCheatsMenuId = "CJBok.CheatsMenu";
    private const string CJBItemSpawnerId = "CJBok.ItemSpawner";
    private const string DataLayersId = "Pathoschild.DataLayers";
    private const string DebugModeId = "Pathoschild.DebugMode";
    private const string DynamicGameAssetsId = "spacechase0.DynamicGameAssets";
    private const string HorseFluteAnywhereId = "Pathoschild.HorseFluteAnywhere";
    private const string InstantBuildingId = "BitwiseJonMods.InstantBuildings";
    private const string LookupAnythingId = "Pathoschild.LookupAnything";
    private const string StardewAquariumId = "Cherry.StardewAquarium";

    private readonly PerScreen<Dictionary<string, string>> _actions = new(() => new());
    private readonly PerScreen<ToolbarIconsApi?> _api = new();
    private readonly PerScreen<ComponentArea> _area = new(() => ComponentArea.Custom);
    private readonly PerScreen<ClickableComponent?> _button = new();
    private readonly PerScreen<string> _hoverText = new();
    private readonly PerScreen<Toolbar?> _toolbar = new();

    private ModConfig? _config;
    private MethodInfo? _overrideButtonReflected;

    private Dictionary<string, string> Actions
    {
        get => this._actions.Value;
    }

    private ToolbarIconsApi Api
    {
        get => this._api.Value ??= new(this.Helper, this.Config.Icons, this.Components);
    }

    private ComponentArea Area
    {
        get => this._area.Value;
        set => this._area.Value = value;
    }

    [MemberNotNull(nameof(ToolbarIcons.Toolbar))]
    private ClickableComponent? Button
    {
        get
        {
            var toolbar = Game1.onScreenMenus.OfType<Toolbar>().FirstOrDefault();
            if (this.Toolbar is not null && ReferenceEquals(toolbar, this.Toolbar))
            {
                return this._button.Value;
            }

            if (toolbar is not null)
            {
                this.Toolbar = toolbar;
                var buttons = this.Helper.Reflection.GetField<List<ClickableComponent>>(toolbar, "buttons").GetValue();
                this._button.Value = buttons.First();
                return this._button.Value;
            }

            return null;
        }
    }

    private Dictionary<string, ClickableTextureComponent> Components { get; } = new();

    private ModConfig Config
    {
        get
        {
            if (this._config is not null)
            {
                return this._config;
            }

            ModConfig? config = null;
            try
            {
                config = this.Helper.ReadConfig<ModConfig>();
            }
            catch (Exception)
            {
                // ignored
            }

            this._config = config ?? new ModConfig();
            Log.Trace(this._config.ToString());
            return this._config;
        }
    }

    private string HoverText
    {
        get => this._hoverText.Value;
        set => this._hoverText.Value = value;
    }

    private IDictionary<string, SButton[]> Keybinds { get; } = new Dictionary<string, SButton[]>();

    private MethodInfo OverrideButtonReflected
    {
        get => this._overrideButtonReflected ??= Game1.input.GetType().GetMethod("OverrideButton")!;
    }

    private Toolbar? Toolbar
    {
        get => this._toolbar.Value;
        set => this._toolbar.Value = value;
    }

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        Log.Monitor = this.Monitor;
        I18n.Init(helper.Translation);

        if (this.Helper.ModRegistry.IsLoaded("furyx639.FuryCore"))
        {
            Log.Alert("Remove FuryCore, it is no longer needed by this mod!");
        }

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

        if (e.Name.IsEquivalentTo("furyx639.FuryCore/Toolbar"))
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
            if (Game1.oldMouseState.LeftButton == ButtonState.Released && Mouse.GetState().LeftButton == ButtonState.Pressed && bounds.Contains(point))
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
        if (!Game1.displayHUD || Game1.activeClickableMenu is not null || !Game1.onScreenMenus.OfType<Toolbar>().Any())
        {
            return;
        }

        if (e.Button is not SButton.MouseLeft or SButton.MouseRight)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        var component = this.Components.Values.FirstOrDefault(component => component.visible && component.containsPoint(x, y));
        if (component is not null)
        {
            Game1.playSound("drumkit6");
            if (this.Actions.TryGetValue(component.name, out var action))
            {
                if (action.StartsWith("toggle:"))
                {
                    var command = action[7..].Trim();
                    foreach (var subComponent in this.Components.Values.Where(subComponent => component != subComponent && subComponent.name.StartsWith(command)))
                    {
                        subComponent.visible = !subComponent.visible;
                    }
                }
                else if (action.StartsWith("keybind:"))
                {
                    if (!this.Keybinds.TryGetValue(action, out var keybind))
                    {
                        var keys = action[8..].Trim().Split(' ');
                        IList<SButton> buttons = new List<SButton>();
                        foreach (var key in keys)
                        {
                            if (Enum.TryParse(key, out SButton button))
                            {
                                buttons.Add(button);
                            }
                        }

                        keybind = buttons.ToArray();
                        this.Keybinds.Add(action, keybind);
                    }

                    foreach (var button in keybind)
                    {
                        this.OverrideButton(button, true);
                    }
                }

                this.Helper.Input.Suppress(e.Button);
            }

            this.Api.Invoke(component.name);
            this.Helper.Input.Suppress(e.Button);
        }
    }

    private void OnCursorMoved(object? sender, CursorMovedEventArgs e)
    {
        if (!Game1.displayHUD || Game1.activeClickableMenu is not null || !Game1.onScreenMenus.OfType<Toolbar>().Any())
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
        var gmcm = new GenericModConfigMenuIntegration(this.Helper.ModRegistry);
        var simple = SimpleIntegration.Init(this.Helper, this.Api);
        var complex = ComplexIntegration.Init(this.Helper, this.Api);

        // Integrations
        complex.AddIntegration(
            ToolbarIcons.AlwaysScrollMapId,
            2,
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
                if (enabledIndoors is null || enabledOutdoors is null)
                {
                    return null;
                }

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
        simple.AddIntegration(ToolbarIcons.ChestsAnywhereId, 3, I18n.Button_ChestsAnywhere(), "OpenMenu");
        simple.AddIntegration(ToolbarIcons.CJBCheatsMenuId, 4, I18n.Button_CheatsMenu(), "OpenCheatsMenu", 0, true);
        complex.AddIntegration(
            ToolbarIcons.CJBItemSpawnerId,
            5,
            I18n.Button_ItemSpawner(),
            mod =>
            {
                var buildMenu = this.Helper.Reflection.GetMethod(mod, "BuildMenu");
                return buildMenu is not null
                    ? () => { Game1.activeClickableMenu = buildMenu.Invoke<ItemGrabMenu>(); }
                    : null;
            });
        simple.AddIntegration(ToolbarIcons.DataLayersId, 10, I18n.Button_DataLayers(), "ToggleLayers");
        simple.AddIntegration(ToolbarIcons.DebugModeId, 11, I18n.Button_DebugMode(), "ToggleDebugMenu");
        simple.AddIntegration(ToolbarIcons.DynamicGameAssetsId, 6, I18n.Button_DynamicGameAssets(), "OnStoreCommand", "dga_store", Array.Empty<string>());
        simple.AddIntegration(ToolbarIcons.HorseFluteAnywhereId, 12, I18n.Button_HorseFluteAnywhere(), "SummonHorse");
        simple.AddIntegration(ToolbarIcons.InstantBuildingId, 7, I18n.Button_InstantBuildings_Build(), "HandleInstantBuildButtonClick");
        simple.AddIntegration(ToolbarIcons.InstantBuildingId, 8, I18n.Button_InstantBuildings_Upgrade(), "HandleInstantUpgradeButtonClick");
        simple.AddIntegration(ToolbarIcons.LookupAnythingId, 9, I18n.Button_LookupAnything(), "TryToggleSearch");
        simple.AddIntegration(ToolbarIcons.StardewAquariumId, 1, I18n.Button_StardewAquarium(), "OpenAquariumCollectionMenu", "aquariumprogress", Array.Empty<string>());

        if (gmcm.IsLoaded)
        {
            // Register mod configuration
            gmcm.Register(
                this.ModManifest,
                () => this._config = new(),
                () => this.Helper.WriteConfig(this.Config));

            gmcm.API.AddComplexOption(
                this.ModManifest,
                I18n.Config_CustomizeToolbar_Name,
                this.DrawButton,
                I18n.Config_CustomizeToolbar_Tooltip,
                height: () => Game1.tileSize);
        }
    }

    private void OnRenderedHud(object? sender, RenderedHudEventArgs e)
    {
        if (!Game1.displayHUD || Game1.activeClickableMenu is not null || !Game1.onScreenMenus.OfType<Toolbar>().Any())
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
        if (!Game1.displayHUD || Game1.activeClickableMenu is not null)
        {
            return;
        }

        this.ReorientComponents();

        foreach (var component in this.Components.Values)
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
        foreach (var (key, data) in this.Helper.GameContent.Load<IDictionary<string, string>>("furyx639.FuryCore/Toolbar"))
        {
            var info = data.Split('/');
            this.Api.AddToolbarIcon(key, info[1], new(16 * int.Parse(info[2]), 0, 16, 16), info[0]);
            this.Actions.Add(key, info[4]);
        }

        this.ReorientComponents();
    }

    private void OverrideButton(SButton button, bool inputState)
    {
        this.OverrideButtonReflected.Invoke(Game1.input, new object[] { button, inputState });
    }

    private void ReorientComponents()
    {
        if (this.Button is null || !this.Components.Values.Any(component => component.visible))
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
        //var (_, playerGlobalY) = Game1.player.GetBoundingBox().Center;
        //var (_, playerLocalY) = Game1.GlobalToLocal(globalPosition: new Vector2(0, playerGlobalY), viewport: Game1.viewport);
        //var x = (Game1.uiViewport.Width - Game1.tileSize * 12) / 2;

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
}