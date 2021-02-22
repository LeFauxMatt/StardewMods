using System;
using System.Collections.Generic;
using System.Linq;
using ImJustMatt.ExpandedStorage.API;
using ImJustMatt.ExpandedStorage.Framework.Extensions;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace ImJustMatt.ExpandedStorage.Framework.Models
{
    public class StorageTab : IStorageTab
    {
        /// <summary>The UniqueId of the Content Pack that storage data was loaded from.</summary>
        protected internal string ModUniqueId;
        internal Texture2D Texture => _texture ??= LoadTexture();
        private Texture2D _texture;
        public string TabName { get; set; }
        public string TabImage { get; set; }
        public IList<string> AllowList { get; set; } = new List<string>();
        public IList<string> BlockList { get; set; } = new List<string>();
        public Func<Texture2D> LoadTexture { get; set; }

        private bool IsAllowed(Item item)
        {
            return AllowList == null || !AllowList.Any() || AllowList.Any(item.MatchesTagExt);
        }

        private bool IsBlocked(Item item)
        {
            return BlockList != null && BlockList.Any() && BlockList.Any(item.MatchesTagExt);
        }

        internal bool Filter(Item item)
        {
            return IsAllowed(item) && !IsBlocked(item);
        }

        internal StorageTab()
        {
        }

        internal StorageTab(string tabImage, params string[] allowList)
        {
            TabImage = tabImage;
            AllowList = allowList.ToList();
        }
        
        internal static StorageTab Clone(IStorageTab storageTab)
        {
            var newTab = new StorageTab();
            newTab.CopyFrom(storageTab);
            return newTab;
        }

        internal void CopyFrom(IStorageTab storageTab)
        {
            TabName = storageTab.TabName;
            TabImage = storageTab.TabImage;
            AllowList = storageTab.AllowList;
            BlockList = storageTab.BlockList;
            LoadTexture = storageTab.LoadTexture;
        }
    }
}