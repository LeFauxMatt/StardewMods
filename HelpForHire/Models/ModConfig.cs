#nullable disable

namespace HelpForHire.Models;

internal class ModConfig
{
    /// <summary>Gets or sets the cost of a birthday gift which is loved by the recipient.</summary>
    public ChoreConfig GiftLove { get; set; } = new(0);

    /// <summary>Gets or sets the cost of a birthday gift which is liked by the recipient.</summary>
    public ChoreConfig GiftLike { get; set; } = new(0);

    /// <summary>Gets or sets the cost to feed all animals.</summary>
    public ChoreConfig FeedAnimals { get; set; } = new(0);

    /// <summary>Gets or sets the cost to feed pets.</summary>
    public ChoreConfig FeedPet { get; set; } = new(0);

    /// <summary>Gets or sets the cost to pet all animals.</summary>
    public ChoreConfig PetAnimals { get; set; } = new(0);

    /// <summary>Gets or sets the cost to pet all animals.</summary>
    public ChoreConfig RepairFences { get; set; } = new(0);

    /// <summary>Gets or sets the cost to pet all animals.</summary>
    public ChoreConfig WaterCrops { get; set; } = new(0);

    /// <summary>Gets or sets the cost to pet all animals.</summary>
    public ChoreConfig WaterSlimes { get; set; } = new(0);
}