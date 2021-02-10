using System.Collections.Generic;

namespace ExpandedStorage.Framework.Models
{
    internal class ContentData
    {
        public IList<Storage> ExpandedStorage { get; set; }
        public IList<StorageTab> StorageTabs { get; set; }
    }
}