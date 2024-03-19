namespace StardewMods.Common.Services.Integrations.FauxCore;

/// <summary>Manages Harmony patches.</summary>
public interface IPatchManager
{
    /// <summary>Adds a patch to the specified id.</summary>
    /// <param name="id">The id to associate the patch with.</param>
    /// <param name="patches">The patch object to add.</param>
    public void Add(string id, params ISavedPatch[] patches);

    /// <summary>Applies the specified patches.</summary>
    /// <param name="id">The id of saved patches.</param>
    public void Patch(string id);

    /// <summary>Removes the specified patches.</summary>
    /// <param name="id">The id of the saved patches.</param>
    public void Unpatch(string id);
}