namespace XSPlus.Features;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using SObject = StardewValley.Object;

internal class BiggerChestFeature : FeatureWithParam<Tuple<int, int, int>>
{
    private static BiggerChestFeature Instance;
    private HarmonyHelper _harmony;
    private OpenNearbyFeature _openNearby;

    private BiggerChestFeature(ServiceLocator serviceLocator)
        : base("BiggerChest", serviceLocator)
    {
        BiggerChestFeature.Instance ??= this;

        // Dependencies
        this.AddDependency<OpenNearbyFeature>(service => this._openNearby = service as OpenNearbyFeature);
        this.AddDependency<HarmonyHelper>(
            service =>
            {
                // Init
                this._harmony = service as HarmonyHelper;

                // Patches
                this._harmony?.AddPatch(
                    this.ServiceName,
                    AccessTools.Method(typeof(SObject), nameof(SObject.drawWhenHeld)),
                    typeof(BiggerChestFeature),
                    nameof(BiggerChestFeature.Object_drawWhenHeld_prefix));

                this._harmony?.AddPatch(
                    this.ServiceName,
                    AccessTools.Method(
                        typeof(SObject),
                        nameof(SObject.draw),
                        new[]
                        {
                            typeof(SpriteBatch), typeof(int), typeof(int), typeof(float),
                        }),
                    typeof(BiggerChestFeature),
                    nameof(BiggerChestFeature.Object_draw_prefix));

                this._harmony?.AddPatch(
                    this.ServiceName,
                    AccessTools.Method(typeof(SObject), nameof(SObject.drawPlacementBounds)),
                    typeof(BiggerChestFeature),
                    nameof(BiggerChestFeature.Object_drawPlacementBounds_prefix));

                this._harmony?.AddPatch(
                    this.ServiceName,
                    AccessTools.Method(typeof(SObject), nameof(SObject.performToolAction)),
                    typeof(BiggerChestFeature),
                    nameof(BiggerChestFeature.Object_performToolAction_prefix));

                this._harmony?.AddPatch(
                    this.ServiceName,
                    AccessTools.Method(typeof(Utility), nameof(Utility.playerCanPlaceItemHere)),
                    typeof(BiggerChestFeature),
                    nameof(BiggerChestFeature.Utility_playerCanPlaceItemHere_postfix),
                    PatchType.Postfix);
            });

        // Events
        this.Helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
    }

    /// <inheritdoc />
    public override void Activate()
    {
        // Events
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        this.Helper.Events.World.ObjectListChanged += this.OnObjectListChanged;

        // Patches
        this._harmony.ApplyPatches(this.ServiceName);
    }

