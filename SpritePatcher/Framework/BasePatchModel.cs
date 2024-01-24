namespace StardewMods.SpritePatcher.Framework;

using System.Text;
using Microsoft.Xna.Framework;
using StardewMods.SpritePatcher.Framework.Enums;
using StardewMods.SpritePatcher.Framework.Interfaces;
using StardewMods.SpritePatcher.Framework.Models;

/// <inheritdoc />
public abstract partial class BasePatchModel : IPatchModel
{
    private readonly IMonitor monitor;
    private readonly INetEventManager netEventManager;
    private readonly ISpriteSheetManager spriteSheetManager;
    private Rectangle sourceArea;

    private string path = string.Empty;
    private ISprite? currentObject;
    private SpriteKey? spriteKey;

    /// <summary>Initializes a new instance of the <see cref="BasePatchModel" /> class.</summary>
    /// <param name="args">The patch model arguments.</param>
    protected BasePatchModel(PatchModelCtorArgs args)
    {
        this.monitor = args.Monitor;
        this.netEventManager = args.NetEventManager;
        this.spriteSheetManager = args.SpriteSheetManager;

        this.Id = args.Id;
        this.ContentPack = args.ContentPack;
        this.Target = args.ContentModel.Target;
        this.sourceArea = args.ContentModel.SourceArea;
        this.SourceArea = args.ContentModel.SourceArea;
        this.DrawMethods = args.ContentModel.DrawMethods;
        this.PatchMode = args.ContentModel.PatchMode;
        this.Helper = new PatchHelper(this);
    }

    /// <inheritdoc />
    public string Id { get; }

    /// <inheritdoc />
    public IContentPack ContentPack { get; }

    /// <inheritdoc />
    public string Target { get; }

    /// <inheritdoc />
    public List<DrawMethod> DrawMethods { get; }

    /// <inheritdoc />
    public PatchMode PatchMode { get; }

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

    /// <summary>Gets a helper that provides useful methods for performing common operations.</summary>
    protected IPatchHelper Helper { get; }

    /// <inheritdoc />
    public bool Intersects(Rectangle area) => this.sourceArea.Intersects(area);

    /// <inheritdoc />
    public int GetCurrentId()
    {
        var hash = default(HashCode);
        hash.Add(this.Id);
        hash.Add(this.Target);
        hash.Add(this.path);
        hash.Add(this.Area);
        hash.Add(this.PatchMode);
        hash.Add(this.Offset);
        hash.Add(this.Tint);
        hash.Add(this.Scale);
        hash.Add(this.Animate);
        hash.Add(this.Frames);
        hash.Add(this.Alpha);
        return hash.ToHashCode();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(Path.Join(this.Id, this.path));
        sb.Append('_');
        sb.Append(this.Area);
        sb.Append('_');
        sb.Append(this.PatchMode);

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

    /// <inheritdoc />
    public abstract bool Run(ISprite sprite, SpriteKey key);

    /// <summary>Resets the Texture, Area, and Tint properties of the object before running.</summary>
    /// <param name="sprite">The managed object requesting the patch.</param>
    /// <param name="key">A key for the original texture method.</param>
    protected void BeforeRun(ISprite sprite, SpriteKey key)
    {
        this.currentObject = sprite;
        this.spriteKey = key;
        this.SourceArea = Rectangle.Intersect(this.sourceArea, key.Area);
        this.Texture = null;
        this.Area = Rectangle.Empty;
        this.Tint = null;
        this.Scale = 1f;
        this.Frames = 1;
        this.Animate = Animate.None;
        this.Offset = Vector2.Zero;
        this.Alpha = 1f;
    }

    /// <summary>Validate the Texture, Area, and Tint properties of the object after running.</summary>
    /// <param name="sprite">The managed object requesting the patch.</param>
    /// <returns><c>true</c> if the patch should be applied; otherwise, <c>false</c>.</returns>
    protected bool AfterRun(ISprite sprite)
    {
        this.currentObject = null;
        this.spriteKey = null;
        return this.Texture != null;
    }
}