namespace StardewMods.HelpfulSpouses;

using System.Globalization;
using System.Text;

/// <summary>
///     Mod config data.
/// </summary>
public sealed class ModConfig
{
    /// <summary>
    ///     Gets or sets the chance that Birthday Shopping will be performed.
    /// </summary>
    public float BirthdayShopping { get; set; } = 0.8f;

    /// <summary>
    ///     Gets or sets the chance that the Birthday Gift will be liked by the NPC.
    /// </summary>
    public float BirthdayShoppingLikedItemChance { get; set; } = 1.0f;

    /// <summary>
    ///     Gets or sets the chance that the Birthday Gift will be loved by the NPC.
    /// </summary>
    public float BirthdayShoppingLovedItemChance { get; set; } = 0.1f;

    /// <summary>
    ///     Gets or sets the chance that Cooking a Meal will be performed.
    /// </summary>
    public float CookAMeal { get; set; } = 0.2f;

    /// <summary>
    ///     Gets or sets the chance that Feeding the Animals will be performed.
    /// </summary>
    public float FeedTheAnimals { get; set; } = 0.5f;

    /// <summary>
    ///     Gets or sets the chance that Loving the Pets will be performed.
    /// </summary>
    public float LoveThePets { get; set; } = 0.2f;

    /// <summary>
    ///     Gets or sets the chance that Petting the Animals will be performed.
    /// </summary>
    public float PetTheAnimals { get; set; } = 0.2f;

    /// <summary>
    ///     Gets or sets the chance that Repairing the Fences will be performed.
    /// </summary>
    public float RepairTheFences { get; set; } = 0.5f;

    /// <summary>
    ///     Gets or sets the chance that Watering the Crops will be performed.
    /// </summary>
    public float WaterTheCrops { get; set; } = 0.5f;

    /// <summary>
    ///     Gets or sets the chance that Watering the Slimes will be performed.
    /// </summary>
    public float WaterTheSlimes { get; set; } = 0.2f;

    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"BirthdayShopping: {this.BirthdayShopping.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine(
            $"BirthdayShoppingLikedItemChance: {this.BirthdayShoppingLikedItemChance.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine(
            $"BirthdayShoppingLovedItemChance: {this.BirthdayShoppingLovedItemChance.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"CookAMeal: {this.CookAMeal.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"FeedTheAnimals: {this.FeedTheAnimals.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"LoveThePets: {this.LoveThePets.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"PetTheAnimals: {this.PetTheAnimals.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"RepairTheFences: {this.RepairTheFences.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"WaterTheCrops: {this.WaterTheCrops.ToString(CultureInfo.InvariantCulture)}");
        sb.AppendLine($"WaterTheSlimes: {this.WaterTheSlimes.ToString(CultureInfo.InvariantCulture)}");
        return sb.ToString();
    }
}