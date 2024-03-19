namespace StardewMods.SpritePatcher.Framework.Services.Operations;

using Microsoft.Xna.Framework;
using StardewMods.SpritePatcher.Framework.Interfaces;

internal sealed class BasePatchHandler : IPatchHandler
{
    public void ApplyTexture(IRawTextureData inputTextureData, IRawTextureData outputTextureData)
    {
        for (var x = 0; x < inputTextureData.Width; x++)
        {
            for (var y = 0; y < inputTextureData.Height; y++)
            {
                var targetIndex = y * inputTextureData.Width + x;
                var targetColor = inputTextureData.Data[targetIndex];
                var newColor = targetColor;
                foreach (var operation in _operations)
                {
                    newColor = operation(targetIndex, newColor);
                }
                outputTextureData.Data[targetIndex] = newColor;
            }
        }
    }

    public IRawTextureData RegisterOperation(IRawTextureData inputTextureData)
    {
        for (var x = 0; x < inputTextureData.Width; x++)
        {
            for (var y = 0; y < inputTextureData.Height; y++)
            {
                var targetIndex = 
            }
        }
    }

    public Color RegisterOperation(Func<int, Color, Color> operation)
    {
        
    }
}