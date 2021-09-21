namespace XSPlus.Features
{
    using System.Diagnostics.CodeAnalysis;
    using HarmonyLib;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewValley;
    using StardewValley.Objects;

    /// <inheritdoc />
    internal class AccessCarriedFeature : BaseFeature
    {
        private readonly IInputHelper _inputHelper;

        /// <summary>Initializes a new instance of the <see cref="AccessCarriedFeature"/> class.</summary>
        /// <param name="inputHelper">Provides an API for checking and changing input state.</param>
        public AccessCarriedFeature(IInputHelper inputHelper)
            : base("AccessCarried")
        {
            this._inputHelper = inputHelper;
        }

        /// <inheritdoc/>
        public override void Activate(IModEvents modEvents, Harmony harmony)
        {
            // Events
            modEvents.Input.ButtonPressed += this.OnButtonPressed;

            // Patches
            harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.addItem)),
                prefix: new HarmonyMethod(typeof(AccessCarriedFeature), nameof(AccessCarriedFeature.Chest_addItem_prefix)));
        }

        /// <inheritdoc/>
        public override void Deactivate(IModEvents modEvents, Harmony harmony)
        {
            // Events
            modEvents.Input.ButtonPressed -= this.OnButtonPressed;

            // Patches
            harmony.Unpatch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.addItem)),
                patch: AccessTools.Method(typeof(AccessCarriedFeature), nameof(AccessCarriedFeature.Chest_addItem_prefix)));
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

            chest.checkForAction(Game1.player);
            this._inputHelper.Suppress(e.Button);
        }
    }
}