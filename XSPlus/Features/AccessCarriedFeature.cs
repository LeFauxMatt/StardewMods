namespace XSPlus.Features
{
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Common.Helpers;
    using CommonHarmony.Services;
    using HarmonyLib;
    using Services;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewValley;
    using StardewValley.Objects;

    /// <inheritdoc />
    internal class AccessCarriedFeature : BaseFeature
    {
        private MixInfo _addItemPatch;

        private AccessCarriedFeature(ModConfigService modConfigService)
            : base("AccessCarried", modConfigService)
        {
        }

        /// <summary>
        ///     Gets or sets the instance of <see cref="AccessCarriedFeature" />.
        /// </summary>
        private static AccessCarriedFeature Instance { get; set; }

        /// <summary>
        ///     Returns and creates if needed an instance of the <see cref="AccessCarriedFeature" /> class.
        /// </summary>
        /// <param name="serviceManager">Service manager to request shared services.</param>
        /// <returns>Returns an instance of the <see cref="AccessCarriedFeature" /> class.</returns>
        public static async Task<AccessCarriedFeature> Create(ServiceManager serviceManager)
        {
            return AccessCarriedFeature.Instance ??= new(await serviceManager.Get<ModConfigService>());
        }

        /// <inheritdoc />
        public override void Activate()
        {
            // Events
            Events.Input.ButtonPressed += this.OnButtonPressed;

            // Patches
            this._addItemPatch = Mixin.Prefix(
                AccessTools.Method(typeof(Chest), nameof(Chest.addItem)),
                typeof(AccessCarriedFeature),
                nameof(AccessCarriedFeature.Chest_addItem_prefix));
        }

        /// <inheritdoc />
        public override void Deactivate()
        {
            // Events
            Events.Input.ButtonPressed -= this.OnButtonPressed;

            // Patches
            Mixin.Unpatch(this._addItemPatch);
        }

        /// <summary>Prevent adding chest into itself.</summary>
        [HarmonyPriority(Priority.High)]
        [SuppressMessage("ReSharper", "SA1313", Justification = "Naming is determined by Harmony.")]
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

        /// <summary>Open inventory for currently held chest.</summary>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsPlayerFree || !e.Button.IsActionButton() || Game1.player.CurrentItem is not Chest chest || !this.IsEnabledForItem(chest))
            {
                return;
            }

            Log.Trace("Opening Menu for Carried Chest.");
            chest.checkForAction(Game1.player);
            Input.Suppress(e.Button);
        }
    }
}