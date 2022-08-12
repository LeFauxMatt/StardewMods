namespace StardewMods.BetterChests.Storages;

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Integrations.BetterChests;
using StardewValley.Network;
using StardewValley.Objects;

/// <inheritdoc cref="StardewMods.BetterChests.Storages.BaseStorage" />
internal class ChestStorage : BaseStorage, IColorable
{
    private readonly Chest _chest;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ChestStorage" /> class.
    /// </summary>
    /// <param name="chest">The source chest.</param>
    /// <param name="source">The context where the source object is contained.</param>
    /// <param name="position">The position of the source object.</param>
    public ChestStorage(Chest chest, object? source, Vector2 position)
        : base(
            chest is { SpecialChestType: Chest.SpecialChestTypes.JunimoChest } ? Game1.player.team : chest,
            source,
            position)
    {
        this.Chest = chest;
        this._chest = new(true, this.Chest.ParentSheetIndex)
        {
            Name = this.Chest.Name,
            playerChoiceColor = { Value = this.Chest.playerChoiceColor.Value },
        };

        foreach (var (key, value) in this.Chest.modData.Pairs)
        {
            this._chest.modData[key] = value;
        }

        this._chest.resetLidFrame();
    }

    /// <summary>
    ///     Gets the source chest object.
    /// </summary>
    public Chest Chest { get; }

    /// <inheritdoc />
    public Color Color
    {
        get => this.Chest.playerChoiceColor.Value;
        set
        {
            this._chest.playerChoiceColor.Value = value;
            this.Chest.playerChoiceColor.Value = value;
        }
    }

    /// <inheritdoc />
    public override IList<Item?> Items => this.Chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID);

    /// <inheritdoc />
    public override ModDataDictionary ModData => this.Chest.modData;

    /// <inheritdoc />
    public override NetMutex? Mutex => this.Chest.GetMutex();

    /// <inheritdoc />
    public void Draw(SpriteBatch spriteBatch, int x, int y)
    {
        this._chest.draw(spriteBatch, x, y, 1f, true);
    }

    /// <inheritdoc />
    public override void ShowMenu()
    {
        this.Chest.ShowMenu();
    }
}