namespace XSPlus.Features
{
    using System;
    using System.Linq;
    using Common.Extensions;
    using Common.Helpers;
    using Common.Services;
    using CommonHarmony.Enums;
    using CommonHarmony.Services;
    using HarmonyLib;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewValley;
    using StardewValley.Objects;
    using SObject = StardewValley.Object;

    internal class CarryChestFeature : BaseFeature
    {
        private HarmonyService _harmony;

        private CarryChestFeature(ServiceManager serviceManager)
            : base("CarryChest", serviceManager)
        {
            // Dependencies
            this.AddDependency<HarmonyService>(
                service =>
                {
                    // Init
                    this._harmony = service as HarmonyService;

                    // Patches
                    this._harmony?.AddPatch(
                        this.ServiceName,
                        AccessTools.Method(typeof(Utility), nameof(Utility.iterateChestsAndStorage)),
                        typeof(CarryChestFeature),
                        nameof(CarryChestFeature.Utility_iterateChestsAndStorage_postfix),
                        PatchType.Postfix);
                });
        }

        /// <inheritdoc />
        public override void Activate()
        {
            // Events
            this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;

            // Patches
            this._harmony.ApplyPatches(this.ServiceName);
        }

        /// <inheritdoc />
        public override void Deactivate()
        {
            // Events
            this.Helper.Events.Input.ButtonPressed -= this.OnButtonPressed;

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

            // Reassign to origin object if applicable
            if (BiggerChestFeature.TryGetOriginObject(Game1.currentLocation, obj, out var sourceObj, out var originPos))
            {
                obj = sourceObj;
                pos = originPos;
            }

            if (!this.IsEnabledForItem(obj) || !Game1.player.addItemToInventoryBool(obj, true))
            {
                return;
            }

            Game1.currentLocation.Objects.Remove(pos);
            this.Helper.Input.Suppress(e.Button);
        }
    }
}