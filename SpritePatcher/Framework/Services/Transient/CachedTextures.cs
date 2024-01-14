namespace StardewMods.SpritePatcher.Framework.Services.Transient;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.SpritePatcher.Framework.Enums;

/// <summary>Represents an object being managed by the mod.</summary>
internal sealed partial class ManagedObject
{
    private class CachedTextures(ManagedObject managedObject)
    {
        private readonly Dictionary<string, Dictionary<(Rectangle? Area, DrawMethod Method), CachedTexture>>
            cachedTextures = [];

        public void AddOrUpdate(
            string target,
            Rectangle? area,
            DrawMethod method,
            Texture2D texture,
            List<(string Path, Rectangle? Area, Color? Tint, PatchMode Mode)> layers)
        {
            if (!this.cachedTextures.TryGetValue(target, out var textures))
            {
                textures = new Dictionary<(Rectangle? Area, DrawMethod Method), CachedTexture>();
                this.cachedTextures[target] = textures;
            }

            if (!textures.TryGetValue((area, method), out var cachedTexture))
            {
                cachedTexture = new CachedTexture(texture, [..layers]);
                textures[(area, method)] = cachedTexture;
                return;
            }

            cachedTexture.Texture = texture;
            cachedTexture.Layers = [..layers];
        }

        public void ClearCache(IEnumerable<string> targets)
        {
            foreach (var target in targets)
            {
                this.cachedTextures.Remove(target);
            }
        }

        public bool TryGet(
            string target,
            Rectangle? area,
            DrawMethod method,
            [NotNullWhen(true)] out Texture2D? texture)
        {
            if (!this.cachedTextures.TryGetValue(target, out var textures)
                || !textures.TryGetValue((area, method), out var cachedTexture))
            {
                texture = null;
                return false;
            }

            texture = cachedTexture.Texture;
            return true;
        }

        private class CachedTexture(
            Texture2D texture,
            HashSet<(string Path, Rectangle? Area, Color? Tint, PatchMode Mode)> layers)
        {
            public HashSet<(string Path, Rectangle? Area, Color? Tint, PatchMode Mode)> Layers { get; set; } = layers;

            public Texture2D Texture { get; set; } = texture;
        }
    }
}