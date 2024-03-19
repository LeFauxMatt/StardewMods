namespace StardewMods.Common.Services.Integrations.GenericModConfigMenu;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/// <summary>Represents a complex menu option for Generic Mod Config Menu.</summary>
public interface IComplexOption
{
    /// <summary>Gets the height of the menu option.</summary>
    public int Height { get; }

    /// <summary>Gets the name of the menu option.</summary>
    public string Name { get; }

    /// <summary>Gets the tooltip of the menu option.</summary>
    public string Tooltip { get; }

    /// <summary>Draws the menu option.</summary>
    /// <param name="spriteBatch">The sprite batch to draw to.</param>
    /// <param name="pos">The position to draw at.</param>
    public void Draw(SpriteBatch spriteBatch, Vector2 pos);

    /// <summary>Executes a set of actions before the menu is opened.</summary>
    public void BeforeMenuOpened();

    /// <summary>Executes a set of actions before the menu is closed.</summary>
    public void BeforeMenuClosed();

    /// <summary>Executes a set of actions before the option is saved.</summary>
    public void BeforeSave();

    /// <summary>Executes a set of actions after the option is saved.</summary>
    public void AfterSave();

    /// <summary>Executes a set of actions before the option is reset.</summary>
    public void BeforeReset();

    /// <summary>Executes a set of actions after the option is set.</summary>
    public void AfterReset();
}