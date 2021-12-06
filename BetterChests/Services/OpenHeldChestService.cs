namespace BetterChests.Services
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Common.Enums;
    using Common.Helpers;
    using Common.Services;
    using CommonHarmony.Services;
    using HarmonyLib;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewValley;
    using StardewValley.Objects;

    internal class OpenHeldChestService : BaseService, IFeatureService
    {
        private readonly IModHelper _helper;
        private HarmonyService _harmony;
        private ManagedChestService _managedChestService;

        private OpenHeldChestService(ServiceManager serviceManager)
            : base("OpenHeldChest")
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
                        AccessTools.Method(typeof(Chest), nameof(Chest.addItem)),
                        typeof(OpenHeldChestService),
                        nameof(OpenHeldChestService.Chest_addItem_prefix));
                });
        }

        /// <inheritdoc />
        public void Activate()
        {
            // Events
            this._helper.Events.GameLoop.UpdateTicked += OpenHeldChestService.OnUpdateTicked;
            this._helper.Events.Input.ButtonPressed += this.OnButtonPressed;

            // Patches
            this._harmony.ApplyPatches(this.ServiceName);
        }

        /// <inheritdoc />
        public void Deactivate()
        {
            // Events
            this._helper.Events.GameLoop.UpdateTicked -= OpenHeldChestService.OnUpdateTicked;
            this._helper.Events.Input.ButtonPressed -= this.OnButtonPressed;

            // Patches
            this._harmony.UnapplyPatches(this.ServiceName);
        }

        /// <summary>Prevent adding chest into itself.</summary>
        [HarmonyPriority(Priority.High)]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
        private static bool Chest_addItem_prefix(Chest __instance, ref Item __result, Item item)
        {
            if (!ReferenceEquals(__instance, item))
            {
                return true;
            }

            __result = item;
            return false;
        }

        private static void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsPlayerFree)
            {
                return;
            }

            foreach (var chest in Game1.player.Items.Take(12).OfType<Chest>())
            {
                chest.updateWhenCurrentLocation(Game1.currentGameTime, Game1.player.currentLocation);
            }
        }

        /// <summary>Open inventory for currently held chest.</summary>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsPlayerFree || !e.Button.IsActionButton() || Game1.player.CurrentItem is not Chest chest)
            {
                return;
            }

            if (!this._managedChestService.TryGetChestConfig(chest, out var chestConfig) || chestConfig.AccessCarried != FeatureOption.Enabled)
            {
                return;
            }

            Log.Trace($"Opening Menu for Carried ${chest.Name}.");
            if (Context.IsMainPlayer)
            {
                chest.checkForAction(Game1.player);
            }
            else
            {
                chest.ShowMenu();
            }

            this._helper.Input.Suppress(e.Button);
        }
    }
}