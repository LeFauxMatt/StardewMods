namespace StardewMods.FuryCore.Events;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewMods.FuryCore.Models;
using StardewMods.FuryCore.Models.CustomEvents;
using StardewMods.FuryCore.Services;
using StardewValley;
using StardewValley.Menus;
using SObject = StardewValley.Object;

/// <inheritdoc />
internal class ClickableMenuChanged : SortedEventHandler<IClickableMenuChangedEventArgs>
{
    private readonly Lazy<GameObjects> _gameObjects;
    private readonly PerScreen<IClickableMenu> _menu = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickableMenuChanged" /> class.
    /// </summary>
    /// <param name="gameLoop">SMAPI events linked to the the game's update loop.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public ClickableMenuChanged(IGameLoopEvents gameLoop, IModServices services)
    {
        ClickableMenuChanged.Instance ??= this;
        this._gameObjects = services.Lazy<GameObjects>();
        gameLoop.UpdateTicked += this.OnUpdateTicked;
        gameLoop.UpdateTicking += this.OnUpdateTicking;
        services.Lazy<IHarmonyHelper>(
            harmonyHelper =>
            {
                var id = $"{FuryCore.ModUniqueId}.{nameof(ClickableMenuChanged)}";
                harmonyHelper.AddPatches(
                    id,
                    new SavedPatch[]
                    {
                        new(
                            AccessTools.Constructor(typeof(ItemGrabMenu), new[] { typeof(IList<Item>), typeof(bool), typeof(bool), typeof(InventoryMenu.highlightThisItem), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(string), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(Item), typeof(int), typeof(object) }),
                            typeof(ClickableMenuChanged),
                            nameof(ClickableMenuChanged.ItemGrabMenu_constructor_postfix),
                            PatchType.Postfix),
                        new(
                            AccessTools.Constructor(typeof(PurchaseAnimalsMenu)),
                            typeof(ClickableMenuChanged),
                            nameof(ClickableMenuChanged.PurchaseAnimalsMenu_constructor_postfix),
                            PatchType.Postfix),
                    });
                harmonyHelper.ApplyPatches(id);
            });
    }

    private static ClickableMenuChanged Instance { get; set; }

    private GameObjects GameObjects
    {
        get => this._gameObjects.Value;
    }

    private IClickableMenu Menu
    {
        get => this._menu.Value;
        set => this._menu.Value = value;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static void ItemGrabMenu_constructor_postfix(ItemGrabMenu __instance)
    {
        if (__instance is { context: { } context } && ClickableMenuChanged.Instance.GameObjects.TryGetGameObject(context, out var gameObject))
        {
            ClickableMenuChanged.Instance.InvokeAll(new ClickableMenuChangedEventArgs(__instance, Context.ScreenId, true, gameObject));
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static void PurchaseAnimalsMenu_constructor_postfix(IClickableMenu __instance)
    {
        ClickableMenuChanged.Instance.InvokeAll(new ClickableMenuChangedEventArgs(__instance, Context.ScreenId, true, null));
    }

    [SuppressMessage("StyleCop", "SA1101", Justification = "This is a pattern match not a local call")]
    private void InvokeIfMenuChanged()
    {
        if (ReferenceEquals(this.Menu, Game1.activeClickableMenu))
        {
            return;
        }

        this.Menu = Game1.activeClickableMenu;
        switch (this.Menu)
        {
            case ItemGrabMenu { context: { } context } when ClickableMenuChanged.Instance.GameObjects.TryGetGameObject(context, out var gameObject):
                this.InvokeAll(new ClickableMenuChangedEventArgs(this.Menu, Context.ScreenId, false, gameObject));
                break;
            default:
                this.InvokeAll(new ClickableMenuChangedEventArgs(this.Menu, Context.ScreenId, false, null));
                break;
        }
    }

    [EventPriority(EventPriority.Low - 1000)]
    private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
    {
        this.InvokeIfMenuChanged();
    }

    [EventPriority(EventPriority.Low - 1000)]
    private void OnUpdateTicking(object sender, UpdateTickingEventArgs e)
    {
        this.InvokeIfMenuChanged();
    }
}