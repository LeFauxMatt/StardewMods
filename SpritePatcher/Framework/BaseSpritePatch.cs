namespace StardewMods.SpritePatcher.Framework;

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

    private ISprite? currentSprite;
    private ISpriteSheet? currentSpriteSheet;
    private string currentPath = string.Empty;

    /// <summary>Initializes a new instance of the <see cref="BaseSpritePatch" /> class.</summary>
    /// <param name="args">The patch model arguments.</param>
    protected BaseSpritePatch(PatchModelCtorArgs args)
    {
        this.log = args.Log;
        this.netEventManager = args.NetEventManager;
        this.spriteSheetManager = args.SpriteSheetManager;
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

    /// <inheritdoc/>
    public PatchLayer Layer { get; set; }

    /// <inheritdoc/>
    public string Path { get; set; }

    /// <inheritdoc />
    public IRawTextureData Texture { get; set; }

    /// <inheritdoc />
    public Rectangle Area { get; set; }

    /// <inheritdoc />
    public Color Tint { get; set; }

    /// <inheritdoc />
    public Vector2 Offset { get; set; }

    /// <inheritdoc />
    public Animate Animate { get; set; }

    /// <inheritdoc />
    public int Frames { get; set; }

    /// <inheritdoc />
    public float Scale { get; set; }

    /// <inheritdoc />
    public float Alpha { get; set; }

    /// <inheritdoc/>
    public Color Color { get; set; }

    /// <inheritdoc/>
    public float Rotation { get; set; }

    /// <inheritdoc/>
    public SpriteEffects Effects { get; set; }

    /// <inheritdoc />
    public abstract bool Run(ISprite sprite, SpriteKey key);

    /// <summary>Resets the Texture, Area, and Tint properties of the object before running.</summary>
    /// <param name="sprite">The managed object requesting the patch.</param>
    /// <param name="spriteSheet">The spriteSheet that the patch is being applied to..</param>
    protected void BeforeRun(ISprite sprite, ISpriteSheet spriteSheet)
    {
        this.currentSprite = sprite;
        this.currentSpriteSheet = spriteSheet;
        this.SourceArea = Rectangle.Intersect(this.ContentModel.SourceArea, spriteSheet.SourceRectangle);
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
        this.currentSprite = null;
        this.currentSpriteSheet = null;
        return this.Texture != null;
    }
}