namespace StardewMods.SpritePatcher.Framework;

using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.SpritePatcher.Framework.Enums;
using StardewMods.SpritePatcher.Framework.Interfaces;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Mods;

/// <inheritdoc cref="IPatchHelper" />
[SuppressMessage("SMAPI.CommonErrors", "AvoidImplicitNetFieldCast", Justification = "Reviewed.")]
public abstract partial class BasePatchModel : IPatchHelper
{
    /// <inheritdoc />
    public void Log(string message) => this.log.Trace($"{this.Id}: {message}");

    /// <inheritdoc />
    public int GetOrSetData(string key, int value)
    {
        if (this.currentObject is null)
        {
            return value;
        }

        if (this.currentObject.Entity.modData.TryGetValue(key, out var stringResult)
            && int.TryParse(stringResult, out var result))
        {
            return result;
        }

        this.currentObject.Entity.modData[key] = value.ToString(CultureInfo.InvariantCulture);
        return value;
    }

    /// <inheritdoc />
    public double GetOrSetData(string key, double value)
    {
        if (this.currentObject is null)
        {
            return value;
        }

        if (this.currentObject.Entity.modData.TryGetValue(key, out var stringResult)
            && int.TryParse(stringResult, out var result))
        {
            return result;
        }

        this.currentObject.Entity.modData[key] = value.ToString(CultureInfo.InvariantCulture);
        return value;
    }

    /// <inheritdoc />
    public void InvalidateCacheOnChanged(object field, string eventName)
    {
        if (this.currentObject is not null)
        {
            this.netEventManager.Subscribe(this.currentObject, field, eventName);
        }
    }

