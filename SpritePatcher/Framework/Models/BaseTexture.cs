namespace StardewMods.SpritePatcher.Framework.Models;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/// <inheritdoc />
internal sealed class BaseTexture : IRawTextureData
{
    private readonly string path;

    private Color[]? data;
    private int? width;
    private int? height;

    /// <summary>Initializes a new instance of the <see cref="BaseTexture" /> class.</summary>
    /// <param name="path">The path to the texture.</param>
    public BaseTexture(string path) => this.path = path;

    /// <inheritdoc />
    public Color[] Data
    {
        get
        {
            if (this.data is null)
            {
                this.Reinitialize();
            }

            return this.data;
        }
    }

    /// <inheritdoc />
    public int Width
    {
        get
        {
            if (this.width is null)
            {
                this.Reinitialize();
            }

            return this.width.Value;
        }
    }

    /// <inheritdoc />
    public int Height
    {
        get
        {
            if (this.height is null)
            {
                this.Reinitialize();
            }

            return this.height.Value;
        }
    }

    /// <summary>Clears the cache by setting the data, width, and height variables to null.</summary>
    public void ClearCache()
    {
        this.data = null;
        this.width = null;
        this.height = null;
    }

    /// <summary>Gets the color data for a specific area of the texture.</summary>
    /// <param name="area">The area of the texture to get the data from.</param>
    /// <returns>An array of colors representing the specified area.</returns>
    public Color[] GetData(Rectangle area)
    {
        // Validate the area to ensure it's within the texture bounds
        if (area.X < 0
            || area.Y < 0
            || area.Width <= 0
            || area.Height <= 0
            || area.Right > this.Width
            || area.Bottom > this.Height)
        {
            throw new ArgumentException("The specified area is outside the bounds of the texture.", nameof(area));
        }

        var areaData = new Color[area.Width * area.Height];
        var dataIndex = 0;

        for (var y = area.Y; y < area.Y + area.Height; y++)
        {
            for (var x = area.X; x < area.X + area.Width; x++)
            {
                areaData[dataIndex++] = this.Data[(y * this.Width) + x];
            }
        }

        return areaData;
    }

    [MemberNotNull(nameof(BaseTexture.data), nameof(BaseTexture.width), nameof(BaseTexture.height))]
    private void Reinitialize()
    {
        var texture = Game1.content.Load<Texture2D>(this.path);
        this.width = texture.Width;
        this.height = texture.Height;
        this.data = new Color[texture.Width * texture.Height];
        texture.GetData(0, new Rectangle(0, 0, texture.Width, texture.Height), this.data, 0, this.data.Length);
    }
}