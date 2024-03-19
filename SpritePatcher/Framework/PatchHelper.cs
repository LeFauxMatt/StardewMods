namespace StardewMods.SpritePatcher.Framework;

using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.SpritePatcher.Framework.Enums;
using StardewMods.SpritePatcher.Framework.Interfaces;
using StardewMods.SpritePatcher.Framework.Models;
using StardewValley.ItemTypeDefinitions;

/// <inheritdoc cref="IPatchHelper" />
[SuppressMessage("SMAPI.CommonErrors", "AvoidImplicitNetFieldCast", Justification = "Reviewed.")]
public abstract partial class BaseSpritePatch : IPatchHelper
{
    /// <inheritdoc />
    public void ApplyTexture(
        Texture2D texture,
        Rectangle sourceArea,
        float scale = -1f,
        float alpha = -1f)
    {
        if (!this.spriteSheetManager.TryGetTexture(texture.Name, out var rawTexture))
        {
            return;
        }

        this.ApplyTexture(rawTexture, texture.Name, sourceArea, scale, alpha);
    }

    /// <inheritdoc />
    public void ApplyTexture(
        string? path,
        Rectangle sourceArea,
        float scale = -1f,
        float alpha = -1f)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        try
        {
            var rawTexture = this.ContentPack.ModContent.Load<IRawTextureData>(path);
            this.ApplyTexture(rawTexture, path, sourceArea, scale, alpha);
        }
        catch (Exception e)
        {
            // Do nothing
        }
    }

    /// <inheritdoc />
    public void ApplyTexture(Item? item, float scale = -1f, float alpha = -1f)
    {
        if (item is null)
        {
            return;
        }

        var data = ItemRegistry.GetDataOrErrorItem(item.QualifiedItemId);
        this.ApplyTexture(data, scale, alpha);
    }

    /// <inheritdoc />
    public void ApplyTexture(ParsedItemData data, float scale = -1f, float alpha = -1f)
    {
        if (data.IsErrorItem)
        {
            return;
        }

        var path = data.GetTextureName();
        if (!this.spriteSheetManager.TryGetTexture(path, out var rawTexture))
        {
            return;
        }

        this.ApplyTexture(rawTexture, path, data.GetSourceRect(), scale, alpha);
    }

    public Rectangle GetAreaFromIndex(int index, int width = -1, int height = -1)
    {
        if (index < 0)
        {
            return this.spriteKey.SourceRectangle;
        }

        if (width == -1)
        {
            width = this.spriteKey.SourceRectangle.Width;
        }

        if (height == -1)
        {
            height = this.spriteKey.SourceRectangle.Height;
        }

        return new Rectangle(width * index, 0, width, height);
    }

    /// <inheritdoc />
    public int GetIndexFromString(string input, string value, char separator = ',')
    {
        var values = input.Split(separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var index = Array.FindIndex(values, v => v.Equals(value, StringComparison.OrdinalIgnoreCase));
        return index;
    }

    /// <inheritdoc />
    public T GetOrSetData<T>(string key, T value)
    {
        if (this.currentSprite is null)
        {
            return value;
        }

        if (this.currentSprite.Entity.modData.TryGetValue(key, out var stringResult))
        {
            var typeConverter = TypeDescriptor.GetConverter(typeof(T));
            if (typeConverter.IsValid(stringResult))
            {
                return (T)typeConverter.ConvertFromInvariantString(stringResult)!;
            }
        }

        this.currentSprite.Entity.modData[key] = value!.ToString();
        return value;
    }

    /// <inheritdoc />
    public (SObject Object, ParsedItemData Data) GetHeldObject(IHaveModData? entity = null)
    {
        entity ??= this.currentSprite?.Entity;
        if (entity is not SObject obj)
        {
            throw new InapplicableContextException();
        }

        this.InvalidateCacheOnChanged(obj.heldObject, "fieldChangeVisibleEvent");

        if (obj.heldObject.Value == null)
        {
            throw new InapplicableContextException();
        }

        var data = ItemRegistry.GetDataOrErrorItem(obj.heldObject.Value.QualifiedItemId);
        if (!data.IsErrorItem)
        {
            return (obj.heldObject.Value, data);
        }

        throw new InapplicableContextException();
    }

    /// <inheritdoc />
    public (Item Item, ParsedItemData Data) GetLastInputItem(IHaveModData? entity = null)
    {
        entity ??= this.currentSprite?.Entity;
        if (entity is not SObject obj)
        {
            throw new InapplicableContextException();
        }

        this.InvalidateCacheOnChanged(obj.lastInputItem, "fieldChangeVisibleEvent");

        if (obj.lastInputItem.Value == null)
        {
            throw new InapplicableContextException();
        }

        var data = ItemRegistry.GetDataOrErrorItem(obj.lastInputItem.Value.QualifiedItemId);
        if (!data.IsErrorItem)
        {
            return (obj.lastInputItem.Value, data);
        }

        throw new InapplicableContextException();
    }

    /// <inheritdoc />
    public Dictionary<Direction, SObject?> GetNeighbors(IHaveModData? entity = null)
    {
        entity ??= this.currentSprite?.Entity;
        if (entity is not SObject
            {
                Location: not null,
            } obj)
        {
            throw new InapplicableContextException();
        }

        this.InvalidateCacheOnChanged(obj.Location.netObjects, "OnValueAdded");
        this.InvalidateCacheOnChanged(obj.Location.netObjects, "OnValueRemoved");

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
            return neighbors;
        }

        throw new InapplicableContextException();
    }

    /// <inheritdoc />
    public ParsedItemData GetPreserve(IHaveModData? entity = null)
    {
        entity ??= this.currentSprite?.Entity;
        if (entity is not SObject obj)
        {
            throw new InapplicableContextException();
        }

        this.InvalidateCacheOnChanged(obj.preservedParentSheetIndex, "fieldChangeVisibleEvent");
        if (obj.preservedParentSheetIndex.Value == null)
        {
            throw new InapplicableContextException();
        }

        var data = ItemRegistry.GetDataOrErrorItem("(O)" + obj.preservedParentSheetIndex.Value);
        if (!data.IsErrorItem)
        {
            return data;
        }

        throw new InapplicableContextException();
    }

    /// <inheritdoc />
    public void InvalidateCacheOnChanged(object field, string eventName)
    {
        if (this.currentSprite is not null)
        {
            this.netEventManager.Subscribe(this.currentSprite, field, eventName);
        }
    }

    /// <inheritdoc />
    public void Log(string message) => this.log.Trace($"{this.Id}: {message}");

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

    private void ApplyTexture(
        IRawTextureData texture,
        string path,
        Rectangle sourceArea,
        float scale = -1f,
        float alpha = -1f)
    {
        this.Texture = texture;
        this.currentPath = path;

        if (scale > 0)
        {
            this.Scale = scale;
        }

        if (alpha > 0)
        {
            this.Alpha = alpha;
        }

        if (this.Area != Rectangle.Empty)
        {
            return;
        }

        if (sourceArea.X > texture.Width && texture.Width >= sourceArea.Width)
        {
            var index = sourceArea.X / sourceArea.Width;
            this.Area = sourceArea with
            {
                X = sourceArea.Width * (index % texture.Width / sourceArea.Width),
                Y = sourceArea.Height * (index / (texture.Width / sourceArea.Width)),
            };

            return;
        }

        this.Area = sourceArea;
    }
}