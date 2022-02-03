namespace XSLite;

using System.Collections.Generic;

internal class ModConfig
{
    public int Capacity { get; set; }

    public HashSet<string> EnabledFeatures { get; set; } = new();
}