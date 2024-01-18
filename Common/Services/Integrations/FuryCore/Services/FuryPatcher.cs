namespace StardewMods.Common.Services.Integrations.FuryCore;

/// <inheritdoc />
internal sealed class FuryPatcher(FuryCoreIntegration furyCore, ILog log) : IPatchManager
{
    private readonly IPatchManager patchManager = furyCore.Api!.CreatePatchService(log);

    /// <inheritdoc />
    public void Add(string id, params ISavedPatch[] patches) => this.patchManager.Add(id, patches);

    /// <inheritdoc />
    public void Patch(string id) => this.patchManager.Patch(id);

    /// <inheritdoc />
    public void Unpatch(string id) => this.patchManager.Unpatch(id);
}