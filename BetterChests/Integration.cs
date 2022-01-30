namespace Mod.BetterChests;

using FuryCore.Interfaces;
using Mod.BetterChests.Interfaces;
using Mod.BetterChests.Services;
using StardewModdingAPI;

/// <inheritdoc />
internal class Integration : IModIntegration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Integration"/> class.
    /// </summary>
    /// <param name="services">Provides access to internal and external services.</param>
    public Integration(IModServices services)
    {
        this.Services = services;
    }

    private IModServices Services { get; }

    private IModConfigMenu ModConfigMenu
    {
        get => this.Services.FindService<ModConfigMenu>();
    }

    /// <inheritdoc/>
    public void ChestConfig(IManifest manifest, IChestData config)
    {
        this.ModConfigMenu.ChestConfig(manifest, config);
    }
}