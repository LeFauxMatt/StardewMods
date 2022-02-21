namespace StardewMods.BetterChests.Helpers;

using System.Collections.Generic;
using System.Linq;
using StardewMods.BetterChests.Interfaces.ManagedObjects;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Models.GameObjects;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;

/// <summary>
///     Represents an attempt to open a crafting page for multiple chests.
/// </summary>
internal class MultipleChestCraftingPage
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MultipleChestCraftingPage" /> class.
    /// </summary>
    /// <param name="storages">To storages to open a crafting page for.</param>
    public MultipleChestCraftingPage(List<KeyValuePair<IGameObjectType, IManagedStorage>> storages)
    {
        this.TimeOut = 60;
        this.Storages = storages;
        this.Mutexes = (
            from storage in this.Storages
            where storage.Key is LocationObject
                  && storage.Value.Context is Chest
            select ((Chest)storage.Value.Context).mutex).ToList();
        this.MultipleMutexRequest = new(
            this.Mutexes,
            this.SuccessCallback,
            this.FailureCallback);
    }

    /// <summary>
    ///     Gets a value indicating whether the request has timed out.
    /// </summary>
    public bool TimedOut
    {
        get => this.TimeOut <= 0;
    }

    private MultipleMutexRequest MultipleMutexRequest { get; }

    private List<NetMutex> Mutexes { get; }

    private List<KeyValuePair<IGameObjectType, IManagedStorage>> Storages { get; }

    private int TimeOut { get; set; }

    /// <summary>
    ///     Updates the mutexes for chests related to this request.
    /// </summary>
    public void UpdateChests()
    {
        if (--this.TimeOut <= 0)
        {
            return;
        }

        foreach (var mutex in this.Mutexes)
        {
            mutex.Update(Game1.getOnlineFarmers());
        }
    }

    private void ExitFunction()
    {
        this.MultipleMutexRequest.ReleaseLocks();
    }

    private void FailureCallback()
    {
        Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Workbench_Chest_Warning"));
        this.TimeOut = 0;
    }

    private void SuccessCallback()
    {
        this.TimeOut = 0;
        var width = 800 + IClickableMenu.borderWidth * 2;
        var height = 600 + IClickableMenu.borderWidth * 2;
        var (x, y) = Utility.getTopLeftPositionForCenteringOnScreen(width, height);
        var chests = this.Storages.Select(storage => storage.Value.Context).OfType<Chest>().ToList();
        Game1.activeClickableMenu = new CraftingPage((int)x, (int)y, width, height, false, true, chests)
        {
            exitFunction = this.ExitFunction,
        };
    }
}