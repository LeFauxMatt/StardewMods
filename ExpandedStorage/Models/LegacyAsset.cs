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
    private readonly IContentPack _contentPack;
    private readonly string _id;
    private readonly string _path;

    private Tuple<string, string>? _craftingRecipe;

    public LegacyAsset(string id, IContentPack contentPack, string path)
    {
        this._id = id;
        this._contentPack = contentPack;
        this._path = path;
    }

    /// <summary>
    ///     Gets the crafting recipe entry.
    /// </summary>
    public Tuple<string, string> CraftingRecipe
    {
        get
        {
            if (this._craftingRecipe is not null)
            {
                return this._craftingRecipe;
            }

            // Get Recipe in DGA Format
            if (!this._contentPack.HasFile("content.json"))
            {
                return this._craftingRecipe = new(string.Empty, string.Empty);
            }

            var content = this._contentPack.ModContent.Load<List<LegacyRecipe>>("content.json");
            foreach (var item in content)
            {
                if (item.Ingredients is null
                    || item.Result?.Value is not string strValue
                    || !strValue.EndsWith(this._id))
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
                sb.Append(this._contentPack.Translation.Get($"big-craftable.{this._id}.name"));
                return this._craftingRecipe = new(item.ID, sb.ToString());
            }

            return this._craftingRecipe = new(string.Empty, string.Empty);
        }
    }

    /// <summary>
    ///     Gets the texture from the content pack's mod content.
    /// </summary>
    public Texture2D Texture => this._contentPack.ModContent.Load<Texture2D>(this._path);
}