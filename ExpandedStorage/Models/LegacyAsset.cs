namespace StardewMods.ExpandedStorage.Models;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
///     Represents assets loaded from a legacy Expanded Storage content pack.
/// </summary>
internal sealed class LegacyAsset
{
    private readonly IContentPack contentPack;
    private readonly string id;
    private readonly string path;

    private Tuple<string, string>? craftingRecipe;

    public LegacyAsset(string id, IContentPack contentPack, string path)
    {
        this.id = id;
        this.contentPack = contentPack;
        this.path = path;
    }

    /// <summary>
    ///     Gets the crafting recipe entry.
    /// </summary>
    public Tuple<string, string> CraftingRecipe
    {
        get
        {
            if (this.craftingRecipe is not null)
            {
                return this.craftingRecipe;
            }

            // Get Recipe in DGA Format
            if (!this.contentPack.HasFile("content.json"))
            {
                return this.craftingRecipe = new(string.Empty, string.Empty);
            }

            var content = this.contentPack.ModContent.Load<List<LegacyRecipe>>("content.json");
            foreach (var item in content)
            {
                if (item.Ingredients is null
                    || item.Result?.Value is not string strValue
                    || !strValue.EndsWith(this.id))
                {
                    continue;
                }

                var sb = new StringBuilder();
                var space = false;
                foreach (var ingredient in item.Ingredients)
                {
                    if (ingredient.Type != "VanillaObject" || ingredient.Value is not IFormattable intValue)
                    {
                        continue;
                    }

                    if (space)
                    {
                        sb.Append(' ');
                    }

                    sb.Append(intValue);
                    sb.Append(' ');
                    sb.Append(ingredient.Quantity.ToString(CultureInfo.InvariantCulture));
                    space = true;
                }

                sb.Append("/Home/232/true/null/");
                sb.Append(this.contentPack.Translation.Get($"big-craftable.{this.id}.name"));
                return this.craftingRecipe = new(item.Id, sb.ToString());
            }

            return this.craftingRecipe = new(string.Empty, string.Empty);
        }
    }

    /// <summary>
    ///     Gets the texture from the content pack's mod content.
    /// </summary>
    public Texture2D Texture => this.contentPack.ModContent.Load<Texture2D>(this.path);
}