namespace StardewMods.ExpandedStorage.Models;

using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Integrations.ExpandedStorage;
using StardewValley.Objects;

/// <inheritdoc />
internal sealed class ManagedStorage : IManagedStorage
{
    private readonly IContentPack _contentPack;
    private readonly string _name;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ManagedStorage" /> class.
    /// </summary>
    /// <param name="name">The storage name.</param>
    /// <param name="contentPack">The content pack to load storage assets from.</param>
    /// <param name="storage">The storage data.</param>
    public ManagedStorage(string name, IContentPack contentPack, ICustomStorage storage)
    {
        this._name = name;
        this._contentPack = contentPack;

        storage.CopyTo(this);
        this.DisplayName = contentPack.Translation.Get($"{name}.display-name");
        this.Description = contentPack.Translation.Get($"{name}.description");
    }

    /// <inheritdoc />
    public string CloseNearbySound { get; set; } = "doorCreakReverse";

    /// <inheritdoc />
    public int Depth { get; set; } = 16;

    /// <inheritdoc />
    public string Description { get; set; }

    /// <inheritdoc />
    public string DisplayName { get; set; }

    /// <inheritdoc />
    public int Height { get; set; } = 32;

    /// <inheritdoc />
    public bool IsFridge { get; set; } = false;

    /// <inheritdoc />
    public IDictionary<string, string> ModData { get; set; } = new Dictionary<string, string>();

    /// <inheritdoc />
    public float OpenNearby { get; set; } = 0;

    /// <inheritdoc />
    public string OpenNearbySound { get; set; } = "doorCreak";

    /// <inheritdoc />
    public string OpenSound { get; set; } = "openChest";

    /// <inheritdoc />
    public bool PlayerColor { get; set; } = false;

    /// <inheritdoc />
    public Chest.SpecialChestTypes SpecialChestType { get; set; } = Chest.SpecialChestTypes.None;

    /// <inheritdoc />
    public Texture2D Texture
    {
        get
        {
            if (string.IsNullOrWhiteSpace(this.TexturePath))
            {
                throw new($"Texture missing for {this._name}");
            }

            try
            {
                return this._contentPack.ModContent.Load<Texture2D>(this.TexturePath);
            }
            catch
            {
                throw new($"Texture missing for {this._name}");
            }
        }
    }

    /// <inheritdoc />
    public string? TexturePath { get; set; }

    /// <inheritdoc />
    public int Width { get; set; } = 16;
}