    /// <inheritdoc />
    public override void Deactivate()
    {
        // Events
        this.Helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
        this.Helper.Events.World.ObjectListChanged -= this.OnObjectListChanged;

        // Patches
        this._harmony.UnapplyPatches(this.ServiceName);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    private static bool Object_draw_prefix(SObject __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
    {
        if (!__instance.modData.TryGetValue($"{XSPlus.ModPrefix}/X", out var xStr)
            || !__instance.modData.TryGetValue($"{XSPlus.ModPrefix}/Y", out var yStr)
            || !int.TryParse(xStr, out var xPos)
            || !int.TryParse(yStr, out var yPos)
            || xPos == x && yPos == y
            || !Game1.player.currentLocation.Objects.TryGetValue(new(xPos, yPos), out var obj)
            || obj is not Chest {playerChest.Value: true})
        {
            return true;
        }

        obj.draw(spriteBatch, xPos, yPos, alpha);
        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    private static bool Object_drawPlacementBounds_prefix(SObject __instance, SpriteBatch spriteBatch, GameLocation location)
    {
        if (!BiggerChestFeature.Instance.IsEnabledForItem(__instance) || !BiggerChestFeature.Instance.TryGetValueForItem(__instance, out var data))
        {
            return true;
        }

        var tileWidth = data.Item1 / 16;
        var tileHeight = (data.Item3 > 0 ? data.Item3 : data.Item2 - 16) / 16;

        var tile = 64 * Game1.GetPlacementGrabTile();
        var x = (int)tile.X;
        var y = (int)tile.Y;

        Game1.isCheckingNonMousePlacement = !Game1.IsPerformingMousePlacement();
        if (Game1.isCheckingNonMousePlacement)
        {
            var pos = Utility.GetNearbyValidPlacementPosition(Game1.player, location, __instance, x, y);
            x = (int)pos.X;
            y = (int)pos.Y;
        }

        var canPlaceHere = Utility.playerCanPlaceItemHere(location, __instance, x, y, Game1.player)
                           || Utility.isThereAnObjectHereWhichAcceptsThisItem(location, __instance, x, y)
                           && Utility.withinRadiusOfPlayer(x, y, 1, Game1.player);

        Game1.isCheckingNonMousePlacement = false;
        var viewport = new Vector2(Game1.viewport.X, Game1.viewport.Y);
        for (var i = 0; i < tileWidth; i++)
        {
            for (var j = 0; j < tileHeight; j++)
            {
                var tileLocation = new Vector2(x / 64 + i, y / 64 + j);
                spriteBatch.Draw(
                    Game1.mouseCursors,
                    tileLocation * 64 - viewport,
                    new Rectangle(canPlaceHere ? 194 : 210, 388, 16, 16),
                    Color.White,
                    0f,
                    Vector2.Zero,
                    4f,
                    SpriteEffects.None,
                    0.01f);
            }
        }

        __instance.draw(spriteBatch, x / 64, y / 64, 0.5f);
        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    private static bool Object_drawWhenHeld_prefix(SObject __instance, SpriteBatch spriteBatch, Vector2 objectPosition)
    {
        if (__instance is not Chest chest || !BiggerChestFeature.Instance.IsEnabledForItem(__instance) || !BiggerChestFeature.Instance.TryGetValueForItem(__instance, out var data))
        {
            return true;
        }

        objectPosition.X += 32;
        objectPosition.X -= data.Item1 * (Game1.pixelZoom / 2);
        objectPosition.Y += 32;
        objectPosition.Y -= data.Item2 * (Game1.pixelZoom / 2);

        chest.draw(spriteBatch, (int)objectPosition.X, (int)objectPosition.Y);
        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    private static bool Object_performToolAction_prefix(SObject __instance, Tool t, GameLocation location)
    {
        if (!BiggerChestFeature.Instance.IsEnabledForItem(__instance))
        {
            return true;
        }

        if (!__instance.modData.TryGetValue($"{XSPlus.ModPrefix}/X", out var xStr)
            || !__instance.modData.TryGetValue($"{XSPlus.ModPrefix}/Y", out var yStr)
            || !int.TryParse(xStr, out var xPos)
            || !int.TryParse(yStr, out var yPos)
            || !location.Objects.TryGetValue(new(xPos, yPos), out var obj)
            || obj == __instance
            || obj is not Chest chest)
        {
            return true;
        }

        return chest.performToolAction(t, location);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    private static void Utility_playerCanPlaceItemHere_postfix(ref bool __result, GameLocation location, Item item, int x, int y, Farmer f)
    {
        if (!BiggerChestFeature.Instance.IsEnabledForItem(item) || !BiggerChestFeature.Instance.TryGetValueForItem(item, out var data))
        {
            return;
        }

        var tileWidth = data.Item1 / 16;
        var tileHeight = (data.Item3 > 0 ? data.Item3 : data.Item2 - 16) / 16;

        if (Utility.isPlacementForbiddenHere(location) || Game1.eventUp || f.bathingClothes.Value || f.onBridge.Value)
        {
            __result = false;
            return;
        }

        // Is Within Tile With Leeway
        x = 64 * (x / 64);
        y = 64 * (y / 64);
        if (!Utility.withinRadiusOfPlayer(x, y, Math.Max(data.Item1, data.Item2), f))
        {
            __result = false;
            return;
        }

        // Position intersects with farmer
        var rect = new Rectangle(x, y, tileWidth * 64, tileHeight * 64);
        if (location.farmers.Any(farmer => farmer.GetBoundingBox().Intersects(rect)))
        {
            __result = false;
            return;
        }

        // Is Close Enough to Farmer
        rect.Inflate(32, 32);
        if (!rect.Intersects(f.GetBoundingBox()))
        {
            __result = false;
            return;
        }

        for (var i = 0; i < tileWidth; i++)
        {
            for (var j = 0; j < tileHeight; j++)
            {
                var tileLocation = new Vector2(x / 64 + i, y / 64 + j);

                if (item.canBePlacedHere(location, tileLocation)
                    && location.getObjectAtTile((int)tileLocation.X, (int)tileLocation.Y) is null
                    && location.isTilePlaceable(tileLocation, item))
                {
                    continue;
                }

                // Item cannot be placed here
                __result = false;
                return;
            }
        }

        __result = true;
    }

    private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
    {
        if (Context.IsMainPlayer)
        {
            var locations = Game1.locations.Concat(Game1.locations.OfType<BuildableGameLocation>().SelectMany(location => location.buildings.Where(building => building.indoors.Value is not null).Select(building => building.indoors.Value)));
            foreach (var location in locations)
            {
                foreach (var obj in location.Objects.Pairs)
                {
                    if (obj.Value is not Chest {playerChest.Value: true} || !this.IsEnabledForItem(obj.Value) || !this.TryGetValueForItem(obj.Value, out var data))
                    {
                        continue;
                    }

                    var tileWidth = data.Item1 / 16;
                    var tileHeight = (data.Item3 > 0 ? data.Item3 : data.Item2 - 16) / 16;
                    for (var x = 0; x < tileWidth; x++)
                    {
                        for (var y = 0; y < tileHeight; y++)
                        {
                            var pos = obj.Key + new Vector2(x, y);
                            if ((x != 0 || y != 0) && location.Objects[pos] is not Chest)
                            {
                                location.Objects[pos] = new(Vector2.Zero, 232);
                            }

                            location.Objects[pos].modData[$"{XSPlus.ModPrefix}/X"] = obj.Key.X.ToString(CultureInfo.InvariantCulture);
                            location.Objects[pos].modData[$"{XSPlus.ModPrefix}/Y"] = obj.Key.Y.ToString(CultureInfo.InvariantCulture);
                        }
                    }
                }
            }
        }
    }

    [EventPriority(EventPriority.High)]
    private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree || !e.Button.IsActionButton())
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
        if (BiggerChestFeature.TryGetOriginObject(Game1.currentLocation, obj, out var sourceObj, out _))
        {
            obj = sourceObj;
        }

        if (!this.IsEnabledForItem(obj) || obj is not Chest {playerChest.Value: true} chest)
        {
            return;
        }

        if (this._openNearby.IsEnabledForItem(obj))
        {
            Game1.playSound("openChest");
            chest.ShowMenu();
        }
        else
        {
            chest.GetMutex().RequestLock(
                () =>
                {
                    chest.frameCounter.Value = 5;
                    Game1.playSound("openChest");
                    Game1.player.Halt();
                    Game1.player.freezePause = 1000;
                });
        }

        this.Helper.Input.Suppress(e.Button);
    }

    [EventPriority(EventPriority.Low)]
    private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
    {
        if (!e.IsCurrentLocation)
        {
            return;
        }

        var added = e.Added.SingleOrDefault(added => added.Value is Chest {playerChest.Value: true} && this.IsEnabledForItem(added.Value));
        if (added.Value is not null && this.TryGetValueForItem(added.Value, out var data))
        {
            var tileWidth = data.Item1 / 16;
            var tileHeight = (data.Item3 > 0 ? data.Item3 : data.Item2 - 16) / 16;
            for (var x = 0; x < tileWidth; x++)
            {
                for (var y = 0; y < tileHeight; y++)
                {
                    var pos = added.Key + new Vector2(x, y);
                    if (x != 0 || y != 0)
                    {
                        e.Location.Objects[pos] = new(Vector2.Zero, 232);
                    }

                    e.Location.Objects[pos].modData[$"{XSPlus.ModPrefix}/X"] = added.Key.X.ToString(CultureInfo.InvariantCulture);
                    e.Location.Objects[pos].modData[$"{XSPlus.ModPrefix}/Y"] = added.Key.Y.ToString(CultureInfo.InvariantCulture);
                }
            }
        }

        foreach (var removed in e.Removed)
        {
            if (!removed.Value.modData.TryGetValue($"{XSPlus.ModPrefix}/X", out var xStr)
                || !removed.Value.modData.TryGetValue($"{XSPlus.ModPrefix}/Y", out var yStr)
                || !int.TryParse(xStr, out var xPos)
                || !int.TryParse(yStr, out var yPos))
            {
                continue;
            }

            var originPos = new Vector2(xPos, yPos);
            if (removed.Key.Equals(originPos) && this.TryGetValueForItem(removed.Value, out data) || e.Location.Objects.TryGetValue(originPos, out var originObj) && this.TryGetValueForItem(originObj, out data))
            {
                var tileWidth = data.Item1 / 16;
                var tileHeight = (data.Item3 > 0 ? data.Item3 : data.Item2 - 16) / 16;
                for (var x = 0; x < tileWidth; x++)
                {
                    for (var y = 0; y < tileHeight; y++)
                    {
                        e.Location.Objects.Remove(originPos + new Vector2(x, y));
                    }
                }

                return;
            }
        }
    }

    public static bool TryGetOriginObject(GameLocation location, SObject obj, out SObject originObject, out Vector2 pos)
    {
        originObject = default;
        pos = default;

        if (obj.modData.TryGetValue($"{XSPlus.ModPrefix}/X", out var xStr)
            && obj.modData.TryGetValue($"{XSPlus.ModPrefix}/Y", out var yStr)
            && int.TryParse(xStr, out var xPos)
            && int.TryParse(yStr, out var yPos)
            && location.Objects.TryGetValue(new(xPos, yPos), out originObject))
        {
            pos = new(xPos, yPos);
            return true;
        }

        return false;
    }
}