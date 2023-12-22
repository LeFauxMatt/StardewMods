namespace StardewMods.Common.Services.Integrations.BetterCrafting;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;

/// <summary>
///     The various currency types supported by <see cref="IBetterCrafting.CreateCurrencyIngredient(string, int)" />
/// </summary>
public enum CurrencyType
{
    /// <summary>The player's gold.</summary>
    Money,

    /// <summary>
    ///     The player's earned points at the current festival. This should likely never actually be used, since players
    ///     can't craft while they're at a festival in the first place.
    /// </summary>
    FestivalPoints,

    /// <summary>The player's casino points.</summary>
    ClubCoins,

    /// <summary>The player's Qi Gems.</summary>
    QiGems,
}

/// <summary>
///     An <c>IInventoryProvider</c> is used by Better Crafting to discover and interact with various item storages in
///     the game.
/// </summary>
public interface IInventoryProvider
{
    /// <summary>Check to see if items can be extracted from this object.</summary>
    /// <param name="obj">the object</param>
    /// <param name="location">the map where the object is</param>
    /// <param name="who">the player accessing the inventory, or null if no player is involved</param>
    /// <returns></returns>
    bool CanExtractItems(object obj, GameLocation? location, Farmer? who);

    /// <summary>Check to see if items can be inserted into this object.</summary>
    /// <param name="obj">the object</param>
    /// <param name="location">the map where the object is</param>
    /// <param name="who">the player accessing the inventory, or null if no player is involved</param>
    /// <returns></returns>
    bool CanInsertItems(object obj, GameLocation? location, Farmer? who);

    /// <summary>Clean the inventory of the object. This is for removing null entries, organizing, etc.</summary>
    /// <param name="obj">the object</param>
    /// <param name="location">the map where the object is</param>
    /// <param name="who">the player accessing the inventory, or null if no player is involved</param>
    void CleanInventory(object obj, GameLocation? location, Farmer? who);

    /// <summary>
    ///     Get the actual inventory capacity of the object's inventory. New items may be added to the GetItems() list up
    ///     until this count.
    /// </summary>
    /// <param name="obj">the object</param>
    /// <param name="location">the map where the object is</param>
    /// <param name="who">the player accessing the inventory, or null if no player is involved</param>
    int GetActualCapacity(object obj, GameLocation? location, Farmer? who);

    /// <summary>
    ///     Get a list of items in the object's inventory, for modification or viewing. Assume that anything using this
    ///     list will use GetMutex() to lock the inventory before modifying.
    /// </summary>
    /// <param name="obj">the object</param>
    /// <param name="location">the map where the object is</param>
    /// <param name="who">the player accessing the inventory, or null if no player is involved</param>
    IList<Item?>? GetItems(object obj, GameLocation? location, Farmer? who);

    /// <summary>
    ///     For objects larger than a single tile on the map, return the rectangle representing the object. For single
    ///     tile objects, return null.
    /// </summary>
    /// <param name="obj">the object</param>
    /// <param name="location">the map where the object is</param>
    /// <param name="who">the player accessing the inventory, or null if no player is involved</param>
    /// <returns></returns>
    Rectangle? GetMultiTileRegion(object obj, GameLocation? location, Farmer? who);

    /// <summary>
    ///     Get the NetMutex that locks the object for multiplayer synchronization. This method must return a mutex. If
    ///     null is returned, the object will be skipped.
    /// </summary>
    /// <param name="obj">the object</param>
    /// <param name="location">the map where the object is</param>
    /// <param name="who">the player accessing the inventory, or null if no player is involved</param>
    NetMutex? GetMutex(object obj, GameLocation? location, Farmer? who);

    /// <summary>
    ///     Return the real position of the object. If the object has no position, returns null. For multi-tile objects,
    ///     this should return the "main" object if there is one.
    /// </summary>
    /// <param name="obj">the object</param>
    /// <param name="location">the map where the object is</param>
    /// <param name="who">the player accessing the inventory, or null if no player is involved</param>
    Vector2? GetTilePosition(object obj, GameLocation? location, Farmer? who);

    /// <summary>Check to see if a specific item is allowed to be stored in the object's inventory.</summary>
    /// <param name="obj">the object</param>
    /// <param name="location">the map where the object is</param>
    /// <param name="who">the player accessing the inventory, or null if no player is involved</param>
    /// <param name="item">the item we're checking</param>
    bool IsItemValid(object obj, GameLocation? location, Farmer? who, Item item) => true;

    /// <summary>
    ///     Whether or not a mutex is required for interacting with this object's inventory. You should always use a mutex
    ///     to ensure items are handled safely with multiplayer, but in case you're doing something exceptional and Better
    ///     Crafting should not worry about locking, you can explicitly disable mutex handling.
    /// </summary>
    /// <param name="obj">the object</param>
    /// <param name="location">the map where the object is</param>
    /// <param name="who">the player accessing the inventory, or null if no player is involved</param>
    bool IsMutexRequired(object obj, GameLocation? location, Farmer? who) => true;

    /// <summary>
    ///     Check to see if this object is valid for inventory operations. If location is null, it should not be
    ///     considered when determining the validitiy of the object.
    /// </summary>
    /// <param name="obj">the object</param>
    /// <param name="location">the map where the object is</param>
    /// <param name="who">the player accessing the inventory, or null if no player is involved</param>
    /// <returns>whether or not the object is valid</returns>
    bool IsValid(object obj, GameLocation? location, Farmer? who);
}

