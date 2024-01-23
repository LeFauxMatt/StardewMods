namespace StardewMods.SpritePatcher.Framework;

using Microsoft.Xna.Framework;
using StardewMods.SpritePatcher.Framework.Enums;
using StardewMods.SpritePatcher.Framework.Interfaces;
using StardewValley.ItemTypeDefinitions;
using StardewValley.TerrainFeatures;

/// <inheritdoc cref="IPatchModel" />
public abstract partial class BasePatchModel
{
    /// <inheritdoc />
    private class PatchHelper(BasePatchModel patchModel) : IPatchHelper
    {
        /// <inheritdoc />
        public void Log(string message) => patchModel.monitor.Log($"{patchModel.Id}: {message}", LogLevel.Info);

        /// <inheritdoc />
        public void InvalidateCacheOnChanged(object field, string eventName)
        {
            if (patchModel.currentObject is not null)
            {
                patchModel.netEventManager.Subscribe(patchModel.currentObject, field, eventName);
            }
        }

        /// <inheritdoc />
        public int GetIndexFromString(string input, string value, char separator = ',')
        {
            var values = input.Split(separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var index = Array.FindIndex(values, v => v.Equals(value, StringComparison.OrdinalIgnoreCase));
            return index;
        }

        public void SetAnimation(Animate animate, int frames)
        {
            if (animate == Animate.None || frames <= 1)
            {
                return;
            }

            patchModel.Animate = animate;
            patchModel.Frames = frames;
        }

        /// <inheritdoc />
        public void SetTexture(ParsedItemData data, float scale = -1f)
        {
            if (data.IsErrorItem)
            {
                return;
            }

            patchModel.path = data.GetTexture().Name;
            patchModel.Area = data.GetSourceRect();
            patchModel.Texture =
                patchModel.spriteSheetManager.TryGetTexture(data.GetTexture().Name, out var baseTexture)
                    ? baseTexture
                    : null;

            if (scale > 0)
            {
                patchModel.Scale = scale;
            }
        }

        /// <inheritdoc />
        public void SetTexture(string path, int index = 0, int width = 16, int height = 16, float scale = -1f)
        {
            if (index == -1)
            {
                return;
            }

            patchModel.path = path;
            patchModel.Texture = patchModel.ContentPack.ModContent.Load<IRawTextureData>(path);

            if (scale > 0)
            {
                patchModel.Scale = scale;
            }

            if (patchModel.Area == Rectangle.Empty)
            {
                patchModel.Area = new Rectangle(
                    width * (index % (patchModel.Texture.Width / width)),
                    height * (index / (patchModel.Texture.Width / width)),
                    width,
                    height);
            }
        }

        /// <inheritdoc />
        public void WithHeldObject(IHaveModData entity, Action<SObject, ParsedItemData> action, bool monitor = false)
        {
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
        public void WithLastInputItem(IHaveModData entity, Action<Item, ParsedItemData> action, bool monitor = false)
        {
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
            IHaveModData entity,
            Action<Dictionary<Direction, SObject?>> action,
            bool monitor = false)
        {
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
        public void WithPreserve(IHaveModData entity, Action<ParsedItemData> action, bool monitor = false)
        {
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
}