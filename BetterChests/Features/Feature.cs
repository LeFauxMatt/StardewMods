namespace StardewMods.BetterChests.Features;

using System;
using StardewModdingAPI;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Interfaces.Config;
using StardewMods.BetterChests.Services;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.CustomEvents;

/// <inheritdoc />
internal abstract class Feature : IModService
{
    private readonly Lazy<ICustomEvents> _customEvents;
    private readonly Lazy<ManagedObjects> _managedObjects;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Feature" /> class.
    /// </summary>
    /// <param name="config">The <see cref="IConfigData" /> for options set by the player.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    protected Feature(IConfigModel config, IModHelper helper, IModServices services)
    {
        this.Id = $"{BetterChests.ModUniqueId}.{this.GetType().Name}";
        this.Config = config;
        this.Helper = helper;
        this._customEvents = services.Lazy<ICustomEvents>();
        this._managedObjects = services.Lazy<ManagedObjects>();
    }

    /// <summary>
    ///     Gets the player configured mod options.
    /// </summary>
    protected IConfigModel Config { get; }

    /// <summary>
    ///     Gets custom events provided by FuryCore.
    /// </summary>
    protected ICustomEvents CustomEvents
    {
        get => this._customEvents.Value;
    }

    /// <summary>
    ///     Gets SMAPIs Helper API for events, input, and content.
    /// </summary>
    protected IModHelper Helper { get; }

    /// <summary>
    ///     Gets an Id that uniquely describes the mod and feature.
    /// </summary>
    protected string Id { get; }

    /// <summary>
    ///     Gets the <see cref="ManagedObjects" /> service to track placed and player chests in the game.
    /// </summary>
    protected ManagedObjects ManagedObjects
    {
        get => this._managedObjects.Value;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the feature is currently enabled.
    /// </summary>
    private bool Enabled { get; set; }

    /// <summary>
    ///     Toggles a feature on or off based on <see cref="IConfigData" />.
    /// </summary>
    public void Toggle()
    {
        var enabled = this switch
        {
            AutoOrganize => this.Config.DefaultChest.AutoOrganize != FeatureOption.Disabled,
            CarryChest => this.Config.DefaultChest.CarryChest != FeatureOption.Disabled,
            CategorizeChest => this.Config.CategorizeChest,
            ChestMenuTabs => this.Config.DefaultChest.ChestMenuTabs != FeatureOption.Disabled,
            CollectItems => this.Config.DefaultChest.CollectItems != FeatureOption.Disabled,
            CraftFromChest => this.Config.DefaultChest.CraftFromChest != FeatureOptionRange.Disabled,
            CustomColorPicker => this.Config.DefaultChest.CustomColorPicker != FeatureOption.Disabled,
            FilterItems => this.Config.DefaultChest.FilterItems != FeatureOption.Disabled,
            MenuForShippingBin => this.Config.CategorizeChest || this.Config.DefaultChest.ChestMenuTabs != FeatureOption.Disabled || this.Config.DefaultChest.ResizeChestMenu != FeatureOption.Disabled || this.Config.DefaultChest.SearchItems != FeatureOption.Disabled,
            OpenHeldChest => this.Config.DefaultChest.OpenHeldChest != FeatureOption.Disabled,
            OrganizeChest => this.Config.DefaultChest.OrganizeChest != FeatureOption.Disabled,
            ResizeChest => this.Config.DefaultChest.ResizeChest != FeatureOption.Disabled,
            ResizeChestMenu => this.Config.DefaultChest.ResizeChestMenu != FeatureOption.Disabled,
            SearchItems => this.Config.DefaultChest.SearchItems != FeatureOption.Disabled,
            SlotLock => this.Config.SlotLock,
            StashToChest => this.Config.DefaultChest.StashToChest != FeatureOptionRange.Disabled,
            UnloadChest => this.Config.DefaultChest.UnloadChest != FeatureOption.Disabled,
            _ => throw new InvalidOperationException($"Invalid feature toggle {this.GetType().Name}."),
        };

        switch (enabled)
        {
            case true when !this.Enabled:
                this.Activate();
                this.Enabled = true;
                return;
            case false when this.Enabled:
                this.Deactivate();
                this.Enabled = false;
                return;
        }
    }

    /// <summary>
    ///     Subscribe to events and apply any Harmony patches.
    /// </summary>
    protected abstract void Activate();

    /// <summary>
    ///     Unsubscribe from events, and reverse any Harmony patches.
    /// </summary>
    protected abstract void Deactivate();
}