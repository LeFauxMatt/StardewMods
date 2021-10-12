namespace XSPlus.Features
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Common.Services;
    using CommonHarmony.Services;
    using HarmonyLib;
    using Microsoft.Xna.Framework;
    using StardewModdingAPI.Events;
    using StardewValley;
    using StardewValley.Objects;

    internal class OpenNearbyFeature : FeatureWithParam<float>
    {
        private static OpenNearbyFeature Instance;
        private BiggerChestFeature _biggerChest;
        private HarmonyService _harmony;

        private OpenNearbyFeature(ServiceManager serviceManager)
            : base("OpenNearby", serviceManager)
        {
            OpenNearbyFeature.Instance ??= this;

            // Dependencies
            this.AddDependency<BiggerChestFeature>(service => this._biggerChest = service as BiggerChestFeature);
            this.AddDependency<HarmonyService>(
                service =>
                {
                    // Init
                    this._harmony = service as HarmonyService;

                    // Patches
                    this._harmony?.AddPatch(
                        this.ServiceName,
                        AccessTools.Method(typeof(Chest), nameof(Chest.fixLidFrame)),
                        typeof(OpenNearbyFeature),
                        nameof(OpenNearbyFeature.Chest_fixLidFrame_prefix));

                    this._harmony?.AddPatch(
                        this.ServiceName,
                        AccessTools.Method(typeof(Chest), nameof(Chest.UpdateFarmerNearby)),
                        typeof(OpenNearbyFeature),
                        nameof(OpenNearbyFeature.Chest_UpdateFarmerNearby_prefix));

                    this._harmony?.AddPatch(
                        this.ServiceName,
                        AccessTools.Method(typeof(Chest), nameof(Chest.updateWhenCurrentLocation)),
                        typeof(OpenNearbyFeature),
                        nameof(OpenNearbyFeature.Chest_updateWhenCurrentLocation_prefix));
                });
        }

        /// <inheritdoc />
        public override void Activate()
        {
            // Events
            this.Helper.Events.Player.Warped += this.OnWarped;

            // Patches
            this._harmony.ApplyPatches(this.ServiceName);
        }

        /// <inheritdoc />
        public override void Deactivate()
        {
            // Events
            this.Helper.Events.Player.Warped += this.OnWarped;

            // Patches
            this._harmony.UnapplyPatches(this.ServiceName);
        }

        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
        private static bool Chest_fixLidFrame_prefix(Chest __instance, ref int ___currentLidFrame)
        {
            if (!OpenNearbyFeature.Instance.IsEnabledForItem(__instance) || !OpenNearbyFeature.Instance.TryGetValueForItem(__instance, out var distance) || distance <= 0)
            {
                return true;
            }

            if (___currentLidFrame == 0)
            {
                ___currentLidFrame = __instance.startingLidFrame.Value;
            }

            return false;
        }

        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
        private static bool Chest_updateWhenCurrentLocation_prefix(Chest __instance, ref int ___health, ref int ____shippingBinFrameCounter, ref bool ____farmerNearby, ref int ___currentLidFrame, GameTime time, GameLocation environment)
        {
            if (!OpenNearbyFeature.Instance.IsEnabledForItem(__instance) || !OpenNearbyFeature.Instance.TryGetValueForItem(__instance, out var distance) || distance <= 0)
            {
                return true;
            }

            if (__instance.synchronized.Value)
            {
                __instance.openChestEvent.Poll();
            }

            if (!__instance.localKickStartTile.HasValue)
            {
                __instance.kickProgress = -1f;
            }
            else
            {
                if (Game1.currentLocation.Equals(environment))
                {
                    if (__instance.kickProgress == 0f)
                    {
                        if (Utility.isOnScreen((__instance.localKickStartTile.Value + new Vector2(0.5f, 0.5f)) * 64f, 64))
                        {
                            environment.localSound("clubhit");
                        }

                        __instance.shakeTimer = 100;
                    }
                }
                else
                {
                    __instance.localKickStartTile = null;
                    __instance.kickProgress = -1f;
                }

                if (__instance.kickProgress >= 0f)
                {
                    __instance.kickProgress += (float)(time.ElapsedGameTime.TotalSeconds / 0.25f);
                    if (__instance.kickProgress >= 1f)
                    {
                        __instance.kickProgress = -1f;
                        __instance.localKickStartTile = null;
                    }
                }
            }

            __instance.fixLidFrame();
            __instance.mutex.Update(environment);
            if (__instance.shakeTimer > 0)
            {
                __instance.shakeTimer -= time.ElapsedGameTime.Milliseconds;
                if (__instance.shakeTimer <= 0)
                {
                    ___health = 10;
                }
            }

            if (distance <= 0)
            {
                if (__instance.frameCounter.Value > -1 && ___currentLidFrame < __instance.getLastLidFrame() + 1)
                {
                    __instance.frameCounter.Value--;
                    if (__instance.frameCounter.Value > 0 || !__instance.GetMutex().IsLockHeld())
                    {
                        return false;
                    }

                    if (___currentLidFrame == __instance.getLastLidFrame())
                    {
                        __instance.ShowMenu();
                        __instance.frameCounter.Value = -1;
                    }
                    else
                    {
                        __instance.frameCounter.Value = 5;
                        ___currentLidFrame++;
                    }
                }
                else if ((__instance.frameCounter.Value == -1 && ___currentLidFrame > __instance.startingLidFrame.Value || ___currentLidFrame >= __instance.getLastLidFrame()) && Game1.activeClickableMenu is null && __instance.GetMutex().IsLockHeld())
                {
                    __instance.GetMutex().ReleaseLock();
                    ___currentLidFrame = __instance.getLastLidFrame();
                    __instance.frameCounter.Value = 2;
                    environment.localSound("doorCreakReverse");
                }

                return false;
            }

            __instance.UpdateFarmerNearby(environment);
            if (____shippingBinFrameCounter <= -1)
            {
                return false;
            }

            ____shippingBinFrameCounter--;
            if (____shippingBinFrameCounter <= 0)
            {
                ____shippingBinFrameCounter = 5;
                switch (____farmerNearby)
                {
                    case true when ___currentLidFrame < __instance.getLastLidFrame():
                        ___currentLidFrame++;
                        break;
                    case false when ___currentLidFrame > __instance.startingLidFrame.Value:
                        ___currentLidFrame--;
                        break;
                    default:
                        ____shippingBinFrameCounter = -1;
                        break;
                }
            }

            if (Game1.activeClickableMenu is null && __instance.GetMutex().IsLockHeld())
            {
                __instance.GetMutex().ReleaseLock();
            }

            return false;
        }

        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
        private static bool Chest_UpdateFarmerNearby_prefix(Chest __instance, ref bool ____farmerNearby, ref int ____shippingBinFrameCounter, ref int ___currentLidFrame, GameLocation location, bool animate)
        {
            if (!OpenNearbyFeature.Instance.IsEnabledForItem(__instance) || !OpenNearbyFeature.Instance.TryGetValueForItem(__instance, out var distance) || distance <= 0)
            {
                return true;
            }

            if (ReferenceEquals(Game1.player.CurrentItem, __instance))
            {
                if (____farmerNearby)
                {
                    return false;
                }

                ____farmerNearby = true;
                ____shippingBinFrameCounter = 5;
                return false;
            }

            if (Game1.player.Items.Any(item => ReferenceEquals(item, __instance)))
            {
                if (!____farmerNearby)
                {
                    return false;
                }

                ____farmerNearby = false;
                ____shippingBinFrameCounter = 5;
                return false;
            }

            var shouldOpen = false;
            if (!__instance.modData.TryGetValue($"{XSPlus.ModPrefix}/X", out var xStr) || !int.TryParse(xStr, out var xPos) || xPos == 0)
            {
                xPos = (int)__instance.TileLocation.X;
            }

            if (!__instance.modData.TryGetValue($"{XSPlus.ModPrefix}/Y", out var yStr) || !int.TryParse(yStr, out var yPos) || yPos == 0)
            {
                yPos = (int)__instance.TileLocation.Y;
            }

            var tileWidth = 1;
            var tileHeight = 1;
            if (OpenNearbyFeature.Instance._biggerChest.TryGetValueForItem(__instance, out var data))
            {
                tileWidth = data.Item1 / 16;
                tileHeight = (data.Item3 > 0 ? data.Item3 : data.Item2 - 16) / 16;
            }

            for (var i = 0; i < tileHeight; i++)
            {
                for (var j = 0; j < tileWidth; j++)
                {
                    var pos = new Vector2(xPos + j, yPos + i);
                    shouldOpen = location.farmers.Any(farmer => Math.Abs(farmer.getTileX() - pos.X) <= distance && Math.Abs(farmer.getTileY() - pos.Y) <= distance);
                    if (shouldOpen)
                    {
                        break;
                    }
                }

                if (shouldOpen)
                {
                    break;
                }
            }

            if (shouldOpen == ____farmerNearby)
            {
                return false;
            }

            ____farmerNearby = shouldOpen;
            ____shippingBinFrameCounter = 5;
            if (!animate)
            {
                ____shippingBinFrameCounter = -1;
                ___currentLidFrame = ____farmerNearby ? __instance.getLastLidFrame() : __instance.startingLidFrame.Value;
            }
            else if (Game1.gameMode != 6)
            {
                switch (____farmerNearby)
                {
                    case true:
                        location.localSound("doorCreak");
                        break;
                    case false:
                        location.localSound("doorCreakReverse");
                        break;
                }
            }

            return false;
        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            foreach (var obj in e.NewLocation.Objects.Values)
            {
                if (obj is Chest chest && this.IsEnabledForItem(chest) && this.TryGetValueForItem(chest, out var openNearby) && openNearby > 0)
                {
                    chest.UpdateFarmerNearby(e.NewLocation, false);
                }
            }
        }
    }
}