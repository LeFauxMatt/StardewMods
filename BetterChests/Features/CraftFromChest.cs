namespace StardewMods.BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using Common.Helpers;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Interfaces.Config;
using StardewMods.BetterChests.Interfaces.ManagedObjects;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.ClickableComponents;
using StardewMods.FuryCore.Models;
using StardewMods.FuryCore.Models.ClickableComponents;
using StardewMods.FuryCore.Models.CustomEvents;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

/// <inheritdoc />
internal class CraftFromChest : Feature
{
    private readonly PerScreen<IClickableComponent> _craftButton = new();
    private readonly Lazy<IHarmonyHelper> _harmony;
    private readonly PerScreen<MultipleChestCraftingPage> _multipleChestCraftingPage = new();
    private readonly Lazy<IHudComponents> _toolbarIcons;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CraftFromChest" /> class.
    /// </summary>
    /// <param name="config">Data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public CraftFromChest(IConfigModel config, IModHelper helper, IModServices services)
        : base(config, helper, services)
    {
        this._harmony = services.Lazy<IHarmonyHelper>(
            harmony =>
            {
                harmony.AddPatches(
                    this.Id,
                    new SavedPatch[]
                    {
                        new(
                            AccessTools.Method(typeof(CraftingRecipe), nameof(CraftingRecipe.consumeIngredients)),
                            typeof(CraftFromChest),
                            nameof(CraftFromChest.CraftingRecipe_consumeIngredients_transpiler),
                            PatchType.Transpiler),
                        new(
                            AccessTools.Method(typeof(CraftingPage), "getContainerContents"),
                            typeof(CraftFromChest),
                            nameof(CraftFromChest.CraftingPage_getContainerContents_postfix),
                            PatchType.Postfix),
                    });
            });
        this._toolbarIcons = services.Lazy<IHudComponents>();
    }

    /// <summary>
    ///     Gets a value indicating which chests are eligible for crafting from.
    /// </summary>
    public List<KeyValuePair<IGameObjectType, IManagedStorage>> EligibleStorages
    {
        get
        {
            var storages = new List<KeyValuePair<IGameObjectType, IManagedStorage>>();
            storages.AddRange(
                from inventoryStorage in this.ManagedObjects.InventoryStorages
                where inventoryStorage.Value.CraftFromChest >= FeatureOptionRange.Inventory
                      && inventoryStorage.Value.OpenHeldChest == FeatureOption.Enabled
                select new KeyValuePair<IGameObjectType, IManagedStorage>(inventoryStorage.Key, inventoryStorage.Value));

            foreach (var (locationObject, locationStorage) in this.ManagedObjects.LocationStorages)
            {
                // Disabled in config or by location name
                if (locationStorage.CraftFromChest == FeatureOptionRange.Disabled || locationStorage.CraftFromChestDisableLocations.Contains(Game1.player.currentLocation.Name))
                {
                    continue;
                }

                // Disabled in mines
                if (locationStorage.CraftFromChestDisableLocations.Contains("UndergroundMine") && Game1.player.currentLocation is MineShaft mineShaft && mineShaft.Name.StartsWith("UndergroundMine"))
                {
                    continue;
                }

                switch (locationStorage.CraftFromChest)
                {
                    // Disabled if not current location for location chest
                    case FeatureOptionRange.Location when !locationObject.Location.Equals(Game1.currentLocation):
                        continue;
                    case FeatureOptionRange.World:
                    case FeatureOptionRange.Location when locationStorage.CraftFromChestDistance == -1:
                    case FeatureOptionRange.Location when Utility.withinRadiusOfPlayer((int)locationObject.Position.X * 64, (int)locationObject.Position.Y * 64, locationStorage.CraftFromChestDistance, Game1.player):
                        storages.Add(new(locationObject, locationStorage));
                        continue;
                    case FeatureOptionRange.Default:
                    case FeatureOptionRange.Disabled:
                    case FeatureOptionRange.Inventory:
                    default:
                        continue;
                }
            }

            return storages.OrderByDescending(storage => storage.Value.StashToChestPriority).ToList();
        }
    }

