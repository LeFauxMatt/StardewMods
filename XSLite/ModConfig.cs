namespace XSLite
{
    using System.Collections.Generic;

    internal class ModConfig
    {
        public int Capacity { get; set; } = 0;

        public HashSet<string> EnabledFeatures { get; set; } = new();
    }
}