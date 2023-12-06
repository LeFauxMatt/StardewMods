namespace StardewMods.TeaTime;

/// <summary>Model used for custom tea saplings.</summary>
internal sealed class TeaModel
{
    /// <summary>Gets or sets the age needed to produce.</summary>
    public int AgeToProduce { get; set; } = 20;

    /// <summary>Gets or sets the day of month to begin producing.</summary>
    public int DayToBeginProducing { get; set; } = 22;

    /// <summary>Gets or sets the item that is dropped by this mod.</summary>
    public string ItemProduced { get; set; } = string.Empty;

    /// <summary>Gets or sets the texture of the tea bush.</summary>
    public string Texture { get; set; } = string.Empty;
}
