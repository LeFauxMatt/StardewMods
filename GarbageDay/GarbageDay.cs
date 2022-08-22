namespace StardewMods.GarbageDay;

using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.Common.Helpers;
using StardewMods.CommonHarmony.Enums;
using StardewMods.CommonHarmony.Helpers;
using StardewMods.CommonHarmony.Models;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.Objects;
using xTile;
using xTile.Dimensions;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

/// <inheritdoc />
public class GarbageDay : Mod
{
    private readonly HashSet<IAssetName> _excludedAssets = new();
    private readonly PerScreen<GarbageCan?> _garbageCan = new();
    private readonly Dictionary<string, Lazy<GarbageCan?>> _garbageCans = new();
    private readonly PerScreen<NPC?> _npc = new();

    private ModConfig? _config;
    private Multiplayer? _multiplayer;

    private ModConfig Config
    {
        get
        {
            if (this._config is not null)
            {
                return this._config;
            }

            ModConfig? config = null;
            try
            {
                config = this.Helper.ReadConfig<ModConfig>();
            }
            catch (Exception)
            {
                // ignored
            }

            this._config = config ?? new ModConfig();
            Log.Trace(this._config.ToString());
            return this._config;
        }
    }

    private GarbageCan? GarbageCan
    {
        get => this._garbageCan.Value;
        set => this._garbageCan.Value = value;
    }

    private IEnumerable<GarbageCan> GarbageCans =>
        this._garbageCans.Values.Select(garbageCan => garbageCan.Value).OfType<GarbageCan>();

    private NPC? NPC
    {
        get => this._npc.Value;
        set => this._npc.Value = value;
    }

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        Log.Monitor = this.Monitor;
        CommonHelpers.Multiplayer = this.Helper.Multiplayer;
        I18n.Init(this.Helper.Translation);

        // Console Commands
        this.Helper.ConsoleCommands.Add("garbage_fill", I18n.Command_GarbageFill_Description(), this.GarbageFill);
        this.Helper.ConsoleCommands.Add("garbage_hat", I18n.Command_GarbageHat_Description(), GarbageDay.GarbageHat);
        this.Helper.ConsoleCommands.Add("garbage_kill", I18n.Command_GarbageKill_Description(), GarbageDay.GarbageKill);

        // Patches
        HarmonyHelper.AddPatches(
            this.ModManifest.UniqueID,
            new SavedPatch[]
            {
                new(
                    AccessTools.Method(
                        typeof(Chest),
                        nameof(Chest.draw),
                        new[]
                        {
                            typeof(SpriteBatch),
                            typeof(int),
                            typeof(int),
                            typeof(float),
                        }),
                    typeof(GarbageDay),
                    nameof(GarbageDay.Chest_draw_prefix),
                    PatchType.Prefix),
                new(
                    AccessTools.Method(typeof(Chest), nameof(Chest.performToolAction)),
                    typeof(GarbageDay),
                    nameof(GarbageDay.Chest_performToolAction_prefix),
                    PatchType.Prefix),
                new(
                    AccessTools.Method(typeof(Chest), nameof(Chest.UpdateFarmerNearby)),
                    typeof(GarbageDay),
                    nameof(GarbageDay.Chest_UpdateFarmerNearby_prefix),
                    PatchType.Prefix),
                new(
                    AccessTools.Method(typeof(Chest), nameof(Chest.updateWhenCurrentLocation)),
                    typeof(GarbageDay),
                    nameof(GarbageDay.Chest_updateWhenCurrentLocation_prefix),
                    PatchType.Prefix),
            });
        HarmonyHelper.ApplyPatches(this.ModManifest.UniqueID);

        // Events
        this.Helper.Events.Content.AssetRequested += this.OnAssetRequested;
        this.Helper.Events.Display.MenuChanged += this.OnMenuChanged;
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;

        if (!Context.IsMainPlayer)
        {
            return;
        }

        this.Helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        this.Helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Chest_draw_prefix(
        Chest __instance,
        ref int ___currentLidFrame,
        SpriteBatch spriteBatch,
        int x,
        int y,
        float alpha)
    {
        if (!__instance.modData.ContainsKey("furyx639.GarbageDay/WhichCan"))
        {
            return true;
        }

        var texture = Game1.content.Load<Texture2D>("furyx639.GarbageDay/Texture");
        var currentLidFrame = ___currentLidFrame % 3;
        var layerDepth = Math.Max(0.0f, ((y + 1f) * 64f - 24f) / 10000f) + x * 1E-05f;
        if (__instance.playerChoiceColor.Value.Equals(Color.Black))
        {
            spriteBatch.Draw(
                texture,
                Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y - 1) * Game1.tileSize),
                new Rectangle(currentLidFrame * 16, 0, 16, 32),
                Color.White * alpha,
                0f,
                Vector2.Zero,
                Game1.pixelZoom,
                SpriteEffects.None,
                layerDepth + (1 + layerDepth) * 1E-05f);
            return false;
        }

        spriteBatch.Draw(
            texture,
            Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y - 1) * Game1.tileSize),
            new Rectangle(currentLidFrame * 16, 64, 16, 32),
            Color.White * alpha,
            0f,
            Vector2.Zero,
            Game1.pixelZoom,
            SpriteEffects.None,
            layerDepth * 1E-05f);

        spriteBatch.Draw(
            texture,
            Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y - 1) * Game1.tileSize),
            new Rectangle(currentLidFrame * 16, 32, 16, 32),
            __instance.playerChoiceColor.Value * alpha,
            0f,
            Vector2.Zero,
            Game1.pixelZoom,
            SpriteEffects.None,
            layerDepth * 1E-05f);
        return false;
    }

    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Chest_performToolAction_prefix(Chest __instance)
    {
        return !__instance.modData.ContainsKey("furyx639.GarbageDay/WhichCan");
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Chest_UpdateFarmerNearby_prefix(
        Chest __instance,
        ref bool ____farmerNearby,
        ref int ____shippingBinFrameCounter,
        ref int ___currentLidFrame,
        GameLocation location,
        bool animate)
    {
        if (!__instance.modData.ContainsKey("furyx639.GarbageDay/WhichCan"))
        {
            return true;
        }

        var shouldOpen = location.farmers.Any(
            farmer => Math.Abs(farmer.getTileX() - __instance.TileLocation.X) <= 1f
                   && Math.Abs(farmer.getTileY() - __instance.TileLocation.Y) <= 1f);
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
            location.localSound("trashcanlid");
        }

        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Chest_updateWhenCurrentLocation_prefix(
        Chest __instance,
        ref int ____shippingBinFrameCounter,
        ref bool ____farmerNearby,
        ref int ___currentLidFrame,
        GameLocation environment)
    {
        if (!__instance.modData.ContainsKey("furyx639.GarbageDay/WhichCan"))
        {
            return true;
        }

        if (__instance.synchronized.Value)
        {
            __instance.openChestEvent.Poll();
        }

        __instance.mutex.Update(environment);

        __instance.UpdateFarmerNearby(environment);
        if (____shippingBinFrameCounter > -1)
        {
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
        }

        if (Game1.activeClickableMenu is null && __instance.GetMutex().IsLockHeld())
        {
            __instance.GetMutex().ReleaseLock();
        }

        return false;
    }

    private static void GarbageHat(string command, string[] args)
    {
        GarbageCan.GarbageHat = true;
    }

    private static void GarbageKill(string command, string[] args)
    {
        var objectsToRemove = new List<(GameLocation, Vector2)>();
        foreach (var location in CommonHelpers.AllLocations)
        {
            foreach (var (tile, obj) in location.Objects.Pairs)
            {
                if (obj is not Chest chest || !chest.modData.TryGetValue("furyx639.GarbageDay/WhichCan", out _))
                {
                    continue;
                }

                objectsToRemove.Add((location, tile));
            }
        }

        foreach (var (location, tile) in objectsToRemove)
        {
            location.Objects.Remove(tile);
        }
    }

    private void GarbageFill(string command, string[] args)
    {
        if (args.Length < 1 || !int.TryParse(args[0], out var amount))
        {
            amount = 1;
        }

        foreach (var garbageCan in this.GarbageCans)
        {
            for (var i = 0; i < amount; i++)
            {
                garbageCan.AddLoot();
            }
        }
    }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo("furyx639.GarbageDay/Loot"))
        {
            e.LoadFromModFile<Dictionary<string, Dictionary<string, float>>>(
                "assets/loot.json",
                AssetLoadPriority.Exclusive);
            return;
        }

        if (e.Name.IsEquivalentTo("furyx639.GarbageDay/Texture"))
        {
            e.LoadFromModFile<Texture2D>("assets/GarbageCan.png", AssetLoadPriority.Exclusive);
        }

        if (e.DataType != typeof(Map) || this._excludedAssets.Contains(e.Name))
        {
            return;
        }

        e.Edit(
            asset =>
            {
                var map = asset.AsMap().Data;
                if (!map.Properties.TryGetValue("GarbageDay", out var lootKey))
                {
                    if (!asset.Name.IsEquivalentTo(@"Maps\Town"))
                    {
                        this._excludedAssets.Add(asset.Name);
                        return;
                    }

                    lootKey = "Town";
                }

                for (var x = 0; x < map.Layers[0].LayerWidth; x++)
                {
                    for (var y = 0; y < map.Layers[0].LayerHeight; y++)
                    {
                        var layer = map.GetLayer("Buildings");
                        var tile = layer.PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size);
                        if (tile is null)
                        {
                            continue;
                        }

                        // Look for Action: Garbage [WhichCan]
                        tile.Properties.TryGetValue("Action", out var property);
                        if (property is null)
                        {
                            continue;
                        }

                        var parts = property.ToString().Split(' ');
                        if (parts.Length != 2 || parts[0] != "Garbage")
                        {
                            continue;
                        }

                        var whichCan = parts[1];
                        if (string.IsNullOrWhiteSpace(whichCan))
                        {
                            continue;
                        }

                        if (!this._garbageCans.ContainsKey(whichCan))
                        {
                            var pos = new Vector2(x, y);
                            this._garbageCans.Add(
                                whichCan,
                                new(
                                    () =>
                                    {
                                        var location = CommonHelpers.AllLocations.FirstOrDefault(
                                            location => asset.Name.IsEquivalentTo(location.mapPath.Value));
                                        if (location is null)
                                        {
                                            return null;
                                        }

                                        if (!location.Objects.TryGetValue(pos, out var obj))
                                        {
                                            obj = new Chest(true, pos)
                                            {
                                                Name = "Garbage Can",
                                                playerChoiceColor = { Value = Color.DarkGray },
                                                modData =
                                                {
                                                    ["furyx639.GarbageDay/WhichCan"] = whichCan,
                                                    ["furyx639.GarbageDay/LootKey"] = lootKey,
                                                    ["Pathoschild.ChestsAnywhere/IsIgnored"] = "true",
                                                },
                                            };

                                            location.Objects.Add(pos, obj);
                                        }

                                        if (obj is not Chest chest)
                                        {
                                            return null;
                                        }

                                        chest.startingLidFrame.Value = 0;
                                        chest.lidFrameCount.Value = 3;
                                        return new(location, chest);
                                    }));
                        }

                        // Remove base tile
                        if (layer.Tiles[x, y] is not null
                         && layer.Tiles[x, y].TileSheet.Id == "Town"
                         && layer.Tiles[x, y].TileIndex == 78)
                        {
                            layer.Tiles[x, y] = null;
                        }

                        // Remove Lid tile
                        layer = map.GetLayer("Front");
                        if (layer.Tiles[x, y - 1] is not null
                         && layer.Tiles[x, y - 1].TileSheet.Id == "Town"
                         && layer.Tiles[x, y - 1].TileIndex == 46)
                        {
                            layer.Tiles[x, y - 1] = null;
                        }

                        // Add NoPath to tile
                        map.GetLayer("Back")
                           .PickTile(new Location(x, y) * Game1.tileSize, Game1.viewport.Size)
                           ?.Properties.Add("NoPath", string.Empty);
                    }
                }
            },
            AssetEditPriority.Late);
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree || !e.Button.IsActionButton() || this.Helper.Input.IsSuppressed(e.Button))
        {
            return;
        }

        var pos = CommonHelpers.GetCursorTile(1);
        if (!Game1.currentLocation.Objects.TryGetValue(pos, out var obj)
         || obj is not Chest chest
         || !chest.modData.TryGetValue("furyx639.GarbageDay/WhichCan", out var whichCan)
         || !this._garbageCans.TryGetValue(whichCan, out var garbageCan)
         || garbageCan.Value is null)
        {
            return;
        }

        this.GarbageCan = garbageCan.Value;
        var character = Utility.isThereAFarmerOrCharacterWithinDistance(
            this.GarbageCan.Tile,
            7,
            this.GarbageCan.Location);
        if (character is not NPC npc || character is Horse)
        {
            this.GarbageCan.CheckAction();
            this.Helper.Input.Suppress(e.Button);
            return;
        }

        this.NPC = npc;
        this._multiplayer?.globalChatInfoMessage("TrashCan", Game1.player.Name, npc.Name);
        if (npc.Name.Equals("Linus"))
        {
            npc.doEmote(32);
            npc.setNewDialogue(
                Game1.content.LoadString("Data\\ExtraDialogue:Town_DumpsterDiveComment_Linus"),
                true,
                true);
            Game1.player.changeFriendship(5, npc);
            this._multiplayer?.globalChatInfoMessage("LinusTrashCan");
        }
        else
        {
            switch (npc.Age)
            {
                case 2:
                    npc.doEmote(28);
                    npc.setNewDialogue(
                        Game1.content.LoadString("Data\\ExtraDialogue:Town_DumpsterDiveComment_Child"),
                        true,
                        true);
                    break;
                case 1:
                    npc.doEmote(8);
                    npc.setNewDialogue(
                        Game1.content.LoadString("Data\\ExtraDialogue:Town_DumpsterDiveComment_Teen"),
                        true,
                        true);
                    break;
                default:
                    npc.doEmote(12);
                    npc.setNewDialogue(
                        Game1.content.LoadString("Data\\ExtraDialogue:Town_DumpsterDiveComment_Adult"),
                        true,
                        true);
                    break;
            }

            Game1.player.changeFriendship(-25, npc);
        }

        this.GarbageCan.CheckAction();
        this.Helper.Input.Suppress(e.Button);
    }

    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        foreach (var garbageCan in this.GarbageCans)
        {
            // Empty chest on garbage day
            if (Game1.dayOfMonth % 7 == (int)this.Config.GarbageDay % 7)
            {
                garbageCan.EmptyTrash();
            }

            garbageCan.AddLoot();
        }
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        this._multiplayer = this.Helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();
    }

    private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.OldMenu is not ItemGrabMenu || this.GarbageCan is null)
        {
            return;
        }

        // Close Can
        if (this.NPC is not null)
        {
            Game1.drawDialogue(this.NPC);
            this.NPC = null;
        }

        this.GarbageCan = null;
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        var objectsToRemove = new List<(GameLocation, Vector2)>();
        foreach (var location in CommonHelpers.AllLocations)
        {
            foreach (var (tile, obj) in location.Objects.Pairs)
            {
                if (obj is not Chest chest
                 || !chest.modData.TryGetValue("furyx639.GarbageDay/WhichCan", out var whichCan)
                 || this._garbageCans.ContainsKey(whichCan))
                {
                    continue;
                }

                objectsToRemove.Add((location, tile));
            }
        }

        foreach (var (location, tile) in objectsToRemove)
        {
            location.Objects.Remove(tile);
        }
    }
}