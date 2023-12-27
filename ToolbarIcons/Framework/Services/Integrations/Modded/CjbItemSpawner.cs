namespace StardewMods.ToolbarIcons.Framework.Services.Integrations.Modded;

using StardewMods.ToolbarIcons.Framework.Interfaces;
using StardewValley.Menus;

/// <inheritdoc />
internal sealed class CjbItemSpawner : ICustomIntegration
{
    private const string ModId = "CJBok.ItemSpawner";

    private readonly ComplexIntegration complexIntegration;
    private readonly IReflectionHelper reflectionHelper;

    /// <summary>Initializes a new instance of the <see cref="CjbItemSpawner" /> class.</summary>
    /// <param name="complexIntegration">Dependency for adding a complex mod integration.</param>
    /// <param name="reflectionHelper">Dependency used for accessing inaccessible code.</param>
    public CjbItemSpawner(ComplexIntegration complexIntegration, IReflectionHelper reflectionHelper)
    {
        this.complexIntegration = complexIntegration;
        this.reflectionHelper = reflectionHelper;
    }

    /// <inheritdoc />
    public void AddIntegration() =>
        this.complexIntegration.AddCustomAction(CjbItemSpawner.ModId, 5, I18n.Button_ItemSpawner(), this.GetAction);

    private Action? GetAction(IMod mod)
    {
        var buildMenu = this.reflectionHelper.GetMethod(mod, "BuildMenu", false);
        return () => { Game1.activeClickableMenu = buildMenu.Invoke<ItemGrabMenu>(); };
    }
}