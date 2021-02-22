using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace ImJustMatt.ExpandedStorage.API
{
    public interface IStorageTab
    {
        /// <summary>Loads Texture to draw for tab.</summary>
        Func<Texture2D> LoadTexture { get; set; }

        /// <summary>Display Name for tab.</summary>
        string TabName { get; set; }

        /// <summary>Image to display for tab, will search asset folder first and default next.</summary>
        string TabImage { get; set; }

        /// <summary>When specified, tab will only show the listed item/category IDs.</summary>
        IList<string> AllowList { get; set; }

        /// <summary>When specified, tab will show all/allowed items except for listed item/category IDs.</summary>
        IList<string> BlockList { get; set; }
    }
}