namespace StardewMods.SpritePatcher.Framework;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.SpritePatcher.Framework.Interfaces;

/// <inheritdoc />
public abstract partial class BasePatchModel : IPatchModel
{
    /// <inheritdoc />
    private class PatchHelper(BasePatchModel patchModel) : IPatchHelper
    {
        public void InvalidateCacheOnChanged(object field, string eventName)
        {
            if (patchModel.currentObject is not null)
            {
                patchModel.netFieldManager.SubscribeToFieldEvent(patchModel.currentObject, field, eventName);
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
        public void SetTexture(Texture2D texture)
        {
            patchModel.path = texture.Name;
            patchModel.Texture = patchModel.textureManager.TryGetTexture(texture.Name, out var baseTexture)
                ? baseTexture
                : null;
        }

        /// <inheritdoc />
        public void SetTexture(string path, int index = 0, int width = 16, int height = 16)
        {
            if (index == -1)
            {
                return;
            }

            patchModel.path = path;
            patchModel.Texture = patchModel.ContentPack.ModContent.Load<IRawTextureData>(path);
            patchModel.Area = new Rectangle(
                width * (index % (patchModel.Texture.Width / width)),
                height * (index / (patchModel.Texture.Width / width)),
                width,
                height);
        }

        /// <inheritdoc />
        public void Log(string message) => patchModel.monitor.Log($"{patchModel.Id}: {message}", LogLevel.Info);
    }
}