    /// <inheritdoc />
    public int GetIndexFromString(string input, string value, char separator = ',')
    {
        var values = input.Split(separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var index = Array.FindIndex(values, v => v.Equals(value, StringComparison.OrdinalIgnoreCase));
        return index;
    }

    /// <inheritdoc />
    public void SetAnimation(Animate animate, int frames)
    {
        if (animate == Animate.None || frames <= 1)
        {
            return;
        }

        this.Animate = animate;
        this.Frames = frames;
    }

    /// <inheritdoc />
    public void SetTexture(Texture2D texture, float scale = -1, float alpha = -1f)
    {
        this.Texture = this.spriteSheetManager.TryGetTexture(texture.Name, out var baseTexture) ? baseTexture : null;
        if (this.Texture is null)
        {
            return;
        }

        this.currentPath = texture.Name;
        this.Area = texture.Bounds;

        if (scale > 0)
        {
            this.Scale = scale;
        }

        if (alpha > 0)
        {
            this.Alpha = alpha;
        }
    }

    /// <inheritdoc />
    public void SetTexture(Item? item, float scale = -1f, float alpha = -1f)
    {
        if (item is null)
        {
            return;
        }

        var data = ItemRegistry.GetDataOrErrorItem(item.QualifiedItemId);
        if (data.IsErrorItem)
        {
            return;
        }

        this.currentPath = data.GetTexture().Name;
        this.Area = data.GetSourceRect();
        this.Texture = this.spriteSheetManager.TryGetTexture(data.GetTexture().Name, out var baseTexture)
            ? baseTexture
            : null;

        if (scale > 0)
        {
            this.Scale = scale;
        }

        if (alpha > 0)
        {
            this.Alpha = alpha;
        }
    }

    /// <inheritdoc />
    public void SetTexture(ParsedItemData data, float scale = -1f, float alpha = -1f)
    {
        if (data.IsErrorItem)
        {
            return;
        }

        this.currentPath = data.GetTexture().Name;
        this.Area = data.GetSourceRect();
        this.Texture = this.spriteSheetManager.TryGetTexture(data.GetTexture().Name, out var baseTexture)
            ? baseTexture
            : null;

        if (scale > 0)
        {
            this.Scale = scale;
        }

        if (alpha > 0)
        {
            this.Alpha = alpha;
        }
    }

    /// <inheritdoc />
    public void SetTexture(
        string? path,
        int index = 0,
        int width = -1,
        int height = -1,
        float scale = -1f,
        float alpha = -1f,
        bool vanilla = false)
    {
        if (string.IsNullOrWhiteSpace(path) || index == -1)
        {
            return;
        }

        if (width == -1)
        {
            width = this.spriteKey.Area.Width;
        }

        if (height == -1)
        {
            height = this.spriteKey.Area.Height;
        }

        this.currentPath = path;
        this.Texture = vanilla && this.spriteSheetManager.TryGetTexture(path, out var baseTexture)
            ? baseTexture
            : this.ContentPack.ModContent.Load<IRawTextureData>(path);

        if (scale > 0)
        {
            this.Scale = scale;
        }

        if (alpha > 0)
        {
            this.Alpha = alpha;
        }

        if (this.Area == Rectangle.Empty)
        {
            this.Area = new Rectangle(
                this.Texture.Width > width ? width * (index % (this.Texture.Width / width)) : 0,
                this.Texture.Width > width ? height * (index / (this.Texture.Width / width)) : 0,
                width,
                height);
        }
    }

    /// <inheritdoc />
    public void WithHeldObject(Action<SObject, ParsedItemData> action, bool monitor = true, IHaveModData? entity = null)
    {
        entity ??= this.currentObject?.Entity;
        if (entity is not SObject obj)
        {
            return;
        }

        if (monitor)
        {
            this.InvalidateCacheOnChanged(obj.heldObject, "fieldChangeVisibleEvent");
        }

        if (obj.heldObject.Value == null)
        {
            return;
        }

        var data = ItemRegistry.GetDataOrErrorItem(obj.heldObject.Value.QualifiedItemId);
        if (!data.IsErrorItem)
        {
            action(obj.heldObject.Value, data);
        }
    }

    /// <inheritdoc />
    public void WithLastInputItem(Action<Item, ParsedItemData> action, bool monitor = true, IHaveModData? entity = null)
    {
        entity ??= this.currentObject?.Entity;
        if (entity is not SObject obj)
        {
            return;
        }

        if (monitor)
        {
            this.InvalidateCacheOnChanged(obj.lastInputItem, "fieldChangeVisibleEvent");
        }

        if (obj.lastInputItem.Value == null)
        {
            return;
        }

        var data = ItemRegistry.GetDataOrErrorItem(obj.lastInputItem.Value.QualifiedItemId);
        if (!data.IsErrorItem)
        {
            action(obj.lastInputItem.Value, data);
        }
    }

    /// <inheritdoc />
    public void WithNeighbors(
        Action<Dictionary<Direction, SObject?>> action,
        bool monitor = true,
        IHaveModData? entity = null)
    {
        entity ??= this.currentObject?.Entity;
        if (entity is not SObject
            {
                Location: not null,
            } obj)
        {
            return;
        }

        if (monitor)
        {
            this.InvalidateCacheOnChanged(obj.Location.netObjects, "OnValueAdded");
            this.InvalidateCacheOnChanged(obj.Location.netObjects, "OnValueRemoved");
        }

        var neighbors = new Dictionary<Direction, SObject?>();
        foreach (var direction in DirectionExtensions.GetValues())
        {
            var position = direction switch
            {
                Direction.Up => obj.TileLocation with { Y = obj.TileLocation.Y - 1 },
                Direction.Down => obj.TileLocation with { Y = obj.TileLocation.Y + 1 },
                Direction.Left => obj.TileLocation with { X = obj.TileLocation.X - 1 },
                Direction.Right => obj.TileLocation with { X = obj.TileLocation.X + 1 },
                _ => obj.TileLocation,
            };

            neighbors[direction] = obj.Location.Objects.TryGetValue(position, out var neighbor) ? neighbor : null;
        }

        if (neighbors.Values.OfType<SObject>().Any())
        {
            action(neighbors);
        }
    }

    /// <inheritdoc />
    public void WithPreserve(Action<ParsedItemData> action, bool monitor = true, IHaveModData? entity = null)
    {
        entity ??= this.currentObject?.Entity;
        if (entity is not SObject obj)
        {
            return;
        }

        if (monitor)
        {
            this.InvalidateCacheOnChanged(obj.preservedParentSheetIndex, "fieldChangeVisibleEvent");
        }

        if (obj.preservedParentSheetIndex.Value == null)
        {
            return;
        }

        var data = ItemRegistry.GetDataOrErrorItem("(O)" + obj.preservedParentSheetIndex.Value);
        if (!data.IsErrorItem)
        {
            action(data);
        }
    }
}

/// <summary>Common extension methods.</summary>
public static class Extensions
{
    /// <summary>Gets the value with the specified key or add if it does not exist.</summary>
    /// <param name="modData">The ModDataDictionary instance.</param>
    /// <param name="key">The key of the value to get or set.</param>
    /// <param name="value">The value to set if the key does not exist.</param>
    /// <returns>The value associated with the specified key if the key.</returns>
    public static int GetOrSet(this ModDataDictionary modData, string key, int value)
    {
        if (modData.TryGetValue(key, out var stringResult) && int.TryParse(stringResult, out var result))
        {
            return result;
        }

        modData[key] = value.ToString(CultureInfo.InvariantCulture);
        return value;
    }

    /// <summary>Gets the value with the specified key or add if it does not exist.</summary>
    /// <param name="modData">The ModDataDictionary instance.</param>
    /// <param name="key">The key of the value to get or set.</param>
    /// <param name="value">The value to set if the key does not exist.</param>
    /// <returns>The value associated with the specified key if the key.</returns>
    public static double GetOrSet(this ModDataDictionary modData, string key, double value)
    {
        if (modData.TryGetValue(key, out var stringResult) && int.TryParse(stringResult, out var result))
        {
            return result;
        }

        modData[key] = value.ToString(CultureInfo.InvariantCulture);
        return value;
    }
}