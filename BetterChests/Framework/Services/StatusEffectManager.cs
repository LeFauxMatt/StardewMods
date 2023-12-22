namespace StardewMods.BetterChests.Framework.Services;

using StardewMods.BetterChests.Framework.Enums;
using StardewMods.Common.Interfaces;
using StardewValley.Buffs;

/// <summary>Responsible for adding or removing custom buffs.</summary>
internal sealed class StatusEffectManager : BaseService
{
    /// <summary>Initializes a new instance of the <see cref="StatusEffectManager" /> class.</summary>
    /// <param name="logging">Dependency used for monitoring and logging.</param>
    public StatusEffectManager(ILogging logging)
        : base(logging) { }

    /// <summary>Adds a custom status effect to the player.</summary>
    /// <param name="statusEffect">The status effect to add.</param>
    public void AddEffect(StatusEffect statusEffect)
    {
        var buff = this.GetEffect(statusEffect);
        if (buff is null)
        {
            return;
        }

        this.Logging.Trace("Adding effect {0}", statusEffect.ToStringFast());
        Game1.player.buffs.Apply(buff);
    }

    /// <summary>Removes a custom status effect from the player.</summary>
    /// <param name="statusEffect">The status effect to remove.</param>
    public void RemoveEffect(StatusEffect statusEffect)
    {
        var id = this.GetId(statusEffect);
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        this.Logging.Trace("Removing effect {0}", statusEffect.ToStringFast());
        Game1.player.buffs.Remove(id);
    }

    private string GetId(StatusEffect statusEffect) => statusEffect switch { StatusEffect.Overburdened => this.Prefix + StatusEffect.Overburdened.ToStringFast(), _ => string.Empty };

    private Buff? GetEffect(StatusEffect statusEffect) =>
        statusEffect switch
        {
            StatusEffect.Overburdened => new Buff(
                this.Prefix + StatusEffect.Overburdened.ToStringFast(),
                displayName: I18n.Effect_CarryChestSlow_Description(-1),
                duration: 5_000,
                iconTexture: Game1.buffsIcons,
                iconSheetIndex: 13,
                effects: new BuffEffects { Speed = { -1 } }),
            _ => null,
        };
}
