namespace StardewMods.BetterChests.Features;

using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Helpers;
using StardewMods.CommonHarmony.Enums;
using StardewMods.CommonHarmony.Helpers;
using StardewMods.CommonHarmony.Models;
using StardewValley;
using StardewValley.Tools;
using SObject = StardewValley.Object;

/// <summary>
///     Configure storages individually.
/// </summary>
internal class Configurator : IFeature
{
    private const string Id = "furyx639.BetterChests/Configurator";
    private const string ToolKey = "furyx639.FuryCore/Tool";
    private const string ToolName = "ConfigTool";

    private Texture2D? _cachedTool;

    private Configurator(IModHelper helper, ModConfig config)
    {
        this.Helper = helper;
        this.Config = config;
        HarmonyHelper.AddPatches(
            Configurator.Id,
            new SavedPatch[]
            {
                new(
                    AccessTools.Method(typeof(Tool), nameof(StardewValley.Tool.beginUsing)),
                    typeof(Configurator),
                    nameof(Configurator.Tool_beginUsing_prefix),
                    PatchType.Prefix),
                new(
                    AccessTools.Method(typeof(Tool), nameof(StardewValley.Tool.drawInMenu), new[] { typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool) }),
                    typeof(Configurator),
                    nameof(Configurator.Tool_drawInMenu_prefix),
                    PatchType.Prefix),
                new(
                    AccessTools.PropertyGetter(typeof(Tool), nameof(StardewValley.Tool.DisplayName)),
                    typeof(Configurator),
                    nameof(Configurator.Tool_getDisplayName_postfix),
                    PatchType.Postfix),
                new(
                    AccessTools.PropertyGetter(typeof(Tool), nameof(StardewValley.Tool.Description)),
                    typeof(Configurator),
                    nameof(Configurator.Tool_getDescription_postfix),
                    PatchType.Postfix),
            });
    }

    private static Configurator? Instance { get; set; }

    private ModConfig Config { get; }

    private IModHelper Helper { get; }

    private bool IsActivated { get; set; }

    private bool IsActive { get; set; }

    private Texture2D Tool
    {
        get => this._cachedTool ??= this.Helper.GameContent.Load<Texture2D>("furyx639.FuryCore/ConfigTool");
    }

    /// <summary>
    ///     Initializes <see cref="Configurator" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="config">Mod config data.</param>
    /// <returns>Returns an instance of the <see cref="Configurator" /> class.</returns>
    public static Configurator Init(IModHelper helper, ModConfig config)
    {
        return Configurator.Instance ??= new(helper, config);
    }

    /// <inheritdoc />
    public void Activate()
    {
        if (!this.IsActivated)
        {
            this.IsActivated = true;
            HarmonyHelper.ApplyPatches(Configurator.Id);
            this.Helper.Events.Display.MenuChanged += this.OnMenuChanged;
            this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;

            if (IntegrationHelper.ToolbarIcons.IsLoaded)
            {
                IntegrationHelper.ToolbarIcons.API.AddToolbarIcon(
                    "BetterChests.Configure",
                    "furyx639.BetterChests/Icons",
                    new(0, 0, 16, 16),
                    I18n.Button_Configure_Name());
                IntegrationHelper.ToolbarIcons.API.ToolbarIconPressed += this.OnToolbarIconPressed;
            }
        }
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (this.IsActivated)
        {
            this.IsActivated = false;
            HarmonyHelper.UnapplyPatches(Configurator.Id);
            this.Helper.Events.Display.MenuChanged += this.OnMenuChanged;
            this.Helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
            this.Helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;

            if (IntegrationHelper.ToolbarIcons.IsLoaded)
            {
                IntegrationHelper.ToolbarIcons.API.RemoveToolbarIcon("BetterChests.CraftFromChest");
                IntegrationHelper.ToolbarIcons.API.ToolbarIconPressed -= this.OnToolbarIconPressed;
            }
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static bool Tool_beginUsing_prefix(Tool __instance, Farmer who, ref bool __result)
    {
        if (!__instance.modData.TryGetValue(Configurator.ToolKey, out var toolName))
        {
            return true;
        }

        switch (toolName)
        {
            case Configurator.ToolName:
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
        if (!__instance.modData.TryGetValue(Configurator.ToolKey, out var toolName))
        {
            return true;
        }

        switch (toolName)
        {
            case Configurator.ToolName:
                spriteBatch.Draw(Configurator.Instance!.Tool, location + new Vector2(32f, 32f), new Rectangle(0, 0, 16, 16), color * transparency, 0f, new(8f, 8f), 4f * scaleSize, SpriteEffects.None, layerDepth);
                return false;
        }

        return true;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static void Tool_getDescription_postfix(Tool __instance, ref string __result)
    {
        if (!__instance.modData.TryGetValue(Configurator.ToolKey, out var toolName))
        {
            return;
        }

        switch (toolName)
        {
            case Configurator.ToolName:
                __result = I18n.Tool_ConfigTool_Description();
                return;
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static void Tool_getDisplayName_postfix(Tool __instance, ref string __result)
    {
        if (!__instance.modData.TryGetValue(Configurator.ToolKey, out var toolName))
        {
            return;
        }

        switch (toolName)
        {
            case Configurator.ToolName:
                __result = I18n.Tool_ConfigTool_Name();
                return;
        }
    }

    [EventPriority(EventPriority.Normal + 100)]
    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree
            || !e.Button.IsUseToolButton()
            || this.Helper.Input.IsSuppressed(e.Button)
            || Game1.player.CurrentItem is not GenericTool genericTool
            || !genericTool.modData.TryGetValue(Configurator.ToolKey, out var toolName)
            || toolName != Configurator.ToolName)
        {
            return;
        }

        var pos = new Vector2(Game1.getOldMouseX() + Game1.viewport.X, Game1.getOldMouseY() + Game1.viewport.Y) / Game1.tileSize;
        if (!Game1.wasMouseVisibleThisFrame || Game1.mouseCursorTransparency == 0f || !Utility.tileWithinRadiusOfPlayer((int)pos.X, (int)pos.Y, 1, Game1.player))
        {
            pos = Game1.player.GetGrabTile();
        }

        pos.X = (int)pos.X;
        pos.Y = (int)pos.Y;
        if (!Game1.currentLocation.Objects.TryGetValue(pos, out var obj) || !StorageHelper.TryGetOne(obj, out var storage))
        {
            return;
        }

        this.Helper.Input.Suppress(e.Button);
        ConfigHelper.SetupSpecificConfig(storage.Data);
        this.IsActive = true;
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!Context.IsPlayerFree
            || !this.Config.ControlScheme.Configure.JustPressed()
            || Game1.player.CurrentItem is not SObject obj
            || !StorageHelper.TryGetOne(obj, out var storage))
        {
            return;
        }

        this.Helper.Input.SuppressActiveKeybinds(this.Config.ControlScheme.Configure);
        ConfigHelper.SetupSpecificConfig(storage.Data);
        this.IsActive = true;
    }

    private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (this.IsActive && e.OldMenu?.GetType().Name == "SpecificModConfigMenu")
        {
            this.IsActive = false;
            ConfigHelper.SetupMainConfig();

            if (e.NewMenu?.GetType().Name == "ModConfigMenu")
            {
                Game1.activeClickableMenu = null;
            }
        }
    }

    private void OnToolbarIconPressed(object? sender, string id)
    {
        if (id != "BetterChests.Configure")
        {
            return;
        }

        if (Game1.player.CurrentItem is SObject obj
            && StorageHelper.TryGetOne(obj, out var storage))
        {
            ConfigHelper.SetupSpecificConfig(storage.Data);
            this.IsActive = true;
            return;
        }

        var pos = Game1.player.GetGrabTile();
        if (!Game1.currentLocation.Objects.TryGetValue(pos, out obj) || !StorageHelper.TryGetOne(obj, out storage))
        {
            return;
        }

        ConfigHelper.SetupSpecificConfig(storage.Data);
        this.IsActive = true;
    }
}