/// <summary>
///     An <c>IInventory</c> represents an item storage that Better Crafting is interacting with, whether by
///     extracting items or inserting them.
/// </summary>
public interface IInventory
{
    /// <summary>Where this object is located, if a location is relevant.</summary>
    GameLocation? Location { get; }

    /// <summary>
    ///     The NetMutex for this object, which should be locked before using it. If there is no mutex, then we apparently
    ///     don't need to worry about that.
    /// </summary>
    NetMutex? Mutex { get; }

    /// <summary>The object that has inventory.</summary>
    object Object { get; }

    /// <summary>The player accessing the inventory, if a player is involved.</summary>
    Farmer? Player { get; }

    /// <summary>Whether or not we can extract items from this inventory.</summary>
    bool CanExtractItems();

    /// <summary>Whether or not we can insert items into this inventory.</summary>
    bool CanInsertItems();

    /// <summary>Attempt to clean the object's inventory. This should remove null entries, and run any other necessary logic.</summary>
    void CleanInventory();

    /// <summary>
    ///     Get the number of item slots in the object's inventory. When adding items to the inventory, we will never
    ///     extend the list beyond this number of entries.
    /// </summary>
    int GetActualCapacity();

    /// <summary>
    ///     Get this object's inventory as a list of items. May be null if there is an issue accessing the object's
    ///     inventory.
    /// </summary>
    IList<Item?>? GetItems();

    /// <summary>
    ///     For multi-tile inventories, the region that this inventory takes up in the world. Only rectangular multi-tile
    ///     inventories are supported, and this is used primarily for discovering connections.
    /// </summary>
    Rectangle? GetMultiTileRegion();

    /// <summary>
    ///     Get the tile position of this object in the world, if it has one. For multi-tile inventories, this should be
    ///     the primary tile if one exists.
    /// </summary>
    Vector2? GetTilePosition();

    /// <summary>Check to see if a specific item is allowed to be stored in the object's inventory.</summary>
    /// <param name="item">The item we're checking</param>
    bool IsItemValid(Item item);

    /// <summary>Whether or not the object is locked and ready for read/write usage.</summary>
    bool IsLocked();

    /// <summary>Whether or not the object is a valid inventory.</summary>
    bool IsValid();
}

/// <summary>
///     An <c>IIngredient</c> represents a single ingredient used when crafting a recipe. An ingredient can be an
///     item, a currency, or anything else. The API provides methods for getting basic item and currency ingredients, so
///     you need not use this unless you're doing something fancy.
/// </summary>
public interface IIngredient
{
    // Display
    /// <summary>The name of this ingredient to be displayed in the menu.</summary>
    string DisplayName { get; }

    /// <summary>The amount of this ingredient required to perform a craft.</summary>
    int Quantity { get; }

    /// <summary>The source rectangle to use when drawing this ingredient in the menu.</summary>
    Rectangle SourceRectangle { get; }

    /// <summary>
    ///     Whether or not this <c>IIngredient</c> supports quality control options, including using low quality first and
    ///     limiting the maximum quality to use.
    /// </summary>
    bool SupportsQuality { get; }

    /// <summary>The texture to use when drawing this ingredient in the menu.</summary>
    Texture2D Texture { get; }

    /// <summary>Consume this ingredient out of the player's inventory and the other available inventories.</summary>
    /// <param name="who">The farmer performing the craft</param>
    /// <param name="inventories">All the available inventories.</param>
    /// <param name="maxQuality">
    ///     The maximum item quality we are allowed to count. This cannot be ignored unless
    ///     <see cref="SupportsQuality" /> returns <c>false</c>.
    /// </param>
    /// <param name="lowQualityFirst">
    ///     Whether or not we should make an effort to consume lower quality ingredients before
    ///     ocnsuming higher quality ingredients.
    /// </param>
    void Consume(Farmer who, IList<IInventory>? inventories, int maxQuality, bool lowQualityFirst);

    /// <summary>
    ///     Determine how much of this ingredient is available for crafting both in the player's inventory and in the
    ///     other inventories.
    /// </summary>
    /// <param name="who">The farmer performing the craft</param>
    /// <param name="items">
    ///     A list of all available <see cref="Item" />s across all available <see cref="IInventory" />
    ///     instances. If you only support consuming ingredients from certain <c>IInventory</c> types, you should not use this
    ///     value and instead iterate over the inventories. Please note that this does <b>not</b> include the player's
    ///     inventory.
    /// </param>
    /// <param name="inventories">All the available inventories.</param>
    /// <param name="maxQuality">
    ///     The maximum item quality we are allowed to count. This cannot be ignored unless
    ///     <see cref="SupportsQuality" /> returns <c>false</c>.
    /// </param>
    int GetAvailableQuantity(Farmer who, IList<Item?>? items, IList<IInventory>? inventories, int maxQuality);
}

/// <summary>
///     This event is dispatched by Better Crafting whenever a player performs a craft, and may be fired multiple
///     times in quick succession if a player is performing bulk crafting.
/// </summary>
public interface IPerformCraftEvent
{
    /// <summary>The item being crafted, may be null depending on the recipe.</summary>
    Item? Item { get; set; }

    /// <summary>The <c>BetterCraftingPage</c> menu instance that the player is crafting from.</summary>
    IClickableMenu Menu { get; }

    /// <summary>The player performing the craft.</summary>
    Farmer Player { get; }

