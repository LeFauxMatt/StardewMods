namespace StardewMods.GarbageDay.Framework.Services;

using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.Common.Extensions;
using StardewMods.Common.Helpers;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.GarbageDay.Framework.Interfaces;
using StardewMods.GarbageDay.Framework.Models;
using StardewValley.Characters;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Represents a manager for managing garbage cans in a game.</summary>
internal sealed class GarbageCanManager : BaseService<GarbageCanManager>
{
    private const string AssetPath = "Data/GarbageCans";

    private readonly PerScreen<GarbageCan?> currentGarbageCan = new();
    private readonly PerScreen<NPC?> currentNpc = new();
    private readonly Dictionary<string, GarbageCan> garbageCans = [];
    private readonly Dictionary<string, FoundGarbageCan> foundGarbageCans = [];
    private readonly Dictionary<string, GameLocation?> foundLocations = [];
    private readonly HashSet<string> invalidGarbageCans = [];
    private readonly IInputHelper inputHelper;
    private readonly IModConfig modConfig;
    private readonly IReflectedField<Multiplayer> multiplayer;

    /// <summary>Initializes a new instance of the <see cref="GarbageCanManager" /> class.</summary>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    /// <param name="reflectionHelper">Dependency used for accessing inaccessible code.</param>
    public GarbageCanManager(
        IInputHelper inputHelper,
        ILog log,
        IManifest manifest,
        IModConfig modConfig,
        IModEvents modEvents,
        IReflectionHelper reflectionHelper)
        : base(log, manifest)
    {
        // Init
        this.ItemId = this.ModId + "/GarbageCan";
        this.QualifiedItemId = "(BC)" + this.ItemId;
        this.inputHelper = inputHelper;
        this.modConfig = modConfig;
        this.multiplayer = reflectionHelper.GetField<Multiplayer>(typeof(Game1), "multiplayer");

        // Events
        modEvents.Content.AssetsInvalidated += this.OnAssetsInvalidated;
        modEvents.Display.MenuChanged += this.OnMenuChanged;
        modEvents.Input.ButtonPressed += this.OnButtonPressed;
        modEvents.GameLoop.DayEnding += this.OnDayEnding;
        modEvents.GameLoop.DayStarted += this.OnDayStarted;
    }

    /// <summary>Gets the item id for the Garbage Can object.</summary>
    public string ItemId { get; }

    /// <summary>Gets the qualified item id for the Garbage Can object.</summary>
    public string QualifiedItemId { get; }

    /// <summary>Add a new pending garbage can.</summary>
    /// <param name="whichCan">The name of the garbage can.</param>
    /// <param name="assetName">The asset name of the map containing the garbage can.</param>
    /// <param name="x">The x-coordinate of the garbage can.</param>
    /// <param name="y">The y-coordinate of the garbage can.</param>
    /// <returns>true if the garbage can was successfully added; otherwise, false.</returns>
    public bool TryAddFound(string whichCan, IAssetName assetName, int x, int y)
    {
        if (this.foundGarbageCans.ContainsKey(whichCan))
        {
            return true;
        }

        if (this.invalidGarbageCans.Contains(whichCan))
        {
            return false;
        }

        if (!this.modConfig.OnByDefault
            && (!DataLoader.GarbageCans(Game1.content).GarbageCans.TryGetValue(whichCan, out var garbageCanData)
                || !garbageCanData.CustomFields.GetBool(this.ModId + "/Enabled")))
        {
            return false;
        }

        this.foundGarbageCans.Add(whichCan, new FoundGarbageCan(whichCan, assetName, x, y));
        return true;
    }

    private void OnAssetsInvalidated(object? sender, AssetsInvalidatedEventArgs e)
    {
        if (e.Names.Any(assetName => assetName.IsEquivalentTo(GarbageCanManager.AssetPath)))
        {
            this.foundGarbageCans.Clear();
        }
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree || !e.Button.IsActionButton())
        {
            return;
        }

        var pos = CommonHelpers.GetCursorTile(1);
        if (!Game1.currentLocation.Objects.TryGetValue(pos, out var obj)
            || obj is not Chest chest
            || !chest.modData.TryGetValue(this.ModId + "/WhichCan", out var whichCan)
            || !this.garbageCans.TryGetValue(whichCan, out var garbageCan))
        {
            return;
        }

        this.inputHelper.Suppress(e.Button);
        this.currentGarbageCan.Value = garbageCan;
        var character = Utility.isThereAFarmerOrCharacterWithinDistance(garbageCan.Tile, 7, garbageCan.Location);

        if (character is not NPC npc || character is Horse)
        {
            garbageCan.CheckAction();
            return;
        }

        // Queue up NPC response
        this.currentNpc.Value = npc;
        this.multiplayer.GetValue().globalChatInfoMessage("TrashCan", Game1.player.Name, npc.Name);
        if (npc.Name.Equals("Linus", StringComparison.OrdinalIgnoreCase))
        {
            npc.doEmote(32);
            npc.setNewDialogue(
                Game1.content.LoadString("Data\\ExtraDialogue:Town_DumpsterDiveComment_Linus"),
                true,
                true);

            Game1.player.changeFriendship(5, npc);
            this.multiplayer.GetValue().globalChatInfoMessage("LinusTrashCan");
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

        garbageCan.CheckAction();
    }

    private void OnDayEnding(object? sender, DayEndingEventArgs e)
    {
        // Remove garbage cans
        foreach (var garbageCan in this.garbageCans.Values)
        {
            garbageCan.Location.Objects.Remove(garbageCan.Tile);
        }

        this.garbageCans.Clear();
    }

    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        // Add garbage cans
        foreach (var (whichCan, foundGarbageCan) in this.foundGarbageCans)
        {
            if (this.garbageCans.ContainsKey(whichCan))
            {
                continue;
            }

            if (!this.TryCreateGarbageCan(foundGarbageCan, out var garbageCan))
            {
                this.invalidGarbageCans.Add(whichCan);
                continue;
            }

            this.garbageCans.Add(whichCan, garbageCan);
        }

        // Add loot
        foreach (var garbageCan in this.garbageCans.Values)
        {
            if (Game1.dayOfMonth % 7 == (int)this.modConfig.GarbageDay % 7)
            {
                garbageCan.EmptyTrash();
            }

            garbageCan.AddLoot();
        }
    }

    private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
    {
        if (e.OldMenu is not ItemGrabMenu || this.currentGarbageCan.Value is null)
        {
            return;
        }

        // Close Can
        if (this.currentNpc.Value is not null)
        {
            Game1.drawDialogue(this.currentNpc.Value);
            this.currentNpc.Value = null;
        }

        this.currentGarbageCan.Value = null;
    }

    private bool TryCreateGarbageCan(FoundGarbageCan foundGarbageCan, [NotNullWhen(true)] out GarbageCan? garbageCan)
    {
        // Find location
        if (!this.foundLocations.TryGetValue(foundGarbageCan.AssetName.Name, out var location))
        {
            location = Game1.locations.FirstOrDefault(l => foundGarbageCan.AssetName.IsEquivalentTo(l.mapPath.Value));
            this.foundLocations[foundGarbageCan.AssetName.Name] = location;
        }

        if (location is null)
        {
            garbageCan = null;
            return false;
        }

        // Remove existing garbage can
        if (location.Objects.TryGetValue(foundGarbageCan.TilePosition, out var obj)
            && obj.modData.ContainsKey(this.ModId + "/WhichCan"))
        {
            location.Objects.Remove(foundGarbageCan.TilePosition);
        }

        // Attempt to place item
        var item = (SObject)ItemRegistry.Create(this.QualifiedItemId);
        if (!item.placementAction(
                location,
                (int)foundGarbageCan.TilePosition.X * Game1.tileSize,
                (int)foundGarbageCan.TilePosition.Y * Game1.tileSize,
                Game1.player)
            || !location.Objects.TryGetValue(foundGarbageCan.TilePosition, out obj)
            || obj is not Chest chest)
        {
            garbageCan = null;
            return false;
        }

        // Update chest
        chest.GlobalInventoryId = this.Prefix + foundGarbageCan.WhichCan;
        chest.playerChoiceColor.Value = Color.DarkGray;
        chest.modData[this.ModId + "/WhichCan"] = foundGarbageCan.WhichCan;
        chest.modData["Pathoschild.ChestsAnywhere/IsIgnored"] = "true";

        // Create garbage can
        garbageCan = new GarbageCan(chest);
        return true;
    }
}