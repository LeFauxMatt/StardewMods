namespace StardewMods.BetterChests.Models;

using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;

internal readonly record struct LocationChest(GameLocation Location, Vector2 Position, Chest Chest, string Name);