namespace XSLite.Migrations.JsonAsset;

using System.Collections.Generic;

internal record Recipe
{
    public int ResultCount { get; set; }

    public List<Ingredient> Ingredients { get; set; }

    public bool CanPurchase { get; set; } = false;

    public bool IsDefault { get; set; } = true;

    public string PurchaseFrom { get; set; } = null;

    public int PurchasePrice { get; set; } = 0;

    public string SkillUnlockName { get; set; } = null;

    public int SkillUnlockLevel { get; set; } = 0;
}