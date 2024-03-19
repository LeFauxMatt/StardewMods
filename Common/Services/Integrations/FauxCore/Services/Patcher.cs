namespace StardewMods.Common.Services.Integrations.FauxCore;

/// <inheritdoc />
internal sealed class Patcher(FauxCoreIntegration fauxCore, ILog log) : IPatchManager
{
    private readonly IPatchManager patchManager = fauxCore.Api!.CreatePatchService(log);

    /// <inheritdoc />
    public void Add(string id, params ISavedPatch[] patches) => this.patchManager.Add(id, patches);

    /// <inheritdoc />
    public void Patch(string id) => this.patchManager.Patch(id);

    /// <inheritdoc />
    public void Unpatch(string id) => this.patchManager.Unpatch(id);
}