    /// <summary>
    ///     Cancel the craft, marking it as a failure. The ingredients will not be consumed and the player will not
    ///     receive the item.
    /// </summary>
    void Cancel();

    /// <summary>
    ///     Complete the craft, marking it as a success. The ingredients will be consumed and the player will receive the
    ///     item, if there is one.
    /// </summary>
    void Complete();
}

/// <summary>
///     An <c>IRecipe</c> represents a single crafting recipe, though it need not be associated with a vanilla
///     <see cref="StardewValley.CraftingRecipe" />. Recipes usually produce <see cref="Item" />s, but they are not
///     required to do so.
/// </summary>
public interface IRecipe
{
    /// <summary>
    ///     The vanilla <c>CraftingRecipe</c> instance for this recipe, if one exists. This may be used for
    ///     interoperability with some other mods, but is not required.
    /// </summary>
    CraftingRecipe? CraftingRecipe { get; }

    /// <summary>An optional description of the recipe displayed on its tooltip.</summary>
    string? Description { get; }

    /// <summary>A name displayed to the user.</summary>
    string DisplayName { get; }

    /// <summary>How tall this recipe should appear in the menu, in grid squares.</summary>
    int GridHeight { get; }

    /// <summary>How wide this recipe should appear in the menu, in grid squares.</summary>
    int GridWidth { get; }

    /// <summary>The ingredients used by this recipe.</summary>
    IIngredient[]? Ingredients { get; }

    /// <summary>
    ///     The internal name of the recipe. For standard recipes, this matches the name of the recipe used in the
    ///     player's cookingRecipes / craftingRecipes dictionaries. For non-standard recipes, this can be anything as long as
    ///     it's unique, and it's recommended to prefix the names with your mod's unique ID to ensure uniqueness.
    /// </summary>
    string Name { get; }

    /// <summary>The quantity of item produced every time this recipe is crafted.</summary>
    int QuantityPerCraft { get; }

    /// <summary>
    ///     An addditional sorting value to apply to recipes in the Better Crafting menu. Applied before other forms of
    ///     sorting.
    /// </summary>
    int SortValue { get; }

    /// <summary>The source rectangle to use when drawing this recipe in the menu.</summary>
    Rectangle SourceRectangle { get; }

    /// <summary>Whether or not the item created by this recipe is stackable, and thus eligible for bulk crafting.</summary>
    bool Stackable { get; }

    /// <summary>The texture to use when drawing this recipe in the menu.</summary>
    Texture2D Texture { get; }

    /// <summary>
    ///     Check to see if the given player can currently craft this recipe. This method is suitable for checking
    ///     external conditions. For example, the add-on for crafting buildings from the crafting menu uses this to check that
    ///     the current <see cref="GameLocation" /> allows building.
    /// </summary>
    /// <param name="who">The player we're asking about.</param>
    bool CanCraft(Farmer who);

    /// <summary>
    ///     Create an instance of the Item this recipe crafts, if this recipe crafts an item. Returning null is perfectly
    ///     acceptable.
    /// </summary>
    Item? CreateItem();

    /// <summary>
    ///     How many times the player has crafted this recipe. If advanced crafting information is enabled, and this value
    ///     is non-zero, it will be displayed on recipe tooltips.
    /// </summary>
    /// <param name="who">The player we're asking about.</param>
    int GetTimesCrafted(Farmer who);

    /// <summary>
    ///     An optional, extra string to appear on item tooltips. This can be used for displaying error messages to the
    ///     user, or anything else that would be relevant. For example, the add-on for crafting buildings uses this to display
    ///     error messages telling users why they are unable to craft a building, if they cannot.
    /// </summary>
    /// <param name="who">The player we're asking about.</param>
    string? GetTooltipExtra(Farmer who);

    /// <summary>Whether or not the player knows this recipe.</summary>
    /// <param name="who">The player we're asking about</param>
    bool HasRecipe(Farmer who);

    /// <summary>
    ///     This method is called when performing a craft, and can be used to perform asynchronous actions or other
    ///     additional logic as required. While crafting is taking place, Better Crafting will hold locks on every inventory
    ///     involved. You should ideally do as little work here as possible.
    /// </summary>
    /// <param name="evt">
    ///     Details about the event, and methods for telling Better Crafting when the craft has succeeded or
    ///     failed.
    /// </param>
    void PerformCraft(IPerformCraftEvent evt) => evt.Complete();
}

/// <summary>Better Crafting uses <c>IRecipeProvider</c> to discover crafting recipes for display in the menu.</summary>
public interface IRecipeProvider
{
    /// <summary>
    ///     Whether or not additional recipes from this provider should be cached. If the list should be updated every
    ///     time the player opens the menu, this should return false.
    /// </summary>
    bool CacheAdditionalRecipes { get; }

    /// <summary>
    ///     The priority of this recipe provider, for sorting purposes. When handling CraftingRecipe instances, the first
    ///     provider to return a result is used.
    /// </summary>
    int RecipePriority { get; }

    /// <summary>
    ///     Get any additional recipes in IRecipe form. Additional recipes are those recipes not included in the
    ///     `CraftingRecipe.cookingRecipes` and `CraftingRecipe.craftingRecipes` objects.
    /// </summary>
    /// <param name="cooking">Whether we want cooking recipes or crafting recipes.</param>
    /// <returns>An enumeration of this provider's additional recipes, or null.</returns>
    IEnumerable<IRecipe>? GetAdditionalRecipes(bool cooking);

