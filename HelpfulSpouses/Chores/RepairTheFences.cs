namespace StardewMods.HelpfulSpouses.Chores;

/// <inheritdoc />
internal sealed class RepairTheFences : IChore
{
    private readonly Config config;

    private int fencesRepaired;

    /// <summary>
    /// Initializes a new instance of the <see cref="RepairTheFences"/> class.
    /// </summary>
    /// <param name="config">Config data for <see cref="RepairTheFences"/>.</param>
    public RepairTheFences(Config config)
    {
        this.config = config;
    }

    /// <inheritdoc/>
    public void AddTokens(Dictionary<string, object> tokens)
    {
        tokens["FencesRepaired"] = this.fencesRepaired;
    }

    /// <inheritdoc/>
    public bool IsPossibleForSpouse(NPC spouse)
    {
        return Game1.getFarm().Objects.Values
            .Any(@object => @object is Fence fence && fence.getHealth() < fence.maxHealth.Value);
    }

    /// <inheritdoc/>
    public bool TryPerformChore(NPC spouse)
    {
        this.fencesRepaired = 0;

        var fences = Game1.getFarm().Objects.Values.OfType<Fence>();
        foreach (var fence in fences)
        {
            fence.repairQueued.Value = true;
            this.fencesRepaired++;
            if (this.config.FenceLimit > 0 && this.fencesRepaired >= this.config.FenceLimit)
            {
                return true;
            }
        }

        return this.fencesRepaired > 0;
    }

    /// <summary>
    /// Config data for <see cref="RepairTheFences" />.
    /// </summary>
    public sealed class Config
    {
        /// <summary>
        /// Gets or sets the limit to the number of fences that will be repaired.
        /// </summary>
        public int FenceLimit { get; set; } = 0;
    }
}