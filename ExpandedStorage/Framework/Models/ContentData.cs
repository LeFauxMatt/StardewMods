using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable ClassNeverInstantiated.Global
namespace ExpandedStorage.Framework.Models
{
    [SuppressMessage("ReSharper", "CollectionNeverUpdated.Global")]
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    internal class ContentData
    {
        public IList<StorageContentData> ExpandedStorage = new List<StorageContentData>();
        public IList<TabContentData> StorageTabs = new List<TabContentData>();
    }
}