    /// <summary>Get an <see cref="IRecipe" /> wrapper for a <see cref="CraftingRecipe" />.</summary>
    /// <param name="recipe">The vanilla <c>CraftingRecipe</c> to wrap</param>
    /// <returns>An IRecipe wrapper, or null if this provider does not handle this recipe.</returns>
    IRecipe? GetRecipe(CraftingRecipe recipe);
}

/// <summary>
///     IDynamicRuleData instances represent all the configuration data associated with dynamic rules that have been
///     added to categories.
/// </summary>
public interface IDynamicRuleData
{
    /// <summary>A dictionary of configuration data for this dynamic rule, as parsed by the game's JSON serializer.</summary>
    public IDictionary<string, JToken> Fields { get; }

    /// <summary>The ID of the dynamic rule this data is for.</summary>
    public string Id { get; }
}

/// <summary>
///     IDynamicRuleHandler instances handle the logic of determining whether or not any given <see cref="IRecipe" />
///     matches a dynamic rule, and thus whether the recipe should be displayed in a category using rules. It also handles
///     anything necessary for displaying a user interface to the user for editing the rule's configuration.
/// </summary>
public interface IDynamicRuleHandler
{
    /// <summary>Whether or not this dynamic rule should be allowed to be added to a category multiple times.</summary>
    bool AllowMultiple { get; }

    /// <summary>
    ///     A description of what the dynamic rule matches, to be displayed to the user when hovering over the rule in the
    ///     interface to add a new rule.
    /// </summary>
    string Description { get; }

    /// <summary>The name of the dynamic rule, to be displayed to the user when editing a category.</summary>
    string DisplayName { get; }

    /// <summary>Whether or not this dynamic rule has a custom editor.</summary>
    bool HasEditor { get; }

    /// <summary>The source area for an icon to display alongside this dynamic rule.</summary>
    Rectangle Source { get; }

    /// <summary>The source texture for an icon to display alongside this dynamic rule.</summary>
    Texture2D Texture { get; }

    /// <summary>This method checks whether a recipe matches this dynamic rule or not.</summary>
    /// <param name="recipe">The recipe being checked.</param>
    /// <param name="item">The item output of the recipe being checked.</param>
    /// <param name="state">The state object returned from <see cref="ParseState(IDynamicRuleData)" /></param>
    bool DoesRecipeMatch(IRecipe recipe, Lazy<Item?> item, object? state);

    /// <summary>
    ///     WIP! This currently does not function. In the future, this will obtain a new editor child menu that will be
    ///     rendered within the rule editor.
    /// </summary>
    /// <param name="parent">The rule editor</param>
    /// <param name="data">The data of the rule being edited</param>
    IClickableMenu? GetEditor(IClickableMenu parent, IDynamicRuleData data);

    /// <summary>
    ///     This method is called before a dynamic rule is executed, allowing the rule to parse its configuration into a
    ///     state object that can be re-used when checking recipes against the rule.
    /// </summary>
    /// <param name="data">The data of the rule</param>
    /// <returns>A custom state object, or null if no state is required</returns>
    object? ParseState(IDynamicRuleData data);
}

/// <summary>
///     ISimpleInputRuleHandler is an <see cref="IDynamicRuleHandler" /> that only has a single text input for
///     configuring it. This allows you to create basic rules without needing to implement a configuration interface.
/// </summary>
public interface ISimpleInputRuleHandler : IDynamicRuleHandler
{
    /// <summary>
    ///     If set to a string, this string will be displayed alongside the text editor added to the rule editor for this
    ///     rule.
    /// </summary>
    string? HelpText { get; }
}

/// <summary>
///     This class allows you to easily modify any part of an <see cref="IRecipe" />'s behavior, including its
///     appearance, cost, and the item(s) it produces. This is primarily meant for customizing how an existing
///     <see cref="CraftingRecipe" /> functions, but can be used for creating new recipes.
/// </summary>
public interface IRecipeBuilder
{
    /// <summary>Add a new ingredient to the recipe's ingredients list.</summary>
    /// <param name="ingredient">The ingredient to add</param>
    /// <returns>The same <see cref="IRecipeBuilder" /> instance</returns>
    IRecipeBuilder AddIngredient(IIngredient ingredient);

    /// <summary>Add multiple new ingredients to the recipe's ingredients list.</summary>
    /// <param name="ingredients">The ingredients to add</param>
    /// <returns>The same <see cref="IRecipeBuilder" /> instance</returns>
    IRecipeBuilder AddIngredients(IEnumerable<IIngredient> ingredients);

    /// <summary>Build this recipe and return it.</summary>
    /// <returns>The built <see cref="IRecipe" /></returns>
    IRecipe Build();

    /// <summary>
    ///     Check to see if the given player can currently craft this recipe. See <see cref="IRecipe.CanCraft(Farmer)" />
    ///     for more details. Setting this to <c>null</c> will restore the default behavior.
    /// </summary>
    /// <param name="canCraft">A method to check if the given player can craft the recipe.</param>
    /// <returns>The same <see cref="IRecipeBuilder" /> instance</returns>
    IRecipeBuilder CanCraft(Func<Farmer, bool>? canCraft);

    /// <summary>Clear a grid size set with a previous call to <see cref="GridSize(int, int)" />.</summary>
    /// <returns>The same <see cref="IRecipeBuilder" /> instance</returns>
    IRecipeBuilder ClearGridSize();

