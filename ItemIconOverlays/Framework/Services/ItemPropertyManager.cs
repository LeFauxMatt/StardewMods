namespace StardewMods.ItemIconOverlays.Framework.Services;

using System.Linq.Expressions;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.ItemIconOverlays.Framework.Interfaces;
using StardewMods.ItemIconOverlays.Framework.Models;
using StardewValley.Objects;

/// <summary>Manages the retrieval of property values from an Item object.</summary>
internal sealed class ItemPropertyManager : BaseService
{
    private readonly Dictionary<string, Delegate> cachedDelegates = new();

    /// <summary>Initializes a new instance of the <see cref="ItemPropertyManager" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public ItemPropertyManager(ILog log, IManifest manifest)
        : base(log, manifest) { }

    /// <summary>Tries to get the value associated with a specific path in an item using compiled delegate functions.</summary>
    /// <param name="item">The item to retrieve the value from.</param>
    /// <param name="path">The path to the desired value.</param>
    /// <param name="value">
    /// When this method returns, contains the value associated with the specified path, if the path is
    /// found; otherwise, null.
    /// </param>
    /// <returns>true if the value was successfully retrieved; otherwise, false.</returns>
    public bool TryGetValue(Item item, string path, [NotNullWhen(true)] out IComparableValue? value)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            value = null;
            return false;
        }

        var parts = path.Split('.');
        if (!this.cachedDelegates.TryGetValue(path, out var cachedDelegate))
        {
            try
            {
                cachedDelegate = item switch
                {
                    Furniture => ItemPropertyManager.CompileGetter<Furniture>(parts),
                    SObject => ItemPropertyManager.CompileGetter<SObject>(parts),
                    _ => ItemPropertyManager.CompileGetter<Item>(parts),
                };

                this.cachedDelegates[path] = cachedDelegate;
            }
            catch (Exception e)
            {
                this.Log.Error("Failed to compile getter for path '{0}': {1}", path, e.Message);
                value = null;
                return false;
            }
        }

        try
        {
            var rawValue = item switch
            {
                Furniture furniture => ((Func<Furniture, object?>)cachedDelegate).Invoke(furniture),
                SObject obj => ((Func<SObject, object?>)cachedDelegate).Invoke(obj),
                _ => ((Func<Item, object?>)cachedDelegate).Invoke(item),
            };

            if (rawValue is null)
            {
                value = null;
                return false;
            }

            value = rawValue switch
            {
                string stringValue => new ComparableValue<string>(
                    stringValue,
                    input => input.Equals(stringValue, StringComparison.OrdinalIgnoreCase)),
                int intValue => new ComparableValue<int>(
                    intValue,
                    input => int.TryParse(input, out var intInput) && intInput == intValue),
                bool boolValue => new ComparableValue<bool>(
                    boolValue,
                    input => bool.TryParse(input, out var boolInput) && boolInput == boolValue),
                _ => new ComparableValue<object>(
                    rawValue,
                    input => input.Equals(rawValue.ToString(), StringComparison.OrdinalIgnoreCase)),
            };

            return true;
        }
        catch (Exception e)
        {
            this.Log.Error("Failed to compile getter for path '{0}': {1}", path, e.Message);
            value = null;
            return false;
        }
    }

    private static Delegate CompileGetter<T>(IEnumerable<string> parts)
    {
        var parameterExpression = Expression.Parameter(typeof(T));
        Expression body = parameterExpression;

        foreach (var member in parts)
        {
            body = Expression.PropertyOrField(body, member);
        }

        body = Expression.Convert(body, typeof(object));
        var lambda = Expression.Lambda<Func<T, object>>(body, parameterExpression);
        return lambda.Compile();
    }
}