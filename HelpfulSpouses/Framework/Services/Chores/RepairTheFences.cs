namespace StardewMods.HelpfulSpouses.Framework.Services.Chores;

using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.HelpfulSpouses.Framework.Enums;
using StardewMods.HelpfulSpouses.Framework.Interfaces;

/// <inheritdoc cref="StardewMods.HelpfulSpouses.Framework.Interfaces.IChore" />
internal sealed class RepairTheFences : BaseChore<RepairTheFences>
{
    private int fencesRepaired;

    /// <summary>Initializes a new instance of the <see cref="RepairTheFences" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    public RepairTheFences(ILog log, IManifest manifest, IModConfig modConfig)
        : base(log, manifest, modConfig) { }

    /// <inheritdoc />
    public override ChoreOption Option => ChoreOption.RepairTheFences;

    /// <inheritdoc />
    public override void AddTokens(Dictionary<string, object> tokens) => tokens["FencesRepaired"] = this.fencesRepaired;

    /// <inheritdoc />
    public override bool IsPossibleForSpouse(NPC spouse) =>
        Game1
            .getFarm()
            .Objects.Values.Any(@object => @object is Fence fence && fence.getHealth() < fence.maxHealth.Value);

    /// <inheritdoc />
    public override bool TryPerformChore(NPC spouse)
    {
        this.fencesRepaired = 0;

        var fences = Game1.getFarm().Objects.Values.OfType<Fence>();
        foreach (var fence in fences)
        {
            fence.repairQueued.Value = true;
            this.fencesRepaired++;
            if (this.Config.RepairTheFences.FenceLimit > 0
                && this.fencesRepaired >= this.Config.RepairTheFences.FenceLimit)
            {
                return true;
            }
        }

        return this.fencesRepaired > 0;
    }
}