namespace HelpForHire;

using Chores;
using Common.Helpers;
using StardewModdingAPI;
using StardewModdingAPI.Events;

public class ModEntry : Mod
{
    internal ServiceLocator ServiceLocator { get; private set; }

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // Init
        Log.Init(this.Monitor);
        this.ServiceLocator = new(this.Helper, this.ModManifest);
        this.ServiceLocator.Create(
            new[]
            {
                typeof(FeedAnimals),
                typeof(FeedPet),
                typeof(PetAnimals),
                typeof(RepairFences),
                typeof(WaterCrops),
                typeof(WaterSlimes),
            });

        // Events
        this.Helper.Events.GameLoop.DayStarted += this.OnDayStarted;
    }

    private void OnDayStarted(object sender, DayStartedEventArgs e)
    {
        foreach (var chore in this.ServiceLocator.GetAll<GenericChore>().Where(chore => chore.IsActive && chore.IsPossible))
        {
            chore.PerformChore();
        }
    }
}