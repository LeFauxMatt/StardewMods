namespace Common.Records;

using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using SObject = StardewValley.Object;

/// <summary>
///     A record to represent the location and position of a placed object.
/// </summary>
internal readonly record struct PlacedObject(GameLocation Location, Vector2 Position)
{
    /// <summary>
    ///     Gets the <see cref="SObject" /> referred to by this record.
    /// </summary>
    public SObject Object
    {
        get
        {
            if (this.Location.Objects.TryGetValue(this.Position, out var obj))
            {
                return obj;
            }

            if (this.Location is FarmHouse farmHouse && this.Position.Equals(new(farmHouse.fridgePosition.X, farmHouse.fridgePosition.Y)))
            {
                return farmHouse.fridge.Value;
            }

            return default;
        }
    }
}