namespace StardewMods.SpritePatcher.Framework;

using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.SpritePatcher.Framework.Enums;
using StardewMods.SpritePatcher.Framework.Interfaces;
using StardewMods.SpritePatcher.Framework.Models;

/// <inheritdoc cref="ISpritePatch" />
public abstract partial class BaseSpritePatch : ISpritePatch
{
    private readonly ILog log;
    private readonly INetEventManager netEventManager;
    private readonly ISpriteSheetManager spriteSheetManager;

    private ISprite? currentObject;
    private string currentPath = string.Empty;
    private SpriteKey spriteKey;

    /// <summary>Initializes a new instance of the <see cref="BaseSpritePatch" /> class.</summary>
    /// <param name="args">The patch model arguments.</param>
    protected BaseSpritePatch(PatchModelCtorArgs args)
    {
        this.log = args.Log;
        this.netEventManager = args.NetEventManager;
        this.spriteSheetManager = args.SpriteSheetManager;
        this.spriteKey = default(SpriteKey);

        this.Id = args.Id;
        this.ContentPack = args.ContentPack;
        this.ContentModel = args.ContentModel;
    }

    /// <inheritdoc />
    public string Id { get; }

    /// <inheritdoc />
    public IContentPack ContentPack { get; }

    /// <inheritdoc/>
    public IContentModel ContentModel { get; }

    /// <inheritdoc />
    public Rectangle SourceArea { get; private set; }

    /// <inheritdoc />
    public IRawTextureData? Texture { get; set; }

    /// <inheritdoc />
    public Rectangle Area { get; set; }

    /// <inheritdoc />
    public Color? Tint { get; set; }

    /// <inheritdoc />
    public float Alpha { get; set; }

    /// <inheritdoc />
    public float Scale { get; set; }

    /// <inheritdoc />
    public int Frames { get; set; }

    /// <inheritdoc />
    public Animate Animate { get; set; }

    /// <inheritdoc />
    public Vector2 Offset { get; set; }

    /// <inheritdoc/>
    public Color? Color { get; set; }

    /// <inheritdoc/>
    public float? Rotation { get; set; }

    /// <inheritdoc/>
    public SpriteEffects? Effects { get; set; }

    /// <inheritdoc />
    public int GetCurrentId()
    {
        var hash = default(HashCode);
        hash.Add(this.Id);
        hash.Add(this.ContentModel.Target);
        hash.Add(this.currentPath);
        hash.Add(this.Area);
        hash.Add(this.Offset);
        hash.Add(this.Tint);
        hash.Add(this.Scale);
        hash.Add(this.Animate);
        hash.Add(this.Frames);
        hash.Add(this.Alpha);
        return hash.ToHashCode();
    }

    /// <inheritdoc />
    public abstract bool Run(ISprite sprite, SpriteKey key);

    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(Path.Join(this.Id, this.currentPath));
        sb.Append('_');
        sb.Append(this.Area);

        if (this.Offset != Vector2.Zero)
        {
            sb.Append('_');
            sb.Append(this.Offset);
        }

        if (this.Tint != null)
        {
            sb.Append('_');
            sb.Append(this.Tint);
        }

        if ((int)this.Scale != 1)
        {
            sb.Append('_');
            sb.Append((int)this.Scale);
        }

        if (this.Animate != Animate.None && this.Frames > 1)
        {
            sb.Append('_');
            sb.Append(this.Animate.ToStringFast());
            sb.Append('_');
            sb.Append(this.Frames);
        }

        if (this.Alpha < 0.99f)
        {
            sb.Append('_');
            sb.Append(this.Alpha);
        }

        return sb.ToString();
    }

    /// <summary>Resets the Texture, Area, and Tint properties of the object before running.</summary>
    /// <param name="sprite">The managed object requesting the patch.</param>
    protected void BeforeRun(ISprite sprite, SpriteKey key)
    {
        this.spriteKey = key;
        this.SourceArea = Rectangle.Intersect(this.ContentModel.SourceArea, key.Area);
        this.currentObject = sprite;
        this.Texture = null;
        this.Area = Rectangle.Empty;
        this.Tint = null;
        this.Scale = 1f;
        this.Frames = 1;
        this.Animate = Animate.None;
        this.Offset = Vector2.Zero;
        this.Alpha = 1f;
        this.Color = null;
        this.Rotation = null;
        this.Effects = null;
    }

    /// <summary>Validate the Texture, Area, and Tint properties of the object after running.</summary>
    /// <param name="sprite">The managed object requesting the patch.</param>
    /// <returns><c>true</c> if the patch should be applied; otherwise, <c>false</c>.</returns>
    protected bool AfterRun(ISprite sprite)
    {
        this.currentObject = null;
        this.spriteKey = default(SpriteKey);
        return this.Texture != null;
    }
}