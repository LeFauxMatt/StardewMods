namespace StardewMods.SpritePatcher.Framework.Services.TextureMangers;

using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models.Events;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.SpritePatcher.Framework.Enums;
using StardewMods.SpritePatcher.Framework.Enums.Patches;
using StardewMods.SpritePatcher.Framework.Interfaces;
using StardewMods.SpritePatcher.Framework.Models;
using StardewMods.SpritePatcher.Framework.Services.Factory;

/// <summary>Base class for texture patches.</summary>
internal abstract class BaseTextureManager : BaseService, ITextureManager
{
    private static readonly CodeInstruction MenuDrawMethod = new(OpCodes.Ldc_I4_0);
    private static readonly CodeInstruction HeldDrawMethod = new(OpCodes.Ldc_I4_1);
    private static readonly CodeInstruction WorldDrawMethod = new(OpCodes.Ldc_I4_2);
    private static readonly CodeInstruction BackgroundDrawMethod = new(OpCodes.Ldc_I4_3);
    private static readonly CodeInstruction ConstructionDrawMethod = new(OpCodes.Ldc_I4_4);
    private static readonly CodeInstruction ShadowDrawMethod = new(OpCodes.Ldc_I4_5);

    private static readonly CodeInstruction CallDraw = CodeInstruction.Call(
        typeof(BaseTextureManager),
        nameof(BaseTextureManager.DrawCustom));

    private static readonly MethodInfo SpriteBatchDraw = AccessTools.DeclaredMethod(
        typeof(SpriteBatch),
        nameof(SpriteBatch.Draw),
        [
            typeof(Texture2D),
            typeof(Vector2),
            typeof(Rectangle),
            typeof(Color),
            typeof(float),
            typeof(Vector2),
            typeof(float),
            typeof(SpriteEffects),
            typeof(float),
        ]);

#nullable disable
    private static BaseTextureManager instance;
#nullable enable

    private readonly ConfigManager config;
    private readonly ManagedObjectFactory managedObjectFactory;

    /// <summary>Initializes a new instance of the <see cref="BaseTextureManager" /> class.</summary>
    /// <param name="configManager">Dependency used for managing config data.</param>
    /// <param name="eventSubscriber">Dependency used for subscribing to events.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="managedObjectFactory">Dependency used for getting managed objects.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="patchManager">Dependency used for managing patches.</param>
    protected BaseTextureManager(
        ConfigManager configManager,
        IEventSubscriber eventSubscriber,
        ILog log,
        ManagedObjectFactory managedObjectFactory,
        IManifest manifest,
        IPatchManager patchManager)
        : base(log, manifest)
    {
        BaseTextureManager.instance = this;
        this.config = configManager;
        this.managedObjectFactory = managedObjectFactory;
        this.Patches = patchManager;
        eventSubscriber.Subscribe<ConfigChangedEventArgs<DefaultConfig>>(this.OnConfigChanged);
    }

    /// <inheritdoc />
    public string Id => this.ModId + "." + this.Type.ToStringFast();

    /// <inheritdoc />
    public abstract AllPatches Type { get; }

    /// <summary>Gets the dependency used for managing patches.</summary>
    protected IPatchManager Patches { get; }

    /// <summary>Transpiles the given set of code instructions by replacing calls to a specific draw method.</summary>
    /// <param name="instructions">The original set of code instructions.</param>
    /// <returns>A new set of code instructions with the replaced draw method calls.</returns>
    protected static IEnumerable<CodeInstruction> Draw(IEnumerable<CodeInstruction> instructions) =>
        BaseTextureManager.DrawTranspiler(instructions, BaseTextureManager.WorldDrawMethod);

    /// <summary>Transpiles the given set of code instructions by replacing calls to a specific draw method.</summary>
    /// <param name="instructions">The original set of code instructions.</param>
    /// <returns>A new set of code instructions with the replaced draw method calls.</returns>
    protected static IEnumerable<CodeInstruction> DrawBackground(IEnumerable<CodeInstruction> instructions) =>
        BaseTextureManager.DrawTranspiler(instructions, BaseTextureManager.BackgroundDrawMethod);

    /// <summary>Transpiles the given set of code instructions by replacing calls to a specific draw method.</summary>
    /// <param name="instructions">The original set of code instructions.</param>
    /// <returns>A new set of code instructions with the replaced draw method calls.</returns>
    protected static IEnumerable<CodeInstruction> DrawInConstruction(IEnumerable<CodeInstruction> instructions) =>
        BaseTextureManager.DrawTranspiler(instructions, BaseTextureManager.ConstructionDrawMethod);

    /// <summary>Transpiles the given set of code instructions by replacing calls to a specific draw method.</summary>
    /// <param name="instructions">The original set of code instructions.</param>
    /// <returns>A new set of code instructions with the replaced draw method calls.</returns>
    protected static IEnumerable<CodeInstruction> DrawInMenu(IEnumerable<CodeInstruction> instructions) =>
        BaseTextureManager.DrawTranspiler(instructions, BaseTextureManager.MenuDrawMethod);

    /// <summary>Transpiles the given set of code instructions by replacing calls to a specific draw method.</summary>
    /// <param name="instructions">The original set of code instructions.</param>
    /// <returns>A new set of code instructions with the replaced draw method calls.</returns>
    protected static IEnumerable<CodeInstruction> DrawShadow(IEnumerable<CodeInstruction> instructions) =>
        BaseTextureManager.DrawTranspiler(instructions, BaseTextureManager.ShadowDrawMethod);

    /// <summary>Transpiles the given set of code instructions by replacing calls to a specific draw method.</summary>
    /// <param name="instructions">The original set of code instructions.</param>
    /// <returns>A new set of code instructions with the replaced draw method calls.</returns>
    protected static IEnumerable<CodeInstruction> DrawWhenHeld(IEnumerable<CodeInstruction> instructions) =>
        BaseTextureManager.DrawTranspiler(instructions, BaseTextureManager.HeldDrawMethod);

    private static IEnumerable<CodeInstruction> DrawTranspiler(
        IEnumerable<CodeInstruction> instructions,
        CodeInstruction drawMethod)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.Calls(BaseTextureManager.SpriteBatchDraw))
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return drawMethod;
                yield return BaseTextureManager.CallDraw;
            }
            else
            {
                yield return instruction;
            }
        }
    }

    private static void DrawCustom(
        SpriteBatch spriteBatch,
        Texture2D texture,
        Vector2 position,
        Rectangle? sourceRectangle,
        Color color,
        float rotation,
        Vector2 origin,
        float scale,
        SpriteEffects effects,
        float layerDepth,
        IHaveModData entity,
        DrawMethod drawMethod)
    {
        var managedObject = BaseTextureManager.instance.managedObjectFactory.GetOrAdd(entity);
        managedObject.Draw(
            spriteBatch,
            texture,
            position,
            sourceRectangle,
            color,
            rotation,
            origin,
            scale,
            effects,
            layerDepth,
            drawMethod);
    }

    private void OnConfigChanged(ConfigChangedEventArgs<DefaultConfig> obj)
    {
        if (this.config.GetValue(this.Type))
        {
            this.Patches.Patch(this.Id);
        }
        else
        {
            this.Patches.Unpatch(this.Id);
        }
    }
}