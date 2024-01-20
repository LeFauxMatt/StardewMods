namespace StardewMods.SpritePatcher.Framework;

using System.Globalization;
using System.Text;
using Microsoft.Xna.Framework;
using StardewMods.SpritePatcher.Framework.Enums;
using StardewMods.SpritePatcher.Framework.Interfaces;
using StardewMods.SpritePatcher.Framework.Models;

/// <inheritdoc />
public abstract partial class BasePatchModel : IPatchModel
{
    private readonly IMonitor monitor;
    private readonly INetFieldManager netFieldManager;
    private readonly ITextureManager textureManager;

    private string path = string.Empty;
    private IManagedObject? currentObject;

    /// <summary>Initializes a new instance of the <see cref="BasePatchModel" /> class.</summary>
    /// <param name="args">The patch model arguments.</param>
    protected BasePatchModel(PatchModelCtorArgs args)
    {
        this.monitor = args.Monitor;
        this.netFieldManager = args.NetFieldManager;
        this.textureManager = args.TextureManager;

        this.Id = args.Id;
        this.ContentPack = args.ContentPack;
        this.Target = args.ContentModel.Target;
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
    public Rectangle SourceArea { get; }

    /// <inheritdoc />
    public List<DrawMethod> DrawMethods { get; }

    /// <inheritdoc />
    public PatchMode PatchMode { get; }

    /// <inheritdoc />
    public IRawTextureData? Texture { get; set; }

    /// <inheritdoc />
    public Rectangle Area { get; set; }

    /// <inheritdoc />
    public Color? Tint { get; set; }

    /// <inheritdoc />
    public float Scale { get; set; }

    /// <inheritdoc />
    public int Frames { get; set; }

    /// <inheritdoc />
    public int TicksPerFrame { get; set; }

    /// <inheritdoc />
    public Vector2 Offset { get; set; }

    /// <summary>Gets a helper that provides useful methods for performing common operations.</summary>
    protected IPatchHelper Helper { get; }

    /// <inheritdoc />
    public string GetCurrentId()
    {
        var sb = new StringBuilder();
        sb.Append(Path.Join(this.Id, this.path));
        sb.Append('_');
        sb.Append(this.Area.ToString());

        if (this.Tint != null)
        {
            sb.Append('_');
            sb.Append(this.Tint.ToString());
        }

        if ((int)this.Scale != 1)
        {
            sb.Append('_');
            sb.Append(((int)this.Scale).ToString(CultureInfo.InvariantCulture));
        }

        sb.Append('_');
        sb.Append(this.PatchMode.ToString());
        return sb.ToString();
    }

    /// <inheritdoc />
    public abstract bool Run(IManagedObject managedObject);

    /// <summary>Resets the Texture, Area, and Tint properties of the object before running.</summary>
    /// <param name="managedObject">The managed object requesting the patch.</param>
    protected void BeforeRun(IManagedObject managedObject)
    {
        this.currentObject = managedObject;
        this.Texture = null;
        this.Area = Rectangle.Empty;
        this.Tint = null;
        this.Scale = 1f;
        this.Frames = 0;
        this.TicksPerFrame = 0;
        this.Offset = Vector2.Zero;
    }

    /// <summary>Validate the Texture, Area, and Tint properties of the object after running.</summary>
    /// <param name="managedObject">The managed object requesting the patch.</param>
    /// <returns><c>true</c> if the patch should be applied; otherwise, <c>false</c>.</returns>
    protected bool AfterRun(IManagedObject managedObject)
    {
        this.currentObject = null;
        return this.Texture != null;
    }
}