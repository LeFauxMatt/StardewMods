namespace Common.Integrations.ProducerFrameworkMod;

using System.Collections.Generic;
using SObject = StardewValley.Object;

/// <summary>
///     Public api to get the producer framework recipes.
/// </summary>
public interface IProducerFrameworkModApi
{
    /// <summary>
    ///     Adds a content pack from the specified directory.
    ///     This method expects a content-pack.json file instead of a manifest.json
    /// </summary>
    /// <param name="directory">The absolute path of the content pack.</param>
    /// <returns>true if the content pack was successfully loaded, otherwise false.</returns>
    bool AddContentPack(string directory);

    /// <summary>
    ///     Get the list of recipes
    ///     The recipe format follow the MachineRecipeData class properties from Lookup Anything mod.
    ///     There are some additional properties that are not presented on that class, these ones has the name of the content
    ///     pack properties of this mod.
    /// </summary>
    /// <returns>The list of recipes.</returns>
    List<Dictionary<string, object>> GetRecipes();

    /// <summary>
    ///     Get the list of recipes for the producer with the giving name.
    ///     The recipe format follow the MachineRecipeData class properties from Lookup Anything mod.
    ///     There are some additional properties that are not presented on that class, these ones has the name of the content
    ///     pack properties of this mod.
    /// </summary>
    /// <param name="producerName">The name of the producer.</param>
    /// <returns>The list of recipes.</returns>
    List<Dictionary<string, object>> GetRecipes(string producerName);

    /// <summary>
    ///     Get the list of recipes for the producer.
    ///     The recipe format follow the MachineRecipeData class properties from Lookup Anything mod.
    ///     There are some additional properties that are not presented on that class, these ones has the name of the content
    ///     pack properties of this mod.
    /// </summary>
    /// <param name="producerObject">The Stardew Valley Object for the producer.</param>
    /// <returns>The list of recipes.</returns>
    List<Dictionary<string, object>> GetRecipes(SObject producerObject);
}