namespace MoreChests.Services;

using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Objects;
using SObject = StardewValley.Object;

internal class InventoryHandler : BaseService
{
    private CustomChestManager _customChestManager;

    internal InventoryHandler(ServiceLocator serviceLocator)
        : base("InventoryHandler")
    {
        // Dependencies
        this.AddDependency<CustomChestManager>(service => this._customChestManager = service as CustomChestManager);

        // Events
        serviceLocator.Helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        serviceLocator.Helper.Events.Player.InventoryChanged += this.OnInventoryChanged;
    }

    private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
    {
        this.ConvertStorages();
    }

    private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
    {
        this.ConvertStorages();
    }

    private void ConvertStorages()
    {
        for (var i = 0; i < Game1.player.Items.Count; i++)
        {
            var item = Game1.player.Items[i];
            if (item is not Chest && item is SObject {bigCraftable.Value: true} && this._customChestManager.TryCreate(item, out var chest))
            {
                Game1.player.Items[i] = chest;
            }
        }
    }
}