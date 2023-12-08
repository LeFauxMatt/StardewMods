namespace StardewMods.ToolbarIcons.Framework.Integrations.Mods;

using StardewValley.Menus;

/// <inheritdoc />
internal sealed class CjbItemSpawner : ICustomIntegration
{
    private const string ModId = "CJBok.ItemSpawner";

    private readonly ComplexIntegration complex;
    private readonly IReflectionHelper reflection;

    /// <summary>Initializes a new instance of the <see cref="CjbItemSpawner" /> class.</summary>
    /// <param name="complex">Dependency for adding a complex mod integration.</param>
    /// <param name="reflection">Dependency used for accessing inaccessible code.</param>
    public CjbItemSpawner(ComplexIntegration complex, IReflectionHelper reflection)
    {
        this.complex = complex;
        this.reflection = reflection;
    }

    /// <inheritdoc />
    public void AddIntegration() =>
        this.complex.AddCustomAction(CjbItemSpawner.ModId, 5, I18n.Button_ItemSpawner(), this.GetAction);

    private Action? GetAction(IMod mod)
    {
        var buildMenu = this.reflection.GetMethod(mod, "BuildMenu", false);
        return () => { Game1.activeClickableMenu = buildMenu.Invoke<ItemGrabMenu>(); };
    }
}
