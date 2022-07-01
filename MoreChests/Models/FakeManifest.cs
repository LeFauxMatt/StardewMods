#nullable disable

namespace MoreChests.Models;

using System.Collections.Generic;
using StardewModdingAPI;

internal class FakeManifest : IManifest
{
    public FakeManifest(IManifest manifest)
    {
        this.Name = manifest.Name;
        this.Description = manifest.Description;
        this.Author = manifest.Author;
        this.Version = manifest.Version;
        this.MinimumApiVersion = manifest.MinimumApiVersion;
        this.UniqueID = manifest.UniqueID;
        this.EntryDll = manifest.EntryDll;
        this.ContentPackFor = manifest.ContentPackFor;
        this.Dependencies = manifest.Dependencies;
        this.UpdateKeys = manifest.UpdateKeys;
        this.ExtraFields = manifest.ExtraFields;
    }
    public string Name { get; }
    public string Description { get; }
    public string Author { get; }
    public ISemanticVersion Version { get; }
    public ISemanticVersion MinimumApiVersion { get; }
    public string UniqueID { get; }
    public string EntryDll { get; }
    public IManifestContentPackFor ContentPackFor { get; set; }
    public IManifestDependency[] Dependencies { get; }
    public string[] UpdateKeys { get; }
    public IDictionary<string, object> ExtraFields { get; set; }
}