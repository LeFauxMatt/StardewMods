namespace BetterChests.Features;

using System;
using FuryCore.Interfaces;
using FuryCore.Services;
using Models;
using Services;
using StardewModdingAPI;
using StardewValley.Menus;

/// <inheritdoc />
internal abstract class Feature : IService
{
    private readonly Lazy<ManagedChests> _managedChests;
    private readonly Lazy<IFuryEvents> _furyEvents;

    /// <summary>
    /// Initializes a new instance of the <see cref="Feature"/> class.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="helper"></param>
    /// <param name="services"></param>
    protected Feature(ModConfig config, IModHelper helper, ServiceCollection services)
    {
        this.Id = $"{ModEntry.ModUniqueId}.{this.GetType().Name}";
        this.Config = config;
        this.Helper = helper;
        this._managedChests = services.Lazy<ManagedChests>();
        this._furyEvents = services.Lazy<IFuryEvents>();
    }

    protected string Id { get; }

    protected IFuryEvents FuryEvents
    {
        get => this._furyEvents.Value;
    }

    protected IModHelper Helper { get; }

    protected ManagedChests ManagedChests
    {
        get => this._managedChests.Value;
    }

    protected ModConfig Config { get; }

    public abstract void Activate();

    public abstract void Deactivate();
}