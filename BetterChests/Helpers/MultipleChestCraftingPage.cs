namespace StardewMods.BetterChests.Helpers;

using System.Collections.Generic;
using System.Linq;
using Common.Helpers;
using StardewMods.BetterChests.Interfaces.ManagedObjects;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Models.GameObjects;
using StardewValley;
using StardewValley.Menus;
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
        this.Chests.AddRange(
            from storage in this.Storages
            where storage.Key is LocationObject
                  && storage.Value.Context is Chest
            select (Chest)storage.Value.Context);
        this.MultipleMutexRequest = new(
            this.Chests.Select(chest => chest.mutex).ToList(),
            this.SuccessCallback,
            this.FailureCallback);
    }

    private List<Chest> Chests { get; } = new();

    private MultipleMutexRequest MultipleMutexRequest { get; }

    private List<KeyValuePair<IGameObjectType, IManagedStorage>> Storages { get; }

    private int TimeOut { get; set; }

    /// <summary>
    /// Cancels current mutex requests and closes the menu.
    /// </summary>
    public void ExitFunction()
    {
        this.TimeOut = 0;
        this.MultipleMutexRequest?.ReleaseLocks();
    }

    /// <summary>
    ///     Check if the request has timed out.
    /// </summary>
    /// <returns>Returns a value indicating whether the request has timed out.</returns>
    public bool TimedOut()
    {
        if (this.TimeOut <= 0)
        {
            foreach (var (gameObjectType, managedStorage) in this.Storages)
            {
                if (managedStorage.Context is Chest chest && !chest.mutex.IsLockHeld())
                {
                    switch (gameObjectType)
                    {
                        case InventoryItem(var farmer, var i):
                            Log.Info($"Could not acquire lock for storage \"{managedStorage.QualifiedItemId}\" with farmer {farmer.Name} at slot {i.ToString()}.");
                            break;
                        case LocationObject(var gameLocation, var (x, y)):
                            Log.Info($"Could not acquire lock for storage \"{managedStorage.QualifiedItemId}\" at location {gameLocation.NameOrUniqueName} at coordinates ({((int)x).ToString()},{((int)y).ToString()}).");
                            break;
                    }
                }
            }

            this.ExitFunction();
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Updates the mutexes for chests related to this request.
    /// </summary>
    public void UpdateChests()
    {
        if (--this.TimeOut <= 0)
        {
            return;
        }

        foreach (var chest in this.Chests)
        {
            chest.mutex.Update(Game1.getOnlineFarmers());
        }
    }

    private void FailureCallback()
    {
        Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Workbench_Chest_Warning"));
        this.ExitFunction();
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