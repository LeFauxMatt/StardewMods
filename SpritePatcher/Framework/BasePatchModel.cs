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
    private string path = string.Empty;

    /// <summary>Initializes a new instance of the <see cref="BasePatchModel" /> class.</summary>
    /// <param name="args">The patch model arguments.</param>
    protected BasePatchModel(PatchModelCtorArgs args)
    {
        this.monitor = args.Monitor;
        this.Id = args.Id;
        this.ContentPack = args.ContentPack;
        this.Target = args.ContentModel.Target;
        this.SourceArea = args.ContentModel.Area;
        this.DrawMethods = args.ContentModel.DrawMethods;
        this.PatchMode = args.ContentModel.PatchMode;
        this.NetFields = args.ContentModel.NetFields ?? [];
        this.Helper = new PatchHelper(this);
    }

    /// <inheritdoc />
    public string Id { get; }

    /// <inheritdoc />
    public IContentPack ContentPack { get; }

    /// <inheritdoc />
    public string Target { get; }

    /// <inheritdoc />
    public Rectangle? SourceArea { get; }

    /// <inheritdoc />
    public List<DrawMethod> DrawMethods { get; }

    /// <inheritdoc />
    public List<string> NetFields { get; }

    /// <inheritdoc />
    public PatchMode PatchMode { get; }

    /// <inheritdoc />
    public IRawTextureData? Texture { get; protected set; }

    /// <inheritdoc />
    public Rectangle? Area { get; protected set; }

    /// <inheritdoc />
    public Color? Tint { get; protected set; }

    /// <summary>Gets a helper that provides useful methods for performing common operations.</summary>
    protected IPatchHelper Helper { get; }

    /// <inheritdoc />
    public string GetCurrentId()
    {
        var sb = new StringBuilder();
        sb.Append(Path.Join(this.Id, this.path));
        if (this.Area != null)
        {
            sb.Append('_');
            sb.Append(this.Area.ToString());
        }

        if (this.Tint != null)
        {
            sb.Append('_');
            sb.Append(this.Tint.ToString());
        }

        sb.Append('_');
        sb.Append(this.PatchMode.ToString());
        return sb.ToString();
    }

    /// <inheritdoc />
    public abstract bool Run(IManagedObject managedObject);
}