    private IClickableComponent CraftButton
    {
        get => this._craftButton.Value ??= new CustomClickableComponent(
            new(
                new(0, 0, 32, 32),
                this.Helper.Content.Load<Texture2D>($"{BetterChests.ModUniqueId}/Icons", ContentSource.GameContent),
                new(32, 0, 16, 16),
                2f)
            {
                name = "Craft from Chest",
                hoverText = I18n.Button_CraftFromChest_Name(),
            },
            ComponentArea.Right);
    }

    private IHarmonyHelper Harmony
    {
        get => this._harmony.Value;
    }

    private IHudComponents HudComponents
    {
        get => this._toolbarIcons.Value;
    }

    private MultipleChestCraftingPage MultipleChestCraftingPage
    {
        get => this._multipleChestCraftingPage.Value;
        set => this._multipleChestCraftingPage.Value = value;
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        this.HudComponents.AddToolbarIcon(this.CraftButton);
        this.Harmony.ApplyPatches(this.Id);
        this.CustomEvents.HudComponentPressed += this.OnHudComponentPressed;
        this.Helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        this.HudComponents.RemoveToolbarIcon(this.CraftButton);
        this.Harmony.UnapplyPatches(this.Id);
        this.CustomEvents.HudComponentPressed -= this.OnHudComponentPressed;
        this.Helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
        this.Helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static void CraftingPage_getContainerContents_postfix(CraftingPage __instance, ref IList<Item> __result)
    {
        if (__instance._materialContainers is null)
        {
            return;
        }

        __result.Clear();
        var items = new List<Item>();
        foreach (var chest in __instance._materialContainers)
        {
            items.AddRange(chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID));
        }

        __result = items;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    private static IEnumerable<CodeInstruction> CraftingRecipe_consumeIngredients_transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.opcode == OpCodes.Ldfld && instruction.operand.Equals(AccessTools.Field(typeof(Chest), nameof(Chest.items))))
            {
                yield return new(OpCodes.Call, AccessTools.Property(typeof(Game1), nameof(Game1.player)).GetGetMethod());
                yield return new(OpCodes.Callvirt, AccessTools.Property(typeof(Farmer), nameof(Farmer.UniqueMultiplayerID)).GetGetMethod());
                yield return new(OpCodes.Callvirt, AccessTools.Method(typeof(Chest), nameof(Chest.GetItemsForPlayer)));
            }
            else
            {
                yield return instruction;
            }
        }
    }

    /// <summary>Open crafting menu for all chests in inventory.</summary>
    private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
    {
        if (!Context.IsPlayerFree || !this.Config.ControlScheme.OpenCrafting.JustPressed())
        {
            return;
        }

        this.OpenCrafting();
        this.Helper.Input.SuppressActiveKeybinds(this.Config.ControlScheme.OpenCrafting);
    }

    private void OnHudComponentPressed(object sender, ClickableComponentPressedEventArgs e)
    {
        if (ReferenceEquals(this.CraftButton, e.Component))
        {
            this.OpenCrafting();
            e.SuppressInput();
        }
    }

    private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
    {
        if (this.MultipleChestCraftingPage is null || this.MultipleChestCraftingPage.TimedOut)
        {
            return;
        }

        this.MultipleChestCraftingPage.UpdateChests();
    }

    private void OpenCrafting()
    {
        var eligibleStorages = this.EligibleStorages;
        if (!eligibleStorages.Any())
        {
            Game1.showRedMessage(I18n.Alert_CraftFromChest_NoEligible());
            return;
        }

        Log.Trace("Launching CraftFromChest Menu.");
        this.MultipleChestCraftingPage = new(eligibleStorages);
        this.Helper.Input.SuppressActiveKeybinds(this.Config.ControlScheme.OpenCrafting);
    }
}