    /// <summary>
    ///     Clear the recipe's ingredients list. Optionally, a predicate can be provided to only clear ingredients from
    ///     the list that match the predicate.
    /// </summary>
    /// <param name="predicate">An optional predicate for selecting which ingredients should be removed.</param>
    /// <returns>The same <see cref="IRecipeBuilder" /> instance</returns>
    IRecipeBuilder ClearIngredients(Func<IIngredient, bool>? predicate = null);

    /// <summary>
    ///     Set the recipe's optional description. Setting this to <c>null</c> will restore the default description. The
    ///     method returning <c>null</c> will result in no description being displayed.
    /// </summary>
    /// <param name="description">A method that returns a description.</param>
    /// <returns>The same <see cref="IRecipeBuilder" /> instance</returns>
    IRecipeBuilder Description(Func<string?>? description);

    /// <summary>Set the recipe's display name. Setting this to <c>null</c> will restore the default display name.</summary>
    /// <param name="displayName">A method that returns a display name.</param>
    /// <returns>The same <see cref="IRecipeBuilder" /> instance</returns>
    IRecipeBuilder DisplayName(Func<string>? displayName);

    /// <summary>
    ///     Get how many times a given player has crafted this recipe. Setting this to <c>null</c> will restore the
    ///     default behavior.
    /// </summary>
    /// <param name="timesCrafted">A method that returns the number of times crafted.</param>
    /// <returns>The same <see cref="IRecipeBuilder" /> instance</returns>
    IRecipeBuilder GetTimesCrafted(Func<Farmer, int>? timesCrafted);

    /// <summary>
    ///     An optional, extra string to appear on recipe tool-tips. See <see cref="IRecipe.GetTooltipExtra(Farmer)" />
    ///     for more details. Setting this to <c>null</c> will restore the default behavior.
    /// </summary>
    /// <param name="tooltipExtra">A method returning an optional, extra string to display.</param>
    /// <returns>The same <see cref="IRecipeBuilder" /> instance</returns>
    IRecipeBuilder GetTooltipExtra(Func<Farmer, string?>? tooltipExtra);

    /// <summary>
    ///     Set a size for the recipe to appear as within UI. By default, this will be calculated based on the aspect
    ///     ratio of the source rectangle. Please note that you should not use an entry larger than 4x4, and usually not more
    ///     than 1x2 or 2x2, to ensure the recipe will fit in the user interface correctly.
    /// </summary>
    /// <param name="width">The size of the recipe in the grid, defaults to 1 or 2</param>
    /// <param name="height">The size of the recipe in the grid, defaults to 1 or 2</param>
    /// <returns>The same <see cref="IRecipeBuilder" /> instance</returns>
    IRecipeBuilder GridSize(int width, int height);

    /// <summary>
    ///     Check to see whether or not a given player knows this recipe. Setting this to <c>null</c> will restore the
    ///     default behavior.
    /// </summary>
    /// <param name="hasRecipe">A method that checks if the player knows this recipe.</param>
    /// <returns>The same <see cref="IRecipeBuilder" /> instance</returns>
    IRecipeBuilder HasRecipe(Func<Farmer, bool>? hasRecipe);

    /// <summary>
    ///     Creates an instance of the <see cref="Item" /> this recipe crafts, if this recipe crafts an item. Return
    ///     <c>null</c> if the recipe does not create an item (and use your logic in
    ///     <see cref="OnPerformCraft(Action{IPerformCraftEvent}?)" />). Setting this to <c>null</c> will restore the default
    ///     behavior.
    /// </summary>
    /// <param name="createItem">A method that returns a created <see cref="Item" />, or <c>null</c>.</param>
    /// <returns>The same <see cref="IRecipeBuilder" /> instance</returns>
    IRecipeBuilder Item(Func<Item?>? createItem);

    /// <summary>
    ///     A method called when performing a craft, which can be used to perform asynchronous actions or other additional
    ///     logic. See <see cref="IRecipe.PerformCraft(IPerformCraftEvent)" /> for more details. Setting this to <c>null</c>
    ///     will restore the default behavior.
    /// </summary>
    /// <param name="action">A method called when performing a craft.</param>
    /// <returns>The same <see cref="IRecipeBuilder" /> instance</returns>
    IRecipeBuilder OnPerformCraft(Action<IPerformCraftEvent>? action);

    /// <summary>
    ///     The quantity of <see cref="Item" /> produced every time this recipe is crafted. This value overrides the stack
    ///     size of the item returned from <see cref="Item(Func{Item?}?)" />. Setting this to <c>null</c> will restore the
    ///     default value, <c>1</c>.
    /// </summary>
    /// <param name="quantity">The quantity of item to produce per craft.</param>
    /// <returns>The same <see cref="IRecipeBuilder" /> instance</returns>
    IRecipeBuilder Quantity(int? quantity);

    /// <summary>
    ///     An additional sorting value to apply to the recipe in the Better Crafting menu. This is applied before other
    ///     forms of sorting. Setting this to <c>null</c> will restore the default behavior.
    /// </summary>
    /// <param name="value">The sorting value. 0 by default.</param>
    /// <returns>The same <see cref="IRecipeBuilder" /> instance</returns>
    IRecipeBuilder SortValue(int? value);

    /// <summary>
    ///     The source rectangle to use when drawing this recipe in UI. Setting this to <c>null</c> will restore the
    ///     default source rectangle. If this method returns <c>null</c>, the entire texture will be used.
    /// </summary>
    /// <param name="source">A method that returns a <see cref="Rectangle" /> or <c>null</c>.</param>
    /// <returns>The same <see cref="IRecipeBuilder" /> instance</returns>
    IRecipeBuilder Source(Func<Rectangle?>? source);

