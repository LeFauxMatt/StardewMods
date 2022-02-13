namespace StardewMods.BetterChests.Models;

using StardewValley;
using StardewValley.Objects;

public record PlayerChest(Farmer Player, Chest Chest, string Name);