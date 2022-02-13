namespace StardewMods.FuryCore.Events;

using System;
using StardewModdingAPI.Events;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Models.CustomEvents;
using StardewMods.FuryCore.Services;

/// <inheritdoc />
internal class GameObjectsRemoved : SortedEventHandler<GameObjectsRemovedEventArgs>
{
    private readonly Lazy<GameObjects> _gameObjects;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GameObjectsRemoved" /> class.
    /// </summary>
    /// <param name="events">SMAPIs events.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public GameObjectsRemoved(IModEvents events, IModServices services)
    {
        this._gameObjects = services.Lazy<GameObjects>();
        events.GameLoop.DayEnding += this.OnDayEnding;
    }

    private GameObjects GameObjects
    {
        get => this._gameObjects.Value;
    }

    private void OnDayEnding(object sender, DayEndingEventArgs e)
    {
        var removed = this.GameObjects.PurgeCache();
        if (removed is not null)
        {
            this.InvokeAll(new(removed));
        }
    }
}