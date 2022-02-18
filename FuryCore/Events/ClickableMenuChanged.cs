namespace StardewMods.FuryCore.Events;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Models;
using StardewMods.FuryCore.Models.CustomEvents;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc />
internal class ClickableMenuChanged : SortedEventHandler<ClickableMenuChangedEventArgs>
{
    private readonly PerScreen<IClickableMenu> _menu = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickableMenuChanged" /> class.
    /// </summary>
    /// <param name="gameLoop">SMAPI events linked to the the game's update loop.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public ClickableMenuChanged(IGameLoopEvents gameLoop, IModServices services)
    {
        ClickableMenuChanged.Instance ??= this;

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
                            nameof(ClickableMenuChanged.IClickableMenu_constructor_postfix),
                            PatchType.Postfix),
                    });
                harmonyHelper.ApplyPatches(id);
            });
        gameLoop.UpdateTicked += this.OnUpdateTicked;
        gameLoop.UpdateTicking += this.OnUpdateTicking;
    }

    private static ClickableMenuChanged Instance { get; set; }

    private IClickableMenu Menu
    {
        get => this._menu.Value;
        set => this._menu.Value = value;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static void IClickableMenu_constructor_postfix(IClickableMenu __instance)
    {
        switch (__instance)
        {
            case ItemGrabMenu:
                ClickableMenuChanged.Instance.InvokeAll(new(__instance, Context.ScreenId, true));
                break;
        }
    }

    [SuppressMessage("StyleCop", "SA1101", Justification = "This is a pattern match not a local call")]
    private void InvokeIfMenuChanged()
    {
        if (!ReferenceEquals(this.Menu, Game1.activeClickableMenu))
        {
            this.Menu = Game1.activeClickableMenu;
            this.InvokeAll(new(this.Menu, Context.ScreenId, false));
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