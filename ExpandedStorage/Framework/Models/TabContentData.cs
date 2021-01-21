using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace ExpandedStorage.Framework.Models
{
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    [SuppressMessage("ReSharper", "NotAccessedField.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    internal class TabContentData : TabConfig
    {
        /// <summary>The UniqueId of the Content Pack that storage data was loaded from.</summary>
        internal string ModUniqueId;
        
        /// <summary>Texture to draw for tab.</summary>
        internal Texture2D Texture;
        
        internal bool IsAllowed(Item item) => !AllowList.Any() || AllowList.Any(item.HasContextTag);
        internal bool IsBlocked(Item item) => BlockList.Any() && BlockList.Any(item.HasContextTag);
        internal void CopyFrom(TabConfig config)
        {
            AllowList = config.AllowList;
            BlockList = config.BlockList;
            TabImage = config.TabImage;
        }
    }
}