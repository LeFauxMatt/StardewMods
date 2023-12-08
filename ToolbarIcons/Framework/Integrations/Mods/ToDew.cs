namespace StardewMods.ToolbarIcons.Framework.Integrations.Mods;

using System.Reflection;
using StardewValley.Menus;

/// <inheritdoc />
internal sealed class ToDew : ICustomIntegration
{
    private const string ModId = "jltaylor-us.ToDew";
    private const string ModType = "ToDew.ToDoMenu";

    private readonly ComplexIntegration complex;

    /// <summary>Initializes a new instance of the <see cref="ToDew" /> class.</summary>
    /// <param name="complex">Dependency for adding a complex mod integration.</param>
    public ToDew(ComplexIntegration complex) => this.complex = complex;

    /// <inheritdoc />
    public void AddIntegration() => this.complex.AddCustomAction(ToDew.ModId, 7, I18n.Button_ToDew(), ToDew.GetAction);

    private static Action? GetAction(IMod mod)
    {
        var modType = mod.GetType();
        var perScreenList = modType.GetField("list", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(mod);
        var toDoMenu = modType.Assembly.GetType(ToDew.ModType);
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

            var action = toDoMenu.GetConstructor(new[] { modType, value.GetType() });
            if (action is null)
            {
                return;
            }

            var menu = action.Invoke(new[] { mod, value });
            Game1.activeClickableMenu = (IClickableMenu)menu;
        };
    }
}
