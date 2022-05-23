#nullable disable

namespace StardewMods.FuryCore.Events;

using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewMods.FuryCore.Models;
using StardewMods.FuryCore.Models.CustomEvents;
using StardewMods.FuryCore.Services;
using StardewValley;
using StardewValley.Tools;

/// <inheritdoc />
internal class ConfiguringGameObject : SortedEventHandler<IConfiguringGameObjectEventArgs>
{
    private readonly Lazy<ConfigureGameObject> _configureGameObject;
    private readonly Lazy<IGameObjects> _gameObjects;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfiguringGameObject" /> class.
    /// </summary>
    /// <param name="config">The data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper to read/save config data and for events.</param>
    /// <param name="manifest">The mod manifest to subscribe to GMCM with.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public ConfiguringGameObject(ConfigData config, IModHelper helper, IManifest manifest, IModServices services)
    {
        this.Config = config;
        this.Helper = helper;
        this.Manifest = manifest;
        this._configureGameObject = services.Lazy<ConfigureGameObject>();
        this._gameObjects = services.Lazy<IGameObjects>();
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
    }

    private ConfigData Config { get; }

    private ConfigureGameObject ConfigureGameObject
    {
        get => this._configureGameObject.Value;
    }

    private IGameObjects GameObjects
    {
        get => this._gameObjects.Value;
    }

    private IModHelper Helper { get; }

    private IManifest Manifest { get; }

    private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree || !e.Button.IsUseToolButton() || this.Helper.Input.IsSuppressed(e.Button) || Game1.player.CurrentItem is not GenericTool genericTool || !genericTool.modData.TryGetValue($"{FuryCore.ModUniqueId}/Tool", out var toolName) || toolName != "ConfigTool")
        {
            return;
        }

        var pos = e.Button.TryGetController(out _) ? Game1.player.GetToolLocation() / 64 : e.Cursor.Tile;
        var x = (int)pos.X;
        var y = (int)pos.Y;
        pos.X = x;
        pos.Y = y;

        if (!Utility.withinRadiusOfPlayer(x * Game1.tileSize, y * Game1.tileSize, 1, Game1.player))
        {
            return;
        }

        if (!Game1.currentLocation.Objects.TryGetValue(pos, out var obj) || !this.GameObjects.TryGetGameObject(obj, out var gameObject))
        {
            return;
        }

        this.Helper.Input.Suppress(e.Button);
        this.ConfigureGameObject.Register(gameObject);
        this.InvokeAll(new ConfiguringGameObjectEventArgs(gameObject, this.Manifest));
        this.ConfigureGameObject.Show();
    }

    private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
    {
        if (!Context.IsPlayerFree
            || this.HandlerCount == 0
            || !this.Config.Configure.JustPressed()
            || Game1.player.CurrentItem is null
            || !this.GameObjects.TryGetGameObject(Game1.player.CurrentItem, out var gameObject))
        {
            return;
        }

        this.Helper.Input.SuppressActiveKeybinds(this.Config.Configure);
        this.ConfigureGameObject.Register(gameObject);
        this.InvokeAll(new ConfiguringGameObjectEventArgs(gameObject, this.Manifest));
        this.ConfigureGameObject.Show();
    }
}