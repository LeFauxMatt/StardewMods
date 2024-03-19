namespace StardewMods.SpritePatcher.Framework.Interfaces;

using Microsoft.Xna.Framework;

public interface IPatchHandler
{
    Color RegisterOperation(Func<int, Color, Color> operation);
}