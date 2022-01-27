namespace BetterChests.Features;

using System;
using BetterChests.Enums;
using BetterChests.Interfaces;
using FuryCore.Interfaces;
using BetterChests.Services;
using StardewModdingAPI;

/// <inheritdoc />
internal abstract class Feature : IService
{
    private readonly Lazy<ManagedChests> _managedChests;
    private readonly Lazy<IFuryEvents> _furyEvents;

    /// <summary>
    /// Initializes a new instance of the <see cref="Feature"/> class.
    /// </summary>
    /// <param name="config">The <see cref="IConfigData" /> for options set by the player.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Internal and external dependency <see cref="IService" />.</param>
    protected Feature(IConfigModel config, IModHelper helper, IServiceLocator services)
    {
        this.Id = $"{ModEntry.ModUniqueId}.{this.GetType().Name}";
        this.Helper = helper;
        this.Config = config;
        this._managedChests = services.Lazy<ManagedChests>();
        this._furyEvents = services.Lazy<IFuryEvents>();
    }

    /// <summary>
    /// Gets an Id that uniquely describes the mod and feature.
    /// </summary>
    protected string Id { get; }

    /// <summary>
    /// Gets custom events provided by FuryCore.
    /// </summary>
    protected IFuryEvents FuryEvents
    {
        get => this._furyEvents.Value;
    }

    /// <summary>
    /// Gets SMAPIs Helper API for events, input, and content.
    /// </summary>
    protected IModHelper Helper { get; }

    /// <summary>
    /// Gets the <see cref="ManagedChests" /> service to track placed and player chests in the game.
    /// </summary>
    protected ManagedChests ManagedChests
    {
        get => this._managedChests.Value;
    }

    /// <summary>
    /// Gets the player configured mod options.
    /// </summary>
    protected IConfigModel Config { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the feature is currently enabled.
    /// </summary>
    private bool Enabled { get; set; }

    /// <summary>
    /// Toggles a feature on or off based on <see cref="IConfigData" />.
    /// </summary>
    public void Toggle()
    {
        var enabled = this switch
        {
            CarryChest => this.Config.DefaultChest.CarryChest != FeatureOption.Disabled,
            CategorizeChest => this.Config.DefaultChest.CategorizeChest != FeatureOption.Disabled,
            ChestMenuTabs => this.Config.DefaultChest.ChestMenuTabs != FeatureOption.Disabled,
            CollectItems => this.Config.DefaultChest.CollectItems != FeatureOption.Disabled,
            CraftFromChest => this.Config.DefaultChest.CraftFromChest != FeatureOptionRange.Disabled,
            CustomColorPicker => this.Config.DefaultChest.CustomColorPicker != FeatureOption.Disabled,
            FilterItems => this.Config.DefaultChest.FilterItems != FeatureOption.Disabled,
            OpenHeldChest => this.Config.DefaultChest.OpenHeldChest != FeatureOption.Disabled,
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
    /// Subscribe to events and apply any Harmony patches.
    /// </summary>
    protected abstract void Activate();

    /// <summary>
    /// Unsubscribe from events, and reverse any Harmony patches.
    /// </summary>
    protected abstract void Deactivate();
}