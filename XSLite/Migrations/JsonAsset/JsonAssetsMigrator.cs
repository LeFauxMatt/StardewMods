namespace XSLite.Migrations.JsonAsset
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.Xna.Framework.Graphics;
    using StardewModdingAPI;

    /// <summary>
    ///     Migrate XS content packs created from JA to DGA
    /// </summary>
    internal class JsonAssetsMigrator
    {
        /// <summary>
        ///     Gets a list of JsonAsset objects.
        /// </summary>
        public IList<JsonAsset> JsonAssets { get; } = new List<JsonAsset>();

        /// <summary>
        ///     Gets textures for each JsonAsset object.
        /// </summary>
        public IList<TextureMigrator> Textures { get; } = new List<TextureMigrator>();

        public static JsonAssetsMigrator FromContentPack(IContentPack contentPack)
        {
            var path = Path.Combine(contentPack.DirectoryPath, "BigCraftables");
            if (!Directory.Exists(path))
            {
                return null;
            }

            // Generate content.json and default.json
            var jsonAssets = new JsonAssetsMigrator();
            foreach (var folder in Directory.GetDirectories(path))
            {
                var folderInfo = new DirectoryInfo(folder);
                var folderName = folderInfo.Name;
                var jsonAsset = contentPack.ReadJsonFile<JsonAsset>($"BigCraftables/{folderName}/big-craftable.json");
                var texturePath = $"BigCraftables/{folderName}/big-craftable.png";
                if (string.IsNullOrWhiteSpace(jsonAsset.Name) || string.IsNullOrWhiteSpace(jsonAsset.Description) || !contentPack.HasFile(texturePath))
                {
                    continue;
                }

                jsonAssets.JsonAssets.Add(jsonAsset);
                var textureMigrator = new TextureMigrator(contentPack, jsonAsset.Name);
                var texture = contentPack.LoadAsset<Texture2D>(texturePath);
                textureMigrator.AddTexture("big-craftable.png", texture);

                for (var frame = 1; frame <= 18; frame++)
                {
                    texturePath = $"BigCraftables/{folderName}/big-craftable-{frame.ToString()}.png";
                    if (!contentPack.HasFile(texturePath))
                    {
                        continue;
                    }

                    texture = contentPack.LoadAsset<Texture2D>(texturePath);
                    textureMigrator.AddTexture($"big-craftable-{frame.ToString()}.png", texture);
                }

                jsonAssets.Textures.Add(textureMigrator);
            }

            return jsonAssets;
        }

        public bool ToDynamicGameAssets(IContentPack contentPack)
        {
            // Create localization Directory
            Directory.CreateDirectory(Path.Combine(contentPack.DirectoryPath, "i18n"));

            var jsonFiles = new Dictionary<string, StringBuilder>();

            // Generate content.json
            var contentJson = new StringBuilder("[");

            // Generate localization files
            var defaultJson = new StringBuilder("{");
            jsonFiles.Add("default", defaultJson);

            foreach (var jsonAsset in this.JsonAssets)
            {
                // BigCraftable
                contentJson.Append(
                    @$"
    {{
        ""$ItemType"": ""BigCraftable"",
        ""ID"": ""{jsonAsset.Name}"",
        ""JsonAssetsName"": ""{jsonAsset.Name}"",
        ""Texture"": ""assets/{jsonAsset.Name}.png:0""
    }},");

                // Default Localizations
                defaultJson.Append(
                    $@"
    ""big-craftable.{jsonAsset.Name}.name"": ""{jsonAsset.Name}"",
    ""big-craftable.{jsonAsset.Name}.description"": ""{jsonAsset.Description}"",");

                // Additional Localizations
                if (jsonAsset.NameLocalization is not null && jsonAsset.DescriptionLocalization is not null)
                {
                    var localizationKeys = jsonAsset.NameLocalization.Keys.Union(jsonAsset.DescriptionLocalization.Keys).Distinct();
                    foreach (var localizationKey in localizationKeys)
                    {
                        if (!jsonFiles.TryGetValue(localizationKey, out var jsonFile))
                        {
                            jsonFile = new StringBuilder("{");
                            jsonFiles.Add(localizationKey, jsonFile);
                        }

                        if (jsonAsset.NameLocalization.TryGetValue(localizationKey, out var localizedName))
                        {
                            jsonFile.Append(
                                $@"
    ""big-craftable.{jsonAsset.Name}.name"": ""{localizedName}"",");
                        }

                        if (jsonAsset.DescriptionLocalization.TryGetValue(localizationKey, out var localizedDescription))
                        {
                            jsonFile.Append(
                                $@"
    ""big-craftable.{jsonAsset.Name}.description"": ""{localizedDescription}"",");
                        }
                    }
                }

                // CraftingRecipe
                if (jsonAsset.Recipe is not null)
                {
                    contentJson.Append(
                        @$"
    {{
        ""$ItemType"": ""CraftingRecipe"",
        ""ID"": ""{jsonAsset.Name} recipe"",
        ""IsCooking"": false,
        ""KnownByDefault"": ""{(jsonAsset.Recipe.IsDefault ? "true" : "false")}"",
        ""Ingredients"": [");

                    foreach (var ingredient in jsonAsset.Recipe.Ingredients)
                    {
                        contentJson.Append(
                            $@"
            {{
                ""Type"": ""VanillaObject"",
                ""Value"": {ingredient.Object.ToString()},
                ""Quantity"": {ingredient.Count.ToString()}
            }},");
                    }

                    contentJson.Append(
                        $@"
        ],
        ""Result"": {{
            ""Type"": ""DGAItem"",
            ""Value"": ""{contentPack.Manifest.UniqueID}/{jsonAsset.Name}"",
            ""Quantity"": 1
        }},");

                    if (!string.IsNullOrWhiteSpace(jsonAsset.Recipe.SkillUnlockName) && jsonAsset.Recipe.SkillUnlockLevel > 0)
                    {
                        contentJson.Append(
                            $@"
        ""SkillUnlockName"": ""{jsonAsset.Recipe.SkillUnlockName}"",
        ""SkillUnlockLevel"": {jsonAsset.Recipe.SkillUnlockLevel.ToString()},");
                    }

                    contentJson.Append(
                        @"
    },");

                    // ShopEntry for CraftingRecipe
                    if (!string.IsNullOrWhiteSpace(jsonAsset.Recipe.PurchaseFrom) && jsonAsset.Recipe.PurchasePrice > 0)
                    {
                        contentJson.Append(
                            $@"
    {{
        ""$ItemType"": ""ShopEntry"",
        ""ShopID"": ""{JsonAssetsMigrator.ToDGAShop(jsonAsset.Recipe.PurchaseFrom)}"",
        ""Item"": {{
            ""Type"": ""DGARecipe"",
            ""Value"": ""{contentPack.Manifest.UniqueID}/{jsonAsset.Name} recipe""
        }},
        ""Cost"": {jsonAsset.Recipe.PurchasePrice.ToString()},
    }},");
                    }

                    // Default Localizations
                    defaultJson.Append(
                        $@"
    ""big-craftable.{jsonAsset.Name} recipe.name"": ""{jsonAsset.Name} recipe"",
    ""big-craftable.{jsonAsset.Name} recipe.description"": ""{jsonAsset.Description}"",");
                }

                if (!string.IsNullOrWhiteSpace(jsonAsset.PurchaseFrom) && jsonAsset.PurchasePrice > 0)
                {
                    contentJson.Append(
                        $@"
    {{
        ""$ItemType"": ""ShopEntry"",
        ""ShopID"": ""{JsonAssetsMigrator.ToDGAShop(jsonAsset.PurchaseFrom!)}"",
        ""Item"": {{
            ""Type"": ""DGARecipe"",
            ""Value"": ""{contentPack.Manifest.UniqueID}/{jsonAsset.Name}""
        }},
        ""Cost"": {jsonAsset.PurchasePrice!.ToString()}
    }},");
                }
            }

            contentJson.Append(
                @"
]");

            // Write content.json
            File.WriteAllText(Path.Combine(contentPack.DirectoryPath, "content.json"), contentJson.ToString());

            // Complete localization and write file(s)
            var path = Path.Combine(contentPack.DirectoryPath, "i18n");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            foreach (var jsonFile in jsonFiles)
            {
                jsonFile.Value.Append(
                    @"
}");

                File.WriteAllText(Path.Combine(contentPack.DirectoryPath, "i18n", $"{jsonFile.Key}.json"), jsonFile.Value.ToString());
            }

            // Process textures
            path = Path.Combine(contentPack.DirectoryPath, "assets");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            foreach (var texture in this.Textures)
            {
                texture.UpdateTextureFormat();
            }

            return true;
        }

        private static string ToDGAShop(string jaShop)
        {
            return jaShop switch
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
                _ => jaShop,
            };
        }
    }
}