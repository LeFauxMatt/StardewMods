using System;
using System.Collections.Generic;
using ExpandedStorage.Framework.Models;
using StardewModdingAPI;

namespace ExpandedStorage
{
    public interface IExpandedStorageAPI
    {
        /// <summary>Load a directory as an Expanded Storage content pack.</summary>
        /// <param name="path">Path containing expandedStorage.json file.</param>
        /// <returns>True if content was loaded successfully.</returns>
        bool LoadContentPack(string path);

        /// <summary>Load an Expanded Storage content pack.</summary>
        /// <param name="contentPack">The content pack to load.</param>
        /// <returns>True if content was loaded successfully.</returns>
        bool LoadContentPack(IContentPack contentPack);

        /// <summary>Event shortly after GameLaunch when content packs can be loaded.</summary>
        event EventHandler ReadyToLoad;
    }
}