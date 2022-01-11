namespace BetterChests.Models;

using System;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;

internal readonly record struct ManagedChestId : IEquatable<Chest>
{
    public ManagedChestId(GameLocation location, Vector2 position)
    {
        this.Location = location;
        this.Position = position;
        this.Player = null;
        this.Index = -1;
    }

    public ManagedChestId(Farmer player, int index)
    {
        this.Player = player;
        this.Index = index;
        this.Location = null;
        this.Position = Vector2.Zero;
    }

    public GameLocation Location { get; }
    public Vector2 Position { get; }
    public Farmer Player { get; }
    public int Index { get; }

    public static implicit operator Chest(ManagedChestId managedChestId)
    {
        return (managedChestId.Location, managedChestId.Position, managedChestId.Player, managedChestId.Index) switch
        {
            (not null, _, _, _) when managedChestId.Location.Objects.TryGetValue(managedChestId.Position, out var obj) => obj as Chest,
            (_, _, not null, > -1) => managedChestId.Player.Items.ElementAtOrDefault(managedChestId.Index) as Chest,
            _ => null,
        };
    }

    public bool Equals(Chest other)
    {
        Chest chest = this;
        return ReferenceEquals(chest, other);
    }
}