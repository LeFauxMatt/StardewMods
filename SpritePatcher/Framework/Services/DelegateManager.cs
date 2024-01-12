namespace StardewMods.SpritePatcher.Framework.Services;

using System.Linq.Expressions;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.SpritePatcher.Framework.Models.ComparableValues;
using StardewValley.Objects;

/// <summary>Manages the retrieval of property values from an Item object.</summary>
internal sealed class DelegateManager : BaseService
{
    private readonly Dictionary<string, Delegate> cachedDelegates = new();

    /// <summary>Initializes a new instance of the <see cref="DelegateManager" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public DelegateManager(ILog log, IManifest manifest)
        : base(log, manifest) { }

    /// <summary>Tries to get the value associated with a specific path in an item using compiled delegate functions.</summary>
    /// <param name="source">The item to retrieve the value from.</param>
    /// <param name="path">The path to the desired value.</param>
    /// <param name="value">
    /// When this method returns, contains the value associated with the specified path, if the path is
    /// found; otherwise, null.
    /// </param>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <returns>true if the value was successfully retrieved; otherwise, false.</returns>
    public bool TryGetValue<TSource>(TSource source, string path, [NotNullWhen(true)] out IEquatable<string>? value)
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
                cachedDelegate = source switch
                {
                    Furniture => DelegateManager.CompileGetter<Furniture>(parts),
                    ColoredObject => DelegateManager.CompileGetter<ColoredObject>(parts),
                    SObject => DelegateManager.CompileGetter<SObject>(parts),
                    Item => DelegateManager.CompileGetter<Item>(parts),
                    _ => DelegateManager.CompileGetter<TSource>(parts),
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
            var rawValue = source switch
            {
                Furniture furniture => ((Func<Furniture, object?>)cachedDelegate).Invoke(furniture),
                ColoredObject coloredObject => ((Func<ColoredObject, object?>)cachedDelegate).Invoke(coloredObject),
                SObject @object => ((Func<SObject, object?>)cachedDelegate).Invoke(@object),
                Item item => ((Func<Item, object?>)cachedDelegate).Invoke(item),
                _ => ((Func<TSource, object?>)cachedDelegate).Invoke(source),
            };

            if (rawValue is null)
            {
                value = null;
                return false;
            }

            value = rawValue switch
            {
                string stringValue => new ComparableString(stringValue),
                int intValue => new ComparableInt(intValue),
                bool boolValue => new ComparableBool(boolValue),
                Enum enumValue => Activator.CreateInstance(
                    typeof(ComparableEnum<>).MakeGenericType(enumValue.GetType()),
                    enumValue) as IEquatable<string>,
                _ => new ComparableOther(rawValue),
            };

            return value is not null;
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
            // Check if member is a method
            var methodInfo = body.Type.GetMethod(member);
            if (methodInfo != null && methodInfo.GetParameters().Length == 0)
            {
                body = Expression.Call(body, methodInfo);
                continue;
            }

            // Check if member is a dictionary with constant key type
            if (body.Type.IsGenericType && body.Type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var bodyType = body.Type.GetGenericArguments()[0];
                var itemProperty = body.Type.GetProperty("Item")!;
                if (bodyType == typeof(string))
                {
                    body = Expression.Property(body, itemProperty, Expression.Constant(member));
                    continue;
                }

                if (bodyType == typeof(int) && int.TryParse(member, out var intMember))
                {
                    body = Expression.Property(body, itemProperty, Expression.Constant(intMember));
                    continue;
                }

                throw new NotSupportedException($"Dictionary key type '{bodyType}' is not supported.");
            }

            // Default to any property or field
            body = Expression.PropertyOrField(body, member);
        }

        body = Expression.Convert(body, typeof(object));
        var lambda = Expression.Lambda<Func<T, object>>(body, parameterExpression);
        return lambda.Compile();
    }
}