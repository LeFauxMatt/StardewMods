using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ExpandedStorage.Framework.Extensions;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace ExpandedStorage.Framework.Models
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "NotAccessedField.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    internal class TabContentData
    {
        /// <summary>Tab Name must match the name from Json Assets.</summary>
        public string TabName;
        
        /// <summary>Image to display for tab, will search asset folder first and default next.</summary>
        public string TabImage;
        
        /// <summary>When specified, tab will only show the listed item/category IDs.</summary>
        public IList<string> AllowList = new List<string>();

        /// <summary>When specified, tab will show all/allowed items except for listed item/category IDs.</summary>
        public IList<string> BlockList = new List<string>();
        
        /// <summary>Texture to draw for tab.</summary>
        internal Texture2D Texture;
        
        /// <summary>The UniqueId of the Content Pack that storage data was loaded from.</summary>
        internal string ModUniqueId;
        
        private bool IsAllowed(Item item) => !AllowList.Any() || AllowList.Any(item.MatchesTagExt);
        private bool IsBlocked(Item item) => BlockList.Any() && BlockList.Any(item.MatchesTagExt);
        internal bool Filter(Item item) => IsAllowed(item) && !IsBlocked(item);
    }
}