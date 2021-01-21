using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ExpandedStorage.Framework.Models
{
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    internal class TabConfig
    {
        /// <summary>Tab Name must match the name from Json Assets.</summary>
        public string TabName;
        
        /// <summary>When specified, tab will only show the listed item/category IDs.</summary>
        public IList<string> AllowList;

        /// <summary>When specified, tab will show all/allowed items except for listed item/category IDs.</summary>
        public IList<string> BlockList;

        /// <summary>Image to display for tab, will search asset folder first and default next.</summary>
        public string TabImage;

        internal TabConfig()
            : this(null, new List<string>(), new List<string>(), null) { }

        internal TabConfig(TabConfig config)
            : this(config.TabName, config.AllowList, config.BlockList, config.TabImage) {}
        internal TabConfig(string tabName, IList<string> allowList, IList<string> blockList, string tabImage)
        {
            TabName = tabName;
            AllowList = allowList;
            BlockList = blockList;
            TabImage = tabImage;
        }
    }
}