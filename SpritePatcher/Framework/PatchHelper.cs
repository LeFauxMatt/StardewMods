namespace StardewMods.SpritePatcher.Framework;

using Microsoft.Xna.Framework;
using Netcode;
using StardewMods.SpritePatcher.Framework.Interfaces;

/// <inheritdoc />
public abstract partial class BasePatchModel : IPatchModel
{
    /// <inheritdoc />
    private class PatchHelper(BasePatchModel patchModel) : IPatchHelper
    {
        public void InvalidateCacheOnChanged<T>(T entity, string fieldName)
            where T : IHaveModData, INetObject<NetFields>
        {
            // Concept -
            // Every patch can send an event to invalidate the cache of a ManagedObject
            // thus forcing that object to re-render itself on the next time it is drawn.
            // Add a service for associating net field changes of an object with
            // a patch's InvalidateCache method.
            // When a patch invalidates itself, it's net field changes are removed from the service.
            // When the patch is reapplied, it should re-add it's net field changes to the service.
        }

        /// <inheritdoc/>
        public int GetIndexFromString(string input, string value, char separator = ',')
        {
            var values = input.Split(separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var index = Array.FindIndex(values, v => v.Equals(value, StringComparison.OrdinalIgnoreCase));
            return index;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public void Log(string message) => patchModel.monitor.Log($"{patchModel.Id}: {message}", LogLevel.Info);
    }
}