namespace StardewMods.EasyAccessConfigurator;

using System.Collections.Generic;
using System.Linq;
using Common.Helpers;
using Common.Integrations.EasyAccess;
using Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using SObject = StardewValley.Object;

/// <inheritdoc />
public class EasyAccessConfigurator : Mod
{
    private SObject CurrentObject { get; set; }

    private EasyAccessIntegration EasyAccess { get; set; }

    private GenericModConfigMenuIntegration GMCM { get; set; }

    private IDictionary<string, string> ProducerData { get; set; }

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // Init
        Log.Monitor = this.Monitor;
        this.EasyAccess = new(this.Helper.ModRegistry);
        this.GMCM = new(this.Helper.ModRegistry);

        // Events
        this.Helper.Events.Display.MenuChanged += this.OnMenuChanged;
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree
            || !e.Button.IsUseToolButton()
            || Game1.player.CurrentItem is not null
            || this.Helper.Input.IsSuppressed(e.Button))
        {
            return;
        }

        var pos = e.Button.TryGetController(out _) ? Game1.player.GetToolLocation() / 64 : e.Cursor.Tile;
        var x = (int)pos.X;
        var y = (int)pos.Y;
        pos.X = x;
        pos.Y = y;

        // Object exists at pos and is within reach of player
        if (!Utility.withinRadiusOfPlayer(x * Game1.tileSize, y * Game1.tileSize, 1, Game1.player)
            || !Game1.currentLocation.Objects.TryGetValue(pos, out var obj))
        {
            return;
        }

        this.CurrentObject = obj;
        this.ProducerData = this.CurrentObject.modData.Pairs
                                .Where(modData => modData.Key.StartsWith($"{this.EasyAccess.UniqueId}"))
                                .ToDictionary(
                                    modData => modData.Key[(this.EasyAccess.UniqueId.Length + 1)..],
                                    modData => modData.Value);
        if (this.ProducerData.Any())
        {
            this.Helper.Input.Suppress(e.Button);
            this.GMCM.Register(this.ModManifest, this.Reset, this.Save);
            this.EasyAccess.API.AddProducerOptions(this.ModManifest, this.ProducerData);
            this.GMCM.API.OpenModMenu(this.ModManifest);
        }
    }

    private void OnMenuChanged(object sender, MenuChangedEventArgs e)
    {
        if (this.ProducerData is not null && e.OldMenu?.GetType().Name == "SpecificModConfigMenu")
        {
            this.GMCM.Unregister(this.ModManifest);
            this.ProducerData = null;
            this.CurrentObject = null;
        }
    }

    private void Reset()
    {
        foreach (var (key, _) in this.ProducerData)
        {
            this.ProducerData[key] = string.Empty;
        }
    }

    private void Save()
    {
        foreach (var (key, value) in this.ProducerData)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                this.CurrentObject.modData.Remove($"{this.EasyAccess.UniqueId}/{key}");
                continue;
            }

            this.CurrentObject.modData[$"{this.EasyAccess.UniqueId}/{key}"] = value;
        }
    }
}