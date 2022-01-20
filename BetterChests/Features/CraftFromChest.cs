namespace BetterChests.Features;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using BetterChests.Enums;
using Common.Helpers;
using FuryCore.Enums;
using FuryCore.Interfaces;
using FuryCore.Models;
using FuryCore.Services;
using HarmonyLib;
using Models;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

/// <inheritdoc />
internal class CraftFromChest : Feature
{
    private readonly PerScreen<MultipleChestCraftingPage> _multipleChestCraftingPage = new();
    private readonly Lazy<IHarmonyHelper> _harmony;

    /// <summary>
    /// Initializes a new instance of the <see cref="CraftFromChest"/> class.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="helper"></param>
    /// <param name="services"></param>
    public CraftFromChest(ModConfig config, IModHelper helper, ServiceCollection services)
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

    private IHarmonyHelper Harmony
    {
        get => this._harmony.Value;
    }

    /// <inheritdoc />
    public override void Activate()
    {
        this.Harmony.ApplyPatches(this.Id);
        this.Helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
    }

    /// <inheritdoc />
    public override void Deactivate()
    {
        this.Harmony.UnapplyPatches(this.Id);
        this.Helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
        this.Helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
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

    private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
    {
        if (this._multipleChestCraftingPage.Value is null || this._multipleChestCraftingPage.Value.Timeout)
        {
            return;
        }

        this._multipleChestCraftingPage.Value.UpdateChests();
    }

    /// <summary>Open crafting menu for all chests in inventory.</summary>
    private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
    {
        if (!Context.IsPlayerFree || !this.Config.OpenCrafting.JustPressed())
        {
            return;
        }

        var eligibleChests = (
                from item in this.ManagedChests.AccessibleChests
                where item.Value.Config.CraftingRange switch
                {
                    FeatureOptionRange.Inventory => ReferenceEquals(item.Key.Player, Game1.player),
                    FeatureOptionRange.Location => ReferenceEquals(item.Key.Location, Game1.currentLocation),
                    FeatureOptionRange.World => true,
                    _ => false,
                }
                select item.Value.Chest).ToList();

        if (!eligibleChests.Any())
        {
            Log.Trace("No eligible chests found to craft items from");
            return;
        }

        this._multipleChestCraftingPage.Value = new(eligibleChests);
        this.Helper.Input.SuppressActiveKeybinds(this.Config.OpenCrafting);
    }

    private class MultipleChestCraftingPage
    {
        private const int TimeOut = 60;
        private readonly List<Chest> _chests;
        private readonly MultipleMutexRequest _multipleMutexRequest;
        private int _timeOut = MultipleChestCraftingPage.TimeOut;

        public MultipleChestCraftingPage(List<Chest> managedChests)
        {
            this._chests = managedChests.Where(chest => !chest.mutex.IsLocked()).ToList();
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

        private void SuccessCallback()
        {
            this._timeOut = 0;
            var width = 800 + (IClickableMenu.borderWidth * 2);
            var height = 600 + (IClickableMenu.borderWidth * 2);
            var (x, y) = Utility.getTopLeftPositionForCenteringOnScreen(width, height);
            Game1.activeClickableMenu = new CraftingPage((int)x, (int)y, width, height, false, true, this._chests)
            {
                exitFunction = this.ExitFunction,
            };
        }

        private void FailureCallback()
        {
            Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Workbench_Chest_Warning"));
            this._timeOut = 0;
        }

        private void ExitFunction()
        {
            this._multipleMutexRequest.ReleaseLocks();
            this._timeOut = MultipleChestCraftingPage.TimeOut;
        }
    }
}