using System.Collections.Generic;

// ReSharper disable ClassNeverInstantiated.Global
namespace ExpandedStorage.Framework.Models
{
    internal class ContentData
    {
        public IList<ExpandedStorageConfig> ExpandedStorage = new List<ExpandedStorageConfig>();
        public IList<ExpandedStorageTab> StorageTabs = new List<ExpandedStorageTab>();
    }
}