namespace StardewMods.BetterChests.Framework.Services;

using System.Globalization;
using StardewMods.BetterChests.Framework.StorageObjects;

/// <summary>Responsible for adding or removing custom buffs.</summary>
internal sealed class BuffHandler
{
    private const string Overburdened = "furyx639.BetterChests/Overburdened";

    private readonly IMonitor monitor;
    private readonly ModConfig config;

    /// <summary>
    /// Initializes a new instance of the <see cref="BuffHandler"/> class.
    /// </summary>
    /// <param name="monitor">Dependency used for monitoring and logging.</param>
    /// <param name="config">Dependency used for accessing config data.</param>
    public BuffHandler(IMonitor monitor, ModConfig config)
    {
        this.monitor = monitor;
        this.config = config;
    }

    /// <summary>Checks if the player should be overburdened while carrying a chest.</summary>
    /// <param name="excludeCurrent">Whether to exclude the current item.</param>
    public void CheckForOverburdened(bool excludeCurrent = false)
    {
        if (this.config.CarryChestSlowAmount == 0)
        {
            this.RemoveBuff(BuffHandler.Overburdened);
            return;
        }

        foreach (var storage in StorageHandler.Inventory)
        {
            if (storage is not
                {
                    Data: Storage storageObject,
                }
                || (excludeCurrent && storageObject.Context == Game1.player.CurrentItem)
                || !storageObject.Inventory.HasAny())
            {
                continue;
            }

            this.AddBuff(BuffHandler.Overburdened);
            return;
        }

        this.RemoveBuff(BuffHandler.Overburdened);
    }

    /// <summary>Adds a custom buff to the player.</summary>
    /// <param name="buffId">The buff id to add.</param>
    private void AddBuff(string buffId)
    {
        var buff = buffId switch
        {
            BuffHandler.Overburdened => new Buff(
                BuffHandler.Overburdened,
                displayName: I18n.Effect_CarryChestSlow_Description(
                    this.config.CarryChestSlowAmount.ToString(CultureInfo.InvariantCulture)),
                duration: int.MaxValue / 700,
                iconTexture: Game1.buffsIcons,
                iconSheetIndex: 13,
                effects: new() { Speed = { -this.config.CarryChestSlowAmount } }),
            _ => default,
        };

        if (buff is not null)
        {
            Game1.player.buffs.Apply(buff);
        }
    }

    /// <summary>Removes a custom buff from the player.</summary>
    /// <param name="buffId">The buff id to remove.</param>
    private void RemoveBuff(string buffId) => Game1.player.buffs.Remove(buffId);
}