    /// <summary>
    ///     Whether or not the <see cref="Item" /> produced by this recipe should be considered stackable, which allows or
    ///     disallows the use of the bulk crafting menu. Setting this to <c>null</c> will restore the default value, which
    ///     checks the <see cref="Item.maximumStackSize" /> of this recipe's output item. Please note that this does not make
    ///     the resulting item unstackable, but only affects how it is handled in the UI.
    /// </summary>
    /// <param name="stackable">Whether or not the recipe's output is stackable.</param>
    /// <returns>The same <see cref="IRecipeBuilder" /> instance</returns>
    IRecipeBuilder Stackable(bool? stackable);

    /// <summary>
    ///     The texture to use when drawing this recipe in UI. Setting this to <c>null</c> will restore the default
    ///     texture. The result of this function call will be cached as appropriate. If a recipe has no texture, an error icon
    ///     will be displayed instead for the recipe.
    /// </summary>
    /// <param name="texture">A method that returns a <see cref="Texture2D" />.</param>
    /// <returns>The same <see cref="IRecipeBuilder" /> instance</returns>
    IRecipeBuilder Texture(Func<Texture2D>? texture);
}

/// <summary>This interface contains a few basic properties on the Better Crafting menu that may be useful for other mods.</summary>
public interface IBetterCraftingMenu
{
    /// <summary>
    ///     Get the current recipe. This is normally the recipe that the player's cursor is hovering over, but when
    ///     performing a craft or when the bulk crafting menu is open, it will return the relevant recipe.
    /// </summary>
    IRecipe? ActiveRecipe { get; }

    /// <summary>Whether or not this crafting menu is for cooking. If this is false, then the menu is for crafting recipes.</summary>
    bool Cooking { get; }

    /// <summary>Whether or not the user is currently editing their categories.</summary>
    bool Editing { get; }

    /// <summary>
    ///     The <see cref="IClickableMenu" /> instance for this menu. This is the same object, but included for
    ///     convenience due to how API proxying works.
    /// </summary>
    IClickableMenu Menu { get; }

    /// <summary>
    ///     Whether or not this is a standalone menu. If this is false, this menu is likely contained in
    ///     <see cref="GameMenu" />.
    /// </summary>
    bool Standalone { get; }

    /// <summary>
    ///     Whether or not the menu is actively crafting something. This will only return true when a craft is happening,
    ///     or when the menu is waiting for an asynchronous craft to return.
    /// </summary>
    bool Working { get; }

    /// <summary>
    ///     Get a list of specific recipes that are to be displayed in the crafting menu. If this list is <c>null</c>, all
    ///     recipes will be displayed to the user.
    /// </summary>
    IReadOnlyList<string>? GetListedRecipes();

    /// <summary>Calling this method will toggle edit mode, as though the user clicked the button themselves.</summary>
    void ToggleEditMode();

    /// <summary>
    ///     Set a new list of specific recipes that are to be displayed in the crafting menu. Note: If the user does not
    ///     know these recipes, they will not be displayed even if they're in this list. Set the list to <c>null</c> to display
    ///     all recipes.
    /// </summary>
    /// <param name="recipes">The list of recipes that should be displayed.</param>
    void UpdateListedRecipes(IEnumerable<string>? recipes);
}

/// <summary>
///     This event is emitted by <see cref="IBetterCrafting" /> whenever a new Better Crafting menu is opened, and
///     serves to allow other mods to add or remove specific containers from a menu.
/// </summary>
public interface IPopulateContainersEvent
{
    IList<Tuple<object, GameLocation?>> Containers { get; }

    /// <summary>The relevant Better Crafting menu.</summary>
    IBetterCraftingMenu Menu { get; }
}

public interface IBetterCrafting
{
    /// <summary>
    ///     This event is fired whenever a new Better Crafting menu is opened, allowing other mods to manipulate the list
    ///     of containers.
    /// </summary>
    event Action<IPopulateContainersEvent>? MenuPopulateContainers;

    /// <summary>
    ///     Register a recipe provider with Better Crafting. Calling this will also invalidate the recipe cache. If the
    ///     recipe provider was already registered, this does nothing.
    /// </summary>
    /// <param name="provider">The recipe provider to add</param>
    void AddRecipeProvider(IRecipeProvider provider);

    /// <summary>Add recipes to a default category. If a player has modified their category, this will not affect them.</summary>
    /// <param name="cooking">If true, we alter a cooking category. Otherwise, crafting.</param>
    /// <param name="categoryId">The ID of the category to alter.</param>
    /// <param name="recipeNames">An enummeration of recipe names for recipes to add to the category.</param>
    void AddRecipesToDefaultCategory(bool cooking, string categoryId, IEnumerable<string> recipeNames);

    /// <summary>
    ///     Consume matching items from a player, and also from a set of <see cref="IInventory" /> instances. This is a
    ///     helper method for building custom <see cref="IIngredient" />s. This method is aware of the mod "Stack Quality" and
    ///     handles merged stacks correctly.
    /// </summary>
    /// <param name="items">
    ///     An enumeration of tuples where the function matches items, and the integer is the quantity to
    ///     consume.
    /// </param>
    /// <param name="who">The player to consume items from, if any. Items are consumed from the player's inventory first.</param>
    /// <param name="inventories">
    ///     An enumeration of <see cref="IInventory" /> instances to consume items from, such as the one
    ///     passed to <see cref="IIngredient.Consume(Farmer, IList{IInventory}?, int, bool)" />.
    /// </param>
    /// <param name="maxQuality">The maximum quality to consume.</param>
    /// <param name="lowQualityFirst">Whether or not to consume low quality items first.</param>
    void ConsumeItems(
        IEnumerable<(Func<Item, bool>, int)> items,
        Farmer? who,
        IEnumerable<IInventory>? inventories,
        int maxQuality = int.MaxValue,
        bool lowQualityFirst = false);

    /// <summary>
    ///     Count the number of <see cref="Item" />s available that match the given predicate, between the given player's
    ///     inventory and the given enumeration of <see cref="Item" />s. This method is aware of the mod "Stack Quality" and
    ///     handles merged stacks correctly.
    /// </summary>
    /// <param name="predicate">A method for checking whether a given <see cref="Item" /> should be counted.</param>
    /// <param name="who">An optional player, to include that player's inventory in the search.</param>
    /// <param name="items">An optional enumeration of <see cref="Item" />s to include in the search.</param>
    /// <param name="maxQuality">The maximum quality of item to count.</param>
    /// <returns>The number of matching items.</returns>
    int CountItem(Func<Item, bool> predicate, Farmer? who, IEnumerable<Item?>? items, int maxQuality = int.MaxValue);

    /// <summary>Create a simple <see cref="IIngredient" /> that matches an item by ID and that consumes an exact quantity.</summary>
    /// <param name="item">The item ID to match.</param>
    /// <param name="quantity">The quantity to consume.</param>
    IIngredient CreateBaseIngredient(int item, int quantity);

    /// <summary>Create a simple <see cref="IIngredient" /> that matches a specific currency and consumes an exact quantity.</summary>
    /// <param name="type">The currency to match.</param>
    /// <param name="quantity">The quantity to consume.</param>
    IIngredient CreateCurrencyIngredient(CurrencyType type, int quantity);

    /// <summary>
    ///     Create a new default category for recipes. Every player will receive this category, but they may delete it or
    ///     alter it as they see fit.
    /// </summary>
    /// <param name="cooking">If true, this category is added to cooking. Otherwise, crafting.</param>
    /// <param name="categoryId">An internal ID for the category. Make sure this is unique.</param>
    /// <param name="Name">A method returning a human-readable name to be displayed in the menu.</param>
    /// <param name="recipeNames">An enumeration of recipe names for recipes to display in the category.</param>
    /// <param name="iconRecipe">The name of a recipe to use as the category's default icon.</param>
    void CreateDefaultCategory(
        bool cooking,
        string categoryId,
        Func<string> Name,
        IEnumerable<string>? recipeNames = null,
        string? iconRecipe = null,
        bool useRules = false,
        IEnumerable<IDynamicRuleData>? rules = null);

    /// <summary>
    ///     Create a simple <see cref="IIngredient" /> that does not match anything but requires a quantity of one, thus
    ///     always preventing a recipe from being crafted. It displays as an error item in the ingredients list.
    /// </summary>
    IIngredient CreateErrorIngredient();

    /// <summary>
    ///     Create a simple <see cref="IIngredient" /> that matches items using a function and that consumes an exact
    ///     quantity.
    /// </summary>
    /// <param name="matcher">The function to check items</param>
    /// <param name="quantity">The quantity to consume.</param>
    /// <param name="displayName">The name to display for the ingredient.</param>
    /// <param name="texture">The texture to display the ingredient with.</param>
    /// <param name="source">The source rectangle of the texture to display.</param>
    IIngredient CreateMatcherIngredient(
        Func<Item, bool> matcher,
        int quantity,
        Func<string> displayName,
        Func<Texture2D> texture,
        Rectangle? source = null);

    [Obsolete("Use the method that takes functions instead.")]
    IIngredient CreateMatcherIngredient(
        Func<Item, bool> matcher,
        int quantity,
        string displayName,
        Texture2D texture,
        Rectangle? source = null);

    /// <summary>
    ///     Create a simple <see cref="IRecipe" /> that gets its properties from an existing <see cref="CraftingRecipe" />
    ///     but that uses different <see cref="IIngredient" />s.
    /// </summary>
    /// <param name="recipe">The <see cref="CraftingRecipe" /> to use as a base.</param>
    /// <param name="ingredients">An enumeration of <see cref="IIngredient" />s the recipe should consume.</param>
    /// <param name="onPerformCraft">An optional event handler to perform additional logic when the item is crafted.</param>
    [Obsolete("Use RecipeBuilder instead.")]
    IRecipe CreateRecipeWithIngredients(
        CraftingRecipe recipe,
        IEnumerable<IIngredient> ingredients,
        Action<IPerformCraftEvent>? onPerformCraft = null);

    /// <summary>Get the currently open Better Crafting menu. This may be <c>null</c> if the menu is still opening.</summary>
    IBetterCraftingMenu? GetActiveMenu();

    /// <summary>Return the Better Crafting menu's type. In case you want to do spooky stuff to it, I guess.</summary>
    /// <returns>The BetterCraftingMenu type.</returns>
    Type GetMenuType();

    /// <summary>Get all known recipes from all providers.</summary>
    /// <param name="cooking">If true, return cooking recipes. If false, return crafting recipes.</param>
    /// <returns>A collection of the recipes.</returns>
    IReadOnlyCollection<IRecipe> GetRecipes(bool cooking);

    /// <summary>
    ///     Invalidate the recipe cache. You should call this if your recipe provider ever adds new recipes after
    ///     registering it.
    /// </summary>
    void InvalidateRecipeCache();

    [Obsolete("Please use the other call with additional parameters.")]
    bool OpenCraftingMenu(
        bool cooking,
        IList<Chest>? containers = null,
        GameLocation? location = null,
        Vector2? position = null,
        bool silent_open = false,
        IList<string>? listed_recipes = null);

    /// <summary>
    ///     Try to open the Better Crafting menu. This may fail if there is another menu open that cannot be replaced. If
    ///     opening the menu from an object in the world, such as a workbench, its location and tile position can be provided
    ///     for automatic detection of nearby chests. Better Crafting has its own handling of mutexes, so please do not worry
    ///     about locking Chests before handing them off to the menu. When discovering additional containers, Better Crafting
    ///     scans all tiles around each of its existing known containers. If a location and position for the menu source is
    ///     provided, the tiles around that position will be scanned as well. Discovery depends on the user's settings, though
    ///     at a minimum a 3x3 area will be scanned to mimic the scanning radius of the vanilla workbench.
    /// </summary>
    /// <param name="cooking">If true, open the cooking menu. If false, open the crafting menu.</param>
    /// <param name="silent_open">If true, do not make a sound upon opening the menu.</param>
    /// <param name="location">The map the associated object is in, or null if there is no object</param>
    /// <param name="position">The tile position the associated object is at, or null if there is no object</param>
    /// <param name="area">
    ///     The tile area the associated object covers, or null if there is no object or if the object only
    ///     covers a single tile
    /// </param>
    /// <param name="discover_containers">If true, attempt to discover additional material containers.</param>
    /// <param name="containers">An optional list of containers to draw extra crafting materials from.</param>
    /// <param name="listed_recipes">
    ///     An optional list of recipes by name. If provided, only these recipes will be listed in the
    ///     crafting menu.
    /// </param>
    /// <returns>Whether or not the menu was opened successfully</returns>
    bool OpenCraftingMenu(
        bool cooking,
        bool silent_open = false,
        GameLocation? location = null,
        Vector2? position = null,
        Rectangle? area = null,
        bool discover_containers = true,
        IList<Tuple<object, GameLocation?>>? containers = null,
        IList<string>? listed_recipes = null);

    /// <summary>Get a new <see cref="IRecipeBuilder" /> for customizing a recipe.</summary>
    /// <param name="recipe">The recipe to customize.</param>
    IRecipeBuilder RecipeBuilder(CraftingRecipe recipe);

    /// <summary>
    ///     Get a new <see cref="IRecipeBuilder" /> for creating a new recipe, not based on an existing crafting recipe.
    ///     If not replacing an existing <see cref="CraftingRecipe" /> then <paramref name="name" /> should be a new, unique
    ///     string.
    /// </summary>
    /// <param name="name">The recipe's name.</param>
    IRecipeBuilder RecipeBuilder(string name);

    /// <summary>
    ///     Register an inventory provider with Better Crafting. Inventory providers are used for interfacing with chests
    ///     and other objects in the world that contain items.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="provider"></param>
    void RegisterInventoryProvider(Type type, IInventoryProvider provider);

    /// <summary>Register a new dynamic rule handler for use with dynamic categories.</summary>
    /// <param name="manifest">The manifest of the mod (your mod) registering this rule handler.</param>
    /// <param name="id">
    ///     An ID for the rule handler. This should be unique within your mod, but can overlap with IDs from other
    ///     mods as rule IDs are prefixed with your mod ID internally.
    /// </param>
    /// <param name="handler">The rule handler instance.</param>
    /// <returns>Whether or not the handler was successfully registered.</returns>
    bool RegisterRuleHandler(IManifest manifest, string id, IDynamicRuleHandler handler);

    /// <summary>
    ///     See <see cref="RegisterRuleHandler(IManifest, string, IDynamicRuleHandler)" /> for details. This method exists
    ///     to ensure the API translation layer functions as you would expect.
    /// </summary>
    bool RegisterRuleHandler(IManifest manifest, string id, ISimpleInputRuleHandler handler);

    /// <summary>
    ///     Unregister a recipe provider. Calling this will also invalidate the recipe cache. If the recipe provider was
    ///     not registered, this does nothing.
    /// </summary>
    /// <param name="provider">The recipe provider to remove</param>
    void RemoveRecipeProvider(IRecipeProvider provider);

    /// <summary>Remove recipes from a default category. If a player has modified their category, this will not affect them.</summary>
    /// <param name="cooking">If true, we alter a cooking category. Otherwise, crafting.</param>
    /// <param name="categoryId">The ID of the category to alter.</param>
    /// <param name="recipeNames">An enummeration of recipe names for recipes to remove from the category.</param>
    void RemoveRecipesFromDefaultCategory(bool cooking, string categoryId, IEnumerable<string> recipeNames);

    /// <summary>Unregister an inventory provider.</summary>
    /// <param name="type"></param>
    void UnregisterInventoryProvider(Type type);

    /// <summary>Unregister a dynamic rule handler.</summary>
    /// <param name="manifest">The manifest of the mod (your mod) which previously registered a rule handler.</param>
    /// <param name="id">The ID of the rule handler.</param>
    /// ///
    /// <returns>Whether or not the handler was successfully unregistered.</returns>
    bool UnregisterRuleHandler(IManifest manifest, string id);
}
