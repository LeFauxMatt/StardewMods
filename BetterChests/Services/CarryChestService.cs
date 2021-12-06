namespace BetterChests.Services
{
    using System;
    using System.Linq;
    using Common.Enums;
    using Common.Extensions;
    using Common.Helpers;
    using Common.Services;
    using CommonHarmony.Enums;
    using CommonHarmony.Services;
    using HarmonyLib;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Objects;

    internal class CarryChestService : BaseService, IFeatureService
    {
        private readonly IModHelper _helper;
        private readonly PerScreen<Chest> _currentChest = new();
        private HarmonyService _harmony;
        private ManagedChestService _managedChestService;

        private CarryChestService(ServiceManager serviceManager)
            : base("CarryChest")
        {
            // Init
            this._helper = serviceManager.Helper;

            // Dependencies
            this.AddDependency<ManagedChestService>(service => this._managedChestService = service as ManagedChestService);
            this.AddDependency<HarmonyService>(
                service =>
                {
                    // Init
                    this._harmony = service as HarmonyService;

                    // Patches
                    this._harmony?.AddPatch(
                        this.ServiceName,
                        AccessTools.Method(typeof(Utility), nameof(Utility.iterateChestsAndStorage)),
                        typeof(CarryChestService),
                        nameof(CarryChestService.Utility_iterateChestsAndStorage_postfix),
                        PatchType.Postfix);
                });
        }

        /// <inheritdoc />
        public void Activate()
        {
            // Events
            this._helper.Events.GameLoop.UpdateTicking += this.OnUpdateTicking;
            this._helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            this._helper.Events.World.ObjectListChanged += this.OnObjectListChanged;

            // Patches
            this._harmony.ApplyPatches(this.ServiceName);
        }

        /// <inheritdoc />
        public void Deactivate()
        {
            // Events
            this._helper.Events.GameLoop.UpdateTicking -= this.OnUpdateTicking;
            this._helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
            this._helper.Events.World.ObjectListChanged -= this.OnObjectListChanged;

            // Patches
            this._harmony.UnapplyPatches(this.ServiceName);
        }

        private static void Utility_iterateChestsAndStorage_postfix(Action<Item> action)
        {
            Log.Verbose("Recursively iterating chests in farmer inventory.");
            foreach (var farmer in Game1.getAllFarmers())
            {
                foreach (var chest in farmer.Items.OfType<Chest>())
                {
                    chest.RecursiveIterate(action);
                }
            }
        }

        private void OnUpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            if (Context.IsPlayerFree)
            {
                this._currentChest.Value = Game1.player.CurrentItem as Chest;
            }
        }

        [EventPriority(EventPriority.High)]
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsPlayerFree || !e.Button.IsUseToolButton() || Game1.player.CurrentTool is not null)
            {
                return;
            }

            var pos = e.Button.TryGetController(out _) ? Game1.player.GetToolLocation() / 64f : e.Cursor.Tile;
            pos.X = (int)pos.X;
            pos.Y = (int)pos.Y;

            // Object exists at pos and is within reach of player
            if (!Utility.withinRadiusOfPlayer((int)(64 * pos.X), (int)(64 * pos.Y), 1, Game1.player)
                || !Game1.currentLocation.Objects.TryGetValue(pos, out var obj))
            {
                return;
            }

            if (!this._managedChestService.TryGetChestConfig(obj, out var chestConfig) || chestConfig.CarryChest != FeatureOption.Enabled)
            {
                return;
            }

            if (!Game1.player.addItemToInventoryBool(obj, true))
            {
                return;
            }

            Game1.currentLocation.Objects.Remove(pos);
            this._helper.Input.Suppress(e.Button);
        }

        [EventPriority(EventPriority.High)]
        private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            if (!e.IsCurrentLocation || this._currentChest.Value is null)
            {
                return;
            }

            var chest = e.Added.Select(added => added.Value).OfType<Chest>().SingleOrDefault();
            if (chest is not null && this._managedChestService.TryGetChestConfig(this._currentChest.Value, out var chestConfig) && chestConfig.CarryChest == FeatureOption.Enabled)
            {
                chest.Name = this._currentChest.Value.Name;
                chest.SpecialChestType = this._currentChest.Value.SpecialChestType;
                chest.fridge.Value = this._currentChest.Value.fridge.Value;
                chest.lidFrameCount.Value = this._currentChest.Value.lidFrameCount.Value;
                chest.playerChoiceColor.Value = this._currentChest.Value.playerChoiceColor.Value;

                if (this._currentChest.Value.items.Any())
                {
                    chest.items.CopyFrom(this._currentChest.Value.items);
                }

                foreach (var (key, value) in this._currentChest.Value.modData.Pairs)
                {
                    chest.modData[key] = value;
                }
            }
        }
    }
}