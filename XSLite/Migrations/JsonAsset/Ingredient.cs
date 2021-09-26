namespace XSLite.Migrations.JsonAsset
{
    internal record Ingredient
    {
        public int Object { get; set; }

        public int Count { get; set; } = 1;
    }
}