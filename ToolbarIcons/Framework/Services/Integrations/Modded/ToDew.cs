namespace StardewMods.ToolbarIcons.Framework.Services.Integrations.Modded;

using System.Reflection;
using StardewMods.ToolbarIcons.Framework.Interfaces;
using StardewValley.Menus;

/// <inheritdoc />
internal sealed class ToDew : IActionIntegration
{
    /// <inheritdoc/>
    public string ModId => "jltaylor-us.ToDew";

    /// <inheritdoc/>
    public int Index => 7;

    /// <inheritdoc/>
    public string HoverText => I18n.Button_ToDew();

    /// <inheritdoc />
    public Action? GetAction(IMod mod)
    {
        var modType = mod.GetType();
        var perScreenList = modType.GetField("list", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(mod);
        var toDoMenu = modType.Assembly.GetType("ToDew.ToDoMenu");
        if (perScreenList is null || toDoMenu is null)
        {
            return null;
        }

        return () =>
        {
            var value = perScreenList.GetType().GetProperty("Value")?.GetValue(perScreenList);
            if (value is null)
            {
                return;
            }

            var action = toDoMenu.GetConstructor([modType, value.GetType()]);
            if (action is null)
            {
                return;
            }

            var menu = action.Invoke([mod, value]);
            Game1.activeClickableMenu = (IClickableMenu)menu;
        };
    }
}