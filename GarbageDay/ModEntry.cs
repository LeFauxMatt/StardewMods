namespace StardewMods.GarbageDay;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.Common.Helpers;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.Objects;
using xTile;
using xTile.Dimensions;

/// <inheritdoc />
public sealed class ModEntry : Mod
{
    private readonly Dictionary<string, Lazy<GarbageCan?>> garbageCans = new();
    private readonly PerScreen<GarbageCan?> perScreenGarbageCan = new();
    private readonly PerScreen<NPC?> perScreenNpc = new();

#nullable disable
    private ModConfig config;
    private Multiplayer multiplayer;
#nullable enable

    private IEnumerable<GarbageCan> GarbageCans =>
        this.garbageCans.Values.Select(garbageCan => garbageCan.Value).OfType<GarbageCan>();

    private GarbageCan? GarbageCan
    {
        get => this.perScreenGarbageCan.Value;
        set => this.perScreenGarbageCan.Value = value;
    }

    private NPC? Npc
    {
        get => this.perScreenNpc.Value;
        set => this.perScreenNpc.Value = value;
    }

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // Init
        I18n.Init(this.Helper.Translation);
        ModPatches.Init(this.ModManifest);
        this.config = this.Helper.ReadConfig<ModConfig>();

        // Console Commands
        this.Helper.ConsoleCommands.Add("garbage_fill", I18n.Command_GarbageFill_Description(), this.GarbageFill);
        this.Helper.ConsoleCommands.Add("garbage_hat", I18n.Command_GarbageHat_Description(), ModEntry.GarbageHat);
        this.Helper.ConsoleCommands.Add("garbage_clear", I18n.Command_GarbageClear_Description(), this.GarbageClear);

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

    private static void GarbageHat(string command, string[] args) => GarbageCan.GarbageHat = true;

    private void GarbageClear(string command, string[] args)
    {
        var objectsToRemove = new List<(GameLocation, Vector2)>();
        Utility.ForEachLocation(
            location =>
            {
                foreach (var (tile, obj) in location.Objects.Pairs)
                {
                    if (obj is not Chest chest || !chest.modData.TryGetValue("furyx639.GarbageDay/WhichCan", out _))
                    {
                        continue;
                    }

                    objectsToRemove.Add((location, tile));
                }

                return true;
            });

        foreach (var (location, tile) in objectsToRemove)
        {
            location.Objects.Remove(tile);
        }

        this.garbageCans.Clear();
    }

    private void GarbageFill(string command, string[] args)
    {
        if (args.Length < 1 || !int.TryParse(args[0], out var amount))
        {
            amount = 1;
        }

        foreach (var garbageCan in this.GarbageCans)
        {
            for (var i = 0; i < amount; ++i)
            {
                garbageCan.AddLoot();
            }
        }
    }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.NameWithoutLocale.IsEquivalentTo("furyx639.GarbageDay/Texture"))
        {
            e.LoadFromModFile<Texture2D>("assets/GarbageCan.png", AssetLoadPriority.Exclusive);
        }

        if (e.DataType != typeof(Map))
        {
            return;
        }

        e.Edit(
            asset =>
            {
                var map = asset.AsMap().Data;
                for (var x = 0; x < map.Layers[0].LayerWidth; ++x)
                {
                    for (var y = 0; y < map.Layers[0].LayerHeight; ++y)
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

                        var parts = ArgUtility.SplitBySpace(property);
                        if (parts[0] != "Garbage")
                        {
                            continue;
                        }

                        var whichCan = parts[1];
                        if (string.IsNullOrWhiteSpace(whichCan))
                        {
                            continue;
                        }

                        if (!this.garbageCans.ContainsKey(whichCan))
                        {
                            var pos = new Vector2(x, y);
                            this.garbageCans.Add(
                                whichCan,
                                new Lazy<GarbageCan?>(
                                    () =>
                                    {
                                        foreach (var location in Game1.locations)
                                        {
                                            if (!asset.Name.IsEquivalentTo(location.mapPath.Value))
                                            {
                                                continue;
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
                                            return new GarbageCan(location, chest);
                                        }

                                        return null;
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
                        map
                            .GetLayer("Back")
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
            || !this.garbageCans.TryGetValue(whichCan, out var garbageCan)
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

        this.Npc = npc;
        this.multiplayer?.globalChatInfoMessage("TrashCan", Game1.player.Name, npc.Name);
        if (npc.Name.Equals("Linus", StringComparison.OrdinalIgnoreCase))
        {
            npc.doEmote(32);
            npc.setNewDialogue(
                Game1.content.LoadString("Data\\ExtraDialogue:Town_DumpsterDiveComment_Linus"),
                true,
                true);

            Game1.player.changeFriendship(5, npc);
            this.multiplayer?.globalChatInfoMessage("LinusTrashCan");
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
            if (Game1.dayOfMonth % 7 == (int)this.config.GarbageDay % 7)
            {
                garbageCan.EmptyTrash();
            }

            garbageCan.AddLoot();
        }
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e) =>
        this.multiplayer = this.Helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();

    private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.OldMenu is not ItemGrabMenu || this.GarbageCan is null)
        {
            return;
        }

        // Close Can
        if (this.Npc is not null)
        {
            Game1.drawDialogue(this.Npc);
            this.Npc = null;
        }

        this.GarbageCan = null;
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        var objectsToRemove = new List<(GameLocation, Vector2)>();
        Utility.ForEachLocation(
            location =>
            {
                foreach (var (tile, obj) in location.Objects.Pairs)
                {
                    if (obj is not Chest chest
                        || !chest.modData.TryGetValue("furyx639.GarbageDay/WhichCan", out var whichCan)
                        || this.garbageCans.ContainsKey(whichCan))
                    {
                        continue;
                    }

                    objectsToRemove.Add((location, tile));
                }

                return true;
            });

        foreach (var (location, tile) in objectsToRemove)
        {
            location.Objects.Remove(tile);
        }
    }
}