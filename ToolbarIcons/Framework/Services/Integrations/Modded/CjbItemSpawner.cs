namespace StardewMods.ToolbarIcons.Framework.Services.Integrations.Modded;

using StardewMods.ToolbarIcons.Framework.Interfaces;
using StardewValley.Menus;

/// <inheritdoc />
internal sealed class CjbItemSpawner : IActionIntegration
{
    private readonly IReflectionHelper reflectionHelper;

    /// <summary>Initializes a new instance of the <see cref="CjbItemSpawner" /> class.</summary>
    /// <param name="reflectionHelper">Dependency used for accessing inaccessible code.</param>
    public CjbItemSpawner(IReflectionHelper reflectionHelper) => this.reflectionHelper = reflectionHelper;

    /// <inheritdoc />
    public string ModId => "CJBok.ItemSpawner";

    /// <inheritdoc />
    public int Index => 5;

    /// <inheritdoc />
    public string HoverText => I18n.Button_ItemSpawner();

    /// <inheritdoc />
    public Action? GetAction(IMod mod)
    {
        var buildMenu = this.reflectionHelper.GetMethod(mod, "BuildMenu", false);
        return () => { Game1.activeClickableMenu = buildMenu.Invoke<ItemGrabMenu>(); };
    }
}