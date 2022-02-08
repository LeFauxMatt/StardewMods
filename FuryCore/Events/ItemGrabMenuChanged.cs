namespace StardewMods.FuryCore.Events;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Common.Extensions;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Models;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

/// <inheritdoc />
internal class ItemGrabMenuChanged : SortedEventHandler<ItemGrabMenuChangedEventArgs>
{
    private readonly PerScreen<IClickableMenu> _menu = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="ItemGrabMenuChanged" /> class.
    /// </summary>
    /// <param name="gameLoop">SMAPI events linked to the the game's update loop.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public ItemGrabMenuChanged(IGameLoopEvents gameLoop, IModServices services)
    {
        ItemGrabMenuChanged.Instance ??= this;

        services.Lazy<IHarmonyHelper>(
            harmonyHelper =>
            {
                var id = $"{FuryCore.ModUniqueId}.{nameof(ItemGrabMenuChanged)}";
                var ctorParams = new[]
                {
                    typeof(IList<Item>), typeof(bool), typeof(bool), typeof(InventoryMenu.highlightThisItem), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(string), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(Item), typeof(int), typeof(object),
                };

                harmonyHelper.AddPatch(
                    id,
                    AccessTools.Constructor(typeof(ItemGrabMenu), ctorParams),
                    typeof(ItemGrabMenuChanged),
                    nameof(ItemGrabMenuChanged.ItemGrabMenu_constructor_postfix),
                    PatchType.Postfix);

                harmonyHelper.ApplyPatches(id);
            });

        gameLoop.UpdateTicked += this.OnUpdateTicked;
        gameLoop.UpdateTicking += this.OnUpdateTicking;
    }

    private static ItemGrabMenuChanged Instance { get; set; }

    private IClickableMenu Menu
    {
        get => this._menu.Value;
        set => this._menu.Value = value;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static void ItemGrabMenu_constructor_postfix(ItemGrabMenu __instance)
    {
        ItemGrabMenuChanged.Instance.Menu = __instance;
        ItemGrabMenuChanged.Instance.InvokeAll(new(__instance, __instance.context, Context.ScreenId, true));
    }

    [SuppressMessage("StyleCop", "SA1101", Justification = "This is a pattern match not a local call")]
    private void InvokeIfMenuChanged()
    {
        var menu = Game1.activeClickableMenu;

        if (ReferenceEquals(this.Menu, menu))
        {
            return;
        }

        this.Menu = Game1.activeClickableMenu;
        if (this.HandlerCount == 0)
        {
            return;
        }

        if (this.Menu is ItemGrabMenu itemGrabMenu)
        {
            this.InvokeAll(new(itemGrabMenu, itemGrabMenu.context, Context.ScreenId, false));
            return;
        }

        this.InvokeAll(new(null, null, Context.ScreenId, false));
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