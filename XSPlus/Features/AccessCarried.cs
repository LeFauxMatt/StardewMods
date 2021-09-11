using System.Diagnostics.CodeAnalysis;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;

namespace XSPlus.Features
{
    internal class AccessCarried : BaseFeature
    {
        public AccessCarried(string featureName, IModHelper helper, IMonitor monitor, Harmony harmony) : base(featureName, helper, monitor, harmony)
        {
        }
        protected override void EnableFeature()
        {
            // Events
            Helper.Events.Input.ButtonPressed += OnButtonPressed;
            
            // Patches
            Harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.addItem)),
                prefix: new HarmonyMethod(typeof(AccessCarried), nameof(AccessCarried.Chest_addItem_prefix))
            );
        }
        protected override void DisableFeature()
        {
            // Events
            Helper.Events.Input.ButtonPressed -= OnButtonPressed;
            
            // Patches
            Harmony.Unpatch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.addItem)),
                patch: AccessTools.Method(typeof(AccessCarried), nameof(AccessCarried.Chest_addItem_prefix))
            );
        }
        /// <summary>Open inventory for currently held chest</summary>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsPlayerFree || !e.Button.IsActionButton() || Game1.player.CurrentItem is not Chest chest || !IsEnabled(chest))
                return;
            chest.checkForAction(Game1.player);
            Helper.Input.Suppress(e.Button);
        }
        /// <summary>Prevent adding chest into itself</summary>
        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [HarmonyPriority(Priority.High)]
        private static bool Chest_addItem_prefix(Chest __instance, ref Item __result, Item item)
        {
            if (!ReferenceEquals(__instance,item))
                return true;
            __result = item;
            return false;
        }
    }
}