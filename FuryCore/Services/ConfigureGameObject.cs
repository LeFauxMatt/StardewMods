namespace StardewMods.FuryCore.Services;

using System;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.FuryCore.Attributes;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Events;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewMods.FuryCore.Interfaces.GameObjects;
using StardewMods.FuryCore.Models;
using StardewValley;
using StardewValley.Tools;

/// <inheritdoc cref="StardewMods.FuryCore.Interfaces.IConfigureGameObject" />
[FuryCoreService(true)]
internal class ConfigureGameObject : IConfigureGameObject, IModService
{
    private readonly ConfiguringGameObject _configuringGameObject;
    private readonly PerScreen<IGameObject> _currentObject = new();
    private readonly Lazy<ModConfigMenu> _modConfigMenu;
    private readonly ResettingConfig _resettingConfig;
    private readonly SavingConfig _savingConfig;
    private Texture2D _configTool;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigureGameObject" /> class.
    /// </summary>
    /// <param name="config">The data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper to read/save config data and for events.</param>
    /// <param name="manifest">The mod manifest to subscribe to GMCM with.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public ConfigureGameObject(ConfigData config, IModHelper helper, IManifest manifest, IModServices services)
    {
        ConfigureGameObject.Instance = this;
        this.Helper = helper;
        this._modConfigMenu = services.Lazy<ModConfigMenu>();
        this._configuringGameObject = new(config, this.Helper, manifest, services);
        this._resettingConfig = new(services);
        this._savingConfig = new(services);
        services.Lazy<IHarmonyHelper>(
            harmonyHelper =>
            {
                var id = $"{FuryCore.ModUniqueId}.{nameof(ConfigureGameObject)}";
                harmonyHelper.AddPatches(
                    id,
                    new SavedPatch[]
                    {
                        new(
                            AccessTools.Method(typeof(Tool), nameof(Tool.beginUsing)),
                            typeof(ConfigureGameObject),
                            nameof(ConfigureGameObject.Tool_beginUsing_prefix),
                            PatchType.Prefix),
                        new(
                            AccessTools.Method(typeof(Tool), nameof(Tool.drawInMenu), new[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool) }),
                            typeof(ConfigureGameObject),
                            nameof(ConfigureGameObject.Tool_drawInMenu_prefix),
                            PatchType.Prefix),
                        new(
                            AccessTools.PropertyGetter(typeof(Tool), nameof(Tool.DisplayName)),
                            typeof(ConfigureGameObject),
                            nameof(ConfigureGameObject.Tool_getDisplayName_postfix),
                            PatchType.Postfix),
                        new(
                            AccessTools.PropertyGetter(typeof(Tool), nameof(Tool.Description)),
                            typeof(ConfigureGameObject),
                            nameof(ConfigureGameObject.Tool_getDescription_postfix),
                            PatchType.Postfix),
                    });
                harmonyHelper.ApplyPatches(id);
            });
        this.Helper.Events.Display.MenuChanged += this.OnMenuChanged;
    }

    /// <inheritdoc />
    public event EventHandler<IConfiguringGameObjectEventArgs> ConfiguringGameObject
    {
        add => this._configuringGameObject.Add(value);
        remove => this._configuringGameObject.Remove(value);
    }

    /// <inheritdoc />
    public event EventHandler<IResettingConfigEventArgs> ResettingConfig
    {
        add => this._resettingConfig.Add(value);
        remove => this._resettingConfig.Remove(value);
    }

    /// <inheritdoc />
    public event EventHandler<ISavingConfigEventArgs> SavingConfig
    {
        add => this._savingConfig.Add(value);
        remove => this._savingConfig.Remove(value);
    }

    /// <inheritdoc />
    public IGameObject CurrentObject
    {
        get => this._currentObject.Value;
        private set => this._currentObject.Value = value;
    }

    private static ConfigureGameObject Instance { get; set; }

    private Texture2D ConfigTool
    {
        get => this._configTool ??= this.Helper.Content.Load<Texture2D>($"{FuryCore.ModUniqueId}/ConfigTool", ContentSource.GameContent);
    }

    private IModHelper Helper { get; }

    private ModConfigMenu ModConfigMenu
    {
        get => this._modConfigMenu.Value;
    }

    /// <inheritdoc />
    public GenericTool GetConfigTool()
    {
        var tool = new GenericTool(I18n.Tool_ConfigTool_Name(), I18n.Tool_ConfigTool_Description(), -1, 6, 6);
        tool.modData.Add($"{FuryCore.ModUniqueId}/Tool", "ConfigTool");
        return tool;
    }

    /// <summary>
    ///     Registers a new Config Menu for the current object.
    /// </summary>
    /// <param name="gameObject">The game object to configure.</param>
    public void Register(IGameObject gameObject)
    {
        if (this.ModConfigMenu.Register(this._resettingConfig.Reset, this._savingConfig.Save))
        {
            this.CurrentObject = gameObject;
        }
    }

    /// <summary>
    ///     Shows the Config Menu.
    /// </summary>
    public void Show()
    {
        if (this.CurrentObject is not null)
        {
            this.ModConfigMenu.Show();
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static bool Tool_beginUsing_prefix(Tool __instance, Farmer who, ref bool __result)
    {
        if (!__instance.modData.TryGetValue($"{FuryCore.ModUniqueId}/Tool", out var toolName))
        {
            return true;
        }

        switch (toolName)
        {
            case "ConfigTool":
                Game1.toolAnimationDone(who);
                who.CanMove = true;
                who.UsingTool = false;
                __result = true;
                return false;
        }

        return true;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static bool Tool_drawInMenu_prefix(Tool __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, Color color)
    {
        if (!__instance.modData.TryGetValue($"{FuryCore.ModUniqueId}/Tool", out var toolName))
        {
            return true;
        }

        switch (toolName)
        {
            case "ConfigTool":
                spriteBatch.Draw(ConfigureGameObject.Instance.ConfigTool, location + new Vector2(32f, 32f), new Rectangle(0, 0, 16, 16), color * transparency, 0f, new(8f, 8f), 4f * scaleSize, SpriteEffects.None, layerDepth);
                return false;
        }

        return true;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static void Tool_getDescription_postfix(Tool __instance, ref string __result)
    {
        if (!__instance.modData.TryGetValue($"{FuryCore.ModUniqueId}/Tool", out var toolName))
        {
            return;
        }

        switch (toolName)
        {
            case "ConfigTool":
                __result = I18n.Tool_ConfigTool_Description();
                return;
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static void Tool_getDisplayName_postfix(Tool __instance, ref string __result)
    {
        if (!__instance.modData.TryGetValue($"{FuryCore.ModUniqueId}/Tool", out var toolName))
        {
            return;
        }

        switch (toolName)
        {
            case "ConfigTool":
                __result = I18n.Tool_ConfigTool_Name();
                return;
        }
    }

    private void OnMenuChanged(object sender, MenuChangedEventArgs e)
    {
        if (this.CurrentObject is not null && e.OldMenu?.GetType().Name == "SpecificModConfigMenu")
        {
            this.CurrentObject = null;
            this.ModConfigMenu.Unregister();

            if (e.NewMenu?.GetType().Name == "ModConfigMenu")
            {
                Game1.activeClickableMenu = null;
            }
        }
    }
}