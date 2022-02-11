namespace StardewMods.BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using Common.Helpers;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Interfaces;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Models;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

/// <inheritdoc />
internal class CraftFromChest : Feature
{
    private readonly Lazy<IHarmonyHelper> _harmony;
    private readonly PerScreen<MultipleChestCraftingPage> _multipleChestCraftingPage = new();

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
    }

    /// <summary>
    ///     Gets a value indicating which chests are eligible for crafting from.
    /// </summary>
    public IEnumerable<IManagedStorage> EligibleChests
    {
        get
        {
            IList<IManagedStorage> eligibleStorages =
                this.ManagedStorages.InventoryStorages
                    .Select(inventoryStorage => inventoryStorage.Value)
                    .Where(playerChest => playerChest.CraftFromChest >= FeatureOptionRange.Inventory && playerChest.OpenHeldChest == FeatureOption.Enabled)
                    .ToList();
            foreach (var ((location, (x, y)), locationStorage) in this.ManagedStorages.LocationStorages)
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
                    case FeatureOptionRange.Location when !location.Equals(Game1.currentLocation):
                        continue;
                    case FeatureOptionRange.World:
                    case FeatureOptionRange.Location when locationStorage.CraftFromChestDistance == -1:
                    case FeatureOptionRange.Location when Utility.withinRadiusOfPlayer((int)x * 64, (int)y * 64, locationStorage.CraftFromChestDistance, Game1.player):
                        eligibleStorages.Add(locationStorage);
                        continue;
                    case FeatureOptionRange.Default:
                    case FeatureOptionRange.Disabled:
                    case FeatureOptionRange.Inventory:
                    default:
                        continue;
                }
            }

            return eligibleStorages;
        }
    }

    private IHarmonyHelper Harmony
    {
        get => this._harmony.Value;
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        this.Harmony.ApplyPatches(this.Id);
        this.Helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        this.Harmony.UnapplyPatches(this.Id);
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

        var eligibleChests = this.EligibleChests.ToList();
        if (!eligibleChests.Any())
        {
            Game1.showRedMessage(I18n.Alert_CraftFromChest_NoEligible());
            return;
        }

        Log.Trace("Launching CraftFromChest Menu.");
        this._multipleChestCraftingPage.Value = new(eligibleChests);
        this.Helper.Input.SuppressActiveKeybinds(this.Config.ControlScheme.OpenCrafting);
    }

    private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
    {
        if (this._multipleChestCraftingPage.Value is null || this._multipleChestCraftingPage.Value.Timeout)
        {
            return;
        }

        this._multipleChestCraftingPage.Value.UpdateChests();
    }

    private class MultipleChestCraftingPage
    {
        private const int TimeOut = 60;
        private readonly List<Chest> _chests;
        private readonly MultipleMutexRequest _multipleMutexRequest;
        private int _timeOut = MultipleChestCraftingPage.TimeOut;

        public MultipleChestCraftingPage(IEnumerable<IManagedStorage> managedChests)
        {
            this._chests = managedChests.Select(managedChest => managedChest.Context).OfType<Chest>().Where(chest => !chest.mutex.IsLocked()).ToList();
            var mutexes = this._chests.Select(chest => chest.mutex).ToList();
            this._multipleMutexRequest = new(
                mutexes,
                this.SuccessCallback,
                this.FailureCallback);
        }

        public bool Timeout
        {
            get => this._timeOut <= 0;
        }

        public void UpdateChests()
        {
            if (--this._timeOut <= 0)
            {
                return;
            }

            foreach (var chest in this._chests)
            {
                chest.mutex.Update(Game1.getOnlineFarmers());
            }
        }

        private void ExitFunction()
        {
            this._multipleMutexRequest.ReleaseLocks();
            this._timeOut = MultipleChestCraftingPage.TimeOut;
        }

        private void FailureCallback()
        {
            Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Workbench_Chest_Warning"));
            this._timeOut = 0;
        }

        private void SuccessCallback()
        {
            this._timeOut = 0;
            var width = 800 + IClickableMenu.borderWidth * 2;
            var height = 600 + IClickableMenu.borderWidth * 2;
            var (x, y) = Utility.getTopLeftPositionForCenteringOnScreen(width, height);
            Game1.activeClickableMenu = new CraftingPage((int)x, (int)y, width, height, false, true, this._chests)
            {
                exitFunction = this.ExitFunction,
            };
        }
    }
}