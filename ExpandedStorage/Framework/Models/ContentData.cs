using System.Collections.Generic;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace ExpandedStorage.Framework.Models
{
    internal class ContentData
    {
        public IList<Storage> ExpandedStorage { get; set; }
        public IList<StorageTab> StorageTabs { get; set; }
    }
}