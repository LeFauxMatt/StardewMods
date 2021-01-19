using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

// ReSharper disable ClassNeverInstantiated.Global
namespace ExpandedStorage.Framework.Models
{
    internal class ExpandedStorageTab
    {
        /// <summary>Tab Name must match the name from Json Assets.</summary>
        public string TabName;
        
        /// <summary>When specified, tab will only show the listed item/category IDs.</summary>
        public IList<string> AllowList = new List<string>();

        /// <summary>When specified, tab will show all/allowed items except for listed item/category IDs.</summary>
        public IList<string> BlockList = new List<string>();

        /// <summary>Image to display for tab, will search asset folder first and default next.</summary>
        public string TabImage;

        /// <summary>The UniqueId of the Content Pack that storage data was loaded from.</summary>
        internal string ModUniqueId;
        
        /// <summary>Texture to draw for tab.</summary>
        internal Texture2D Texture;
        
        public bool IsAllowed(Item item) => !AllowList.Any() || AllowList.Any(item.HasContextTag);
        public bool IsBlocked(Item item) => BlockList.Any() && BlockList.Any(item.HasContextTag);
    }
}