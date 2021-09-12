using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace XSLite
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
    public static class MigrationHelper
    {
        public static bool CreateDynamicAsset(IContentPack contentPack)
        {
            // Create localization Directory
            var path = Path.Combine(contentPack.DirectoryPath, "BigCraftables");
            if (!Directory.Exists(path))
                return false;
            Directory.CreateDirectory(Path.Combine(contentPack.DirectoryPath, "i18n"));
            // Generate content.json and default.json
            var content = new List<Dictionary<string, object>>();
            var i18n = new Dictionary<string, Dictionary<string, string>>();
            foreach (var folder in Directory.GetDirectories(path))
            {
                var folderInfo = new DirectoryInfo(folder);
                var folderName = folderInfo.Name;
                var jsonAsset = contentPack.ReadJsonFile<JsonAsset>($"BigCraftables/{folderName}/big-craftable.json");
                if (string.IsNullOrWhiteSpace(jsonAsset.Name) || !XSLite.Storages.TryGetValue(jsonAsset.Name, out var storage))
                    continue;
                // Add texture for legacy formatted packs
                var texturePath = $"BigCraftables/{folderName}/big-craftable.png";
                if (string.IsNullOrWhiteSpace(storage.Image) && contentPack.HasFile(texturePath))
                {
                    storage.Image = texturePath;
                    storage.Format = Storage.AssetFormat.JsonAssets;
                    var texture = contentPack.LoadAsset<Texture2D>(texturePath);
                    XSLite.Textures.Add(jsonAsset.Name, texture);
                    for (var frame = 1; frame <= jsonAsset.ReserveExtraIndexCount; frame++)
                    {
                        texturePath = $"BigCraftables/{folderName}/big-craftable-{frame}.png";
                        if (!contentPack.HasFile(texturePath))
                            break;
                        texture = contentPack.LoadAsset<Texture2D>(texturePath);
                        XSLite.Textures.Add($"{jsonAsset.Name}-{frame-1}", texture);
                    }
                }
                // Add to default localization
                if (!i18n.TryGetValue("default", out var localization))
                {
                    localization = new Dictionary<string, string>();
                    i18n.Add("default", localization);
                }
                localization.Add($"big-craftable.{jsonAsset.Name}.name", jsonAsset.Name);
                localization.Add($"big-craftable.{jsonAsset.Name}.description", jsonAsset.Description);
                localization.Add($"crafting.{jsonAsset.Name} recipe.name", jsonAsset.Name);
                localization.Add($"crafting.{jsonAsset.Name} recipe.description", jsonAsset.Description);
                // Add additional localizations
                if (jsonAsset.NameLocalization != null)
                {
                    foreach (var localizedText in jsonAsset.NameLocalization)
                    {
                        if (!i18n.TryGetValue(localizedText.Key, out localization))
                        {
                            localization = new Dictionary<string, string>();
                            i18n.Add(localizedText.Key, localization);
                        }
                        localization.Add($"big-craftable.{jsonAsset.Name}.name", localizedText.Value);
                        localization.Add($"big-craftable.{jsonAsset.Name} recipe.name", localizedText.Value);
                    }
                }
                if (jsonAsset.DescriptionLocalization != null)
                {
                    foreach (var localizedText in jsonAsset.DescriptionLocalization)
                    {
                        if (!i18n.TryGetValue(localizedText.Key, out localization))
                        {
                            localization = new Dictionary<string, string>();
                            i18n.Add(localizedText.Key, localization);
                        }
                        localization.Add($"crafting.{jsonAsset.Name}.description", localizedText.Value);
                        localization.Add($"crafting.{jsonAsset.Name} recipe.description", localizedText.Value);
                    }
                }
                // Add BigCraftable
                content.Add(new Dictionary<string, object>
                {
                    { "$ItemType", "BigCraftable" },
                    { "ID", jsonAsset.Name },
                    { "JsonAssetsName", jsonAsset.Name },
                    { "Texture", $"BigCraftables/{folderName}/big-craftable.png:0" }
                });
                // Add CraftingRecipe
                if (jsonAsset.Recipe is not null)
                {
                    var recipe = new Dictionary<string, object>
                    {
                        { "$ItemType", "CraftingRecipe" },
                        { "ID", $"{jsonAsset.Name} recipe" },
                        { "IsCooking", false },
                        { "KnownByDefault", jsonAsset.Recipe.IsDefault },
                        {
                            "Ingredients", jsonAsset.Recipe.Ingredients.Select(ingredient => new DGAItem("VanillaObject", ingredient.Object, ingredient.Count)).ToList()
                        },
                        {
                            "Result", new DGAItem("DGAItem", $"{contentPack.Manifest.UniqueID}/{jsonAsset.Name}")
                        }
                    };
                    if (!string.IsNullOrWhiteSpace(jsonAsset.Recipe.SkillUnlockName) && jsonAsset.Recipe.SkillUnlockLevel > 0)
                    {
                        recipe.Add("SkillUnlockName", jsonAsset.Recipe.SkillUnlockName);
                        recipe.Add("SkillUnlockLevel", jsonAsset.Recipe.SkillUnlockLevel);
                    }
                    content.Add(recipe);
                    if (!string.IsNullOrWhiteSpace(jsonAsset.Recipe.PurchaseFrom) && jsonAsset.Recipe.PurchasePrice > 0)
                    {
                        content.Add(new Dictionary<string, object>
                        {
                            { "$ItemType", "ShopEntry" },
                            {
                                "ShopId", jsonAsset.Recipe.PurchaseFrom switch
                                {
                                    "Clint" => "Blacksmith",
                                    "Marnie" => "AnimalSupplies",
                                    "Robin" => "Carpenter",
                                    "Marlon" => "AdventurerGuild",
                                    "Gus" => "Saloon",
                                    "Pierre" => "SeedShop",
                                    "Willy" => "FishShop",
                                    "Harvey" => "Hospital",
                                    "Maru" => "Hospital",
                                    _ => jsonAsset.Recipe.PurchaseFrom
                                }
                            },
                            { "Item", new DGAItem("DGARecipe", $"{contentPack.Manifest.UniqueID}/{jsonAsset.Name} recipe") },
                            { "MaxSold", 1 },
                            { "Cost", jsonAsset.Recipe.PurchasePrice }
                        });
                    }
                }
                // Add ShopEntry
                if (jsonAsset.PurchaseFrom is not null)
                {
                    content.Add(new Dictionary<string, object>
                    {
                        { "$ItemType", "ShopEntry" },
                        {
                            "ShopId", jsonAsset.PurchaseFrom switch
                            {
                                "Clint" => "Blacksmith",
                                "Marnie" => "AnimalSupplies",
                                "Robin" => "Carpenter",
                                "Marlon" => "AdventurerGuild",
                                "Gus" => "Saloon",
                                "Pierre" => "SeedShop",
                                "Willy" => "FishShop",
                                "Harvey" => "Hospital",
                                "Maru" => "Hospital",
                                _ => jsonAsset.PurchaseFrom
                            }
                        },
                        { "Item", new DGAItem("DGAItem", $"{contentPack.Manifest.UniqueID}/{jsonAsset.Name}") },
                        { "MaxSold", int.MaxValue },
                        { "Cost", jsonAsset.PurchasePrice }
                    });
                }
            }
            contentPack.WriteJsonFile("content.json", content);
            foreach (var localization in i18n)
            {
                contentPack.WriteJsonFile($"i18n/{localization.Key}.json", localization.Value);
            }
            return true;
        }

        private record JsonAsset
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public int ReserveExtraIndexCount { get; set; } = 0;
            public JARecipe Recipe { get; set; } = null;
            public string PurchaseFrom { get; set; } = null;
            public int PurchasePrice { get; set; } = 0;
            public Dictionary<string, string> NameLocalization { get; set; } = null;
            public Dictionary<string, string> DescriptionLocalization { get; set; } = null;
        };
        private record JARecipe
        {
            public int ResultCount { get; set; }
            public List<JAIngredient> Ingredients { get; set; }
            public bool CanPurchase { get; set; } = false;
            public bool IsDefault { get; set; } = true;
            public string PurchaseFrom { get; set; } = null;
            public int PurchasePrice { get; set; } = 0;
            public string SkillUnlockName { get; set; } = null;
            public int SkillUnlockLevel { get; set; } = 0;
        }
        private record JAIngredient
        {
            public int Object { get; set; }
            public int Count { get; set; } = 1;
        }
        private record DGAItem
        {
            public string Type { get; set; }
            public object Value { get; set; }
            public int Quantity { get; set; }
            public DGAItem(string type, object value, int quantity = 1)
            {
                Type = type;
                Value = value;
                Quantity = quantity;
            }
        }
    }
}