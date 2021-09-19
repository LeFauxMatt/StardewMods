namespace XSLite
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using Microsoft.Xna.Framework.Graphics;
    using StardewModdingAPI;

    public static class MigrationHelper
    {
        public static bool CreateDynamicAsset(IContentPack contentPack)
        {
            // Create localization Directory
            string path = Path.Combine(contentPack.DirectoryPath, "BigCraftables");
            if (!Directory.Exists(path))
            {
                return false;
            }

            Directory.CreateDirectory(Path.Combine(contentPack.DirectoryPath, "i18n"));

            // Generate content.json and default.json
            var content = new List<Dictionary<string, object>>();
            var localizations = new Dictionary<string, Dictionary<string, string>>();
            foreach (string folder in Directory.GetDirectories(path))
            {
                var folderInfo = new DirectoryInfo(folder);
                string folderName = folderInfo.Name;
                JsonAsset jsonAsset = contentPack.ReadJsonFile<JsonAsset>($"BigCraftables/{folderName}/big-craftable.json");
                if (string.IsNullOrWhiteSpace(jsonAsset.Name) || !XSLite.Storages.TryGetValue(jsonAsset.Name, out Storage storage))
                {
                    continue;
                }

                // Add texture for legacy formatted packs
                string texturePath = Path.Combine("BigCraftables", folderName, "big-craftable.png");
                if (string.IsNullOrWhiteSpace(storage.Image) && contentPack.HasFile(texturePath))
                {
                    storage.Image = texturePath;
                    storage.Format = Storage.AssetFormat.JsonAssets;
                    Texture2D texture = contentPack.LoadAsset<Texture2D>(texturePath);
                    XSLite.Textures.Add(jsonAsset.Name, texture);
                    for (int frame = 1; frame <= jsonAsset.ReserveExtraIndexCount; frame++)
                    {
                        texturePath = $"BigCraftables/{folderName}/big-craftable-{frame.ToString()}.png";
                        if (!contentPack.HasFile(texturePath))
                        {
                            break;
                        }

                        texture = contentPack.LoadAsset<Texture2D>(texturePath);
                        XSLite.Textures.Add($"{jsonAsset.Name}-{(frame - 1).ToString()}", texture);
                    }
                }

                // Add to default localization
                if (!localizations.TryGetValue("default", out Dictionary<string, string> localization))
                {
                    localization = new Dictionary<string, string>();
                    localizations.Add("default", localization);
                }

                localization.Add($"big-craftable.{jsonAsset.Name}.name", jsonAsset.Name);
                localization.Add($"big-craftable.{jsonAsset.Name}.description", jsonAsset.Description);
                localization.Add($"crafting.{jsonAsset.Name} recipe.name", jsonAsset.Name);
                localization.Add($"crafting.{jsonAsset.Name} recipe.description", jsonAsset.Description);

                // Add additional localizations
                if (jsonAsset.NameLocalization != null)
                {
                    foreach (KeyValuePair<string, string> localizedText in jsonAsset.NameLocalization)
                    {
                        if (!localizations.TryGetValue(localizedText.Key, out localization))
                        {
                            localization = new Dictionary<string, string>();
                            localizations.Add(localizedText.Key, localization);
                        }

                        localization.Add($"big-craftable.{jsonAsset.Name}.name", localizedText.Value);
                        localization.Add($"big-craftable.{jsonAsset.Name} recipe.name", localizedText.Value);
                    }
                }

                if (jsonAsset.DescriptionLocalization != null)
                {
                    foreach (KeyValuePair<string, string> localizedText in jsonAsset.DescriptionLocalization)
                    {
                        if (!localizations.TryGetValue(localizedText.Key, out localization))
                        {
                            localization = new Dictionary<string, string>();
                            localizations.Add(localizedText.Key, localization);
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
                    { "Texture", $"BigCraftables/{folderName}/big-craftable.png:0" },
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
                        },
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
                                    _ => jsonAsset.Recipe.PurchaseFrom,
                                }
                            },
                            { "Item", new DGAItem("DGARecipe", $"{contentPack.Manifest.UniqueID}/{jsonAsset.Name} recipe") },
                            { "MaxSold", 1 },
                            { "Cost", jsonAsset.Recipe.PurchasePrice },
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
                                _ => jsonAsset.PurchaseFrom,
                            }
                        },
                        { "Item", new DGAItem("DGAItem", $"{contentPack.Manifest.UniqueID}/{jsonAsset.Name}") },
                        { "MaxSold", int.MaxValue },
                        { "Cost", jsonAsset.PurchasePrice },
                    });
                }
            }

            contentPack.WriteJsonFile("content.json", content);
            foreach (KeyValuePair<string, Dictionary<string, string>> localization in localizations)
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
        }

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
            public DGAItem(string type, object value, int quantity = 1)
            {
                this.Type = type;
                this.Value = value;
                this.Quantity = quantity;
            }

            public string Type { get; set; }

            public object Value { get; set; }

            public int Quantity { get; set; }
        }
    }
}