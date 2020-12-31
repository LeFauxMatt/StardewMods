namespace ExpandedStorage.Framework.Models
{
    public class ExpandedStorageData
    {
        public string StorageName { get; set; }
        public int Capacity { get; set; } = 36;
        internal int ParentSheetIndex { get; set; }
    }
}