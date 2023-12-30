namespace StardewMods.HelpfulSpouses.Framework.Services.Chores;

using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.HelpfulSpouses.Framework.Enums;
using StardewMods.HelpfulSpouses.Framework.Interfaces;
using StardewValley.Extensions;

/// <inheritdoc cref="StardewMods.HelpfulSpouses.Framework.Interfaces.IChore" />
internal sealed class MakeBreakfast : BaseChore<MakeBreakfast>
{
    private static readonly Lazy<List<Item>> Items = new(
        delegate
        {
            return ItemRegistry
                .GetObjectTypeDefinition()
                .GetAllIds()
                .Select(localId => ItemRegistry.type_object + localId)
                .Where(id => ItemContextTagManager.HasBaseTag(id, "food_breakfast"))
                .Select(id => ItemRegistry.Create(id))
                .ToList();
        });

    private Item? breakfast;

    /// <summary>Initializes a new instance of the <see cref="MakeBreakfast" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    public MakeBreakfast(ILog log, IManifest manifest, IModConfig modConfig)
        : base(log, manifest, modConfig) { }

    /// <inheritdoc />
    public override ChoreOption Option => ChoreOption.MakeBreakfast;

    /// <inheritdoc />
    public override void AddTokens(Dictionary<string, object> tokens)
    {
        if (this.breakfast is null)
        {
            return;
        }

        tokens["ItemName"] = this.breakfast.DisplayName;
        tokens["ItemId"] = $"[{this.breakfast.QualifiedItemId}]";
    }

    /// <inheritdoc />
    public override bool IsPossibleForSpouse(NPC spouse) => true;

    /// <inheritdoc />
    public override bool TryPerformChore(NPC spouse)
    {
        this.breakfast = Game1.random.ChooseFrom(MakeBreakfast.Items.Value);
        return true;
    }
}