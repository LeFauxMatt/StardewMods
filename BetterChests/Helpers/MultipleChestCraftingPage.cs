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
    public MultipleChestCraftingPage(IEnumerable<KeyValuePair<IGameObjectType, IManagedStorage>> storages)
    {
        this.TimeOut = 60;
        this.Storages = (
                from storage in storages
                where storage.Key is LocationObject
                      && storage.Value.Context is Chest
                select storage)
            .ToDictionary(
                storage => storage.Key,
                storage => storage.Value);
        this.Chests.AddRange(from storage in this.Storages.Values select (Chest)storage.Context);
        foreach (var chest in this.Chests)
        {
            chest.mutex.RequestLock();
        }

        this.Update();
    }

    private List<Chest> Chests { get; } = new();

    private List<Chest> LockedChests { get; } = new();

    private Dictionary<IGameObjectType, IManagedStorage> Storages { get; }

    private int TimeOut { get; set; }

    /// <summary>
    ///     Cancels current mutex requests and closes the menu.
    /// </summary>
    public void ExitFunction()
    {
        this.TimeOut = 0;
        foreach (var chest in this.Chests.Where(chest => chest.mutex.IsLockHeld()))
        {
            chest.mutex.ReleaseLock();
        }

        this.Chests.Clear();
        this.LockedChests.Clear();
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

            this.OpenCraftingPage();
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Updates the mutexes for chests related to this request.
    /// </summary>
    public void Update()
    {
        if (--this.TimeOut <= 0)
        {
            return;
        }

        foreach (var chest in this.Chests.Where(chest => !this.LockedChests.Contains(chest)))
        {
            if (chest.mutex.IsLockHeld())
            {
                this.LockedChests.Add(chest);
                continue;
            }

            chest.mutex.Update(Game1.getOnlineFarmers());
        }

        if (this.Chests.Count == this.LockedChests.Count)
        {
            this.OpenCraftingPage();
        }
    }

    private void OpenCraftingPage()
    {
        this.TimeOut = 0;
        var width = 800 + IClickableMenu.borderWidth * 2;
        var height = 600 + IClickableMenu.borderWidth * 2;
        var (x, y) = Utility.getTopLeftPositionForCenteringOnScreen(width, height);
        Game1.activeClickableMenu = new CraftingPage((int)x, (int)y, width, height, false, true, this.LockedChests)
        {
            exitFunction = this.ExitFunction,
        };
    }
}