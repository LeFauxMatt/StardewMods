﻿namespace StardewMods.BetterChests.Features;

using System;
using Common.Helpers;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Interfaces.Config;
using StardewMods.FuryCore.Interfaces;
using StardewValley;

/// <inheritdoc />
internal class UnloadChest : Feature
{
    private readonly Lazy<CarryChest> _carryChest;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UnloadChest" /> class.
    /// </summary>
    /// <param name="config">Data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public UnloadChest(IConfigModel config, IModHelper helper, IModServices services)
        : base(config, helper, services)
    {
        this._carryChest = services.Lazy<CarryChest>();
    }

    private CarryChest CarryChest
    {
        get => this._carryChest.Value;
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        this.Helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
    }

    [EventPriority(EventPriority.High + 1)]
    private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree
            || !e.Button.IsUseToolButton()
            || !this.ManagedObjects.TryGetManagedStorage(Game1.player.CurrentItem, out var source)
            || source.UnloadChest == FeatureOption.Disabled)
        {
            return;
        }

        var pos = e.Button.TryGetController(out _) ? Game1.player.GetToolLocation() / 64f : e.Cursor.Tile;
        var x = (int)pos.X * Game1.tileSize;
        var y = (int)pos.Y * Game1.tileSize;

        // Object exists at pos and is within reach of player
        if (!Utility.withinRadiusOfPlayer(x, y, 1, Game1.player)
            || !Game1.currentLocation.Objects.TryGetValue(pos, out var obj))
        {
            return;
        }

        // Object is Chest and supports Unload Chest
        if (!this.ManagedObjects.TryGetManagedStorage(obj, out var target))
        {
            return;
        }

        // Stash items into target chest
        for (var index = source.Items.Count - 1; index >= 0; index--)
        {
            var item = source.Items[index];
            if (item is null)
            {
                continue;
            }

            item = target.StashItem(item);

            if (item is null)
            {
                source.Items[index] = null;
            }
        }

        // Add remaining items to target chest
        for (var index = source.Items.Count - 1; index >= 0; index--)
        {
            var item = source.Items[index];
            if (item is null)
            {
                continue;
            }

            item = target.AddItem(item);
            if (item is null)
            {
                source.Items[index] = null;
            }
        }

        Log.Info($"Unloading items from Chest {source.QualifiedItemId} into Chest {target.QualifiedItemId}");
        source.ClearNulls();
        this.CarryChest.CheckForOverburdened();
        this.Helper.Input.Suppress(e.Button);
    }
}