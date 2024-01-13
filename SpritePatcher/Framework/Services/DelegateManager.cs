namespace StardewMods.SpritePatcher.Framework.Services;

using System.Collections;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.SpritePatcher.Framework.Models.ComparableValues;

/// <summary>Manages the retrieval of property values from an IHaveModData object.</summary>
internal sealed class DelegateManager : BaseService
{
    private readonly Dictionary<Type, Dictionary<string, Delegate>> cachedDelegates = new();

    /// <summary>Initializes a new instance of the <see cref="DelegateManager" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public DelegateManager(ILog log, IManifest manifest)
        : base(log, manifest) { }

    /// <summary>Used to obtain a value from an object that implements <see cref="IHaveModData" />.</summary>
    /// <param name="source">The object from which the value is being obtained.</param>
    /// <param name="path">The path to the value to be obtained.</param>
    /// <param name="value">When this method returns, contains the value if one is found; otherwise, <see langword="null" />.</param>
    /// <returns><see langword="true" /> if a value is found; otherwise, <see langword="false" />.</returns>
    public delegate bool TryGetComparable(
        IHaveModData source,
        string path,
        [NotNullWhen(true)] out IEquatable<string>? value);

    /// <summary>Tries to get the value associated with a specific path in a source using compiled delegate functions.</summary>
    /// <param name="source">The source to retrieve the value from.</param>
    /// <param name="path">The path to the desired value.</param>
    /// <param name="value">
    /// When this method returns, contains the value associated with the specified path, if the path is
    /// found; otherwise, null.
    /// </param>
    /// <returns>true if the value was successfully retrieved; otherwise, false.</returns>
    public bool TryGetValue(IHaveModData source, string path, [NotNullWhen(true)] out IEquatable<string>? value)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            value = null;
            return false;
        }

        var type = source.GetType();
        if (!this.cachedDelegates.TryGetValue(type, out var delegates))
        {
            delegates = new Dictionary<string, Delegate>();
            this.cachedDelegates[type] = delegates;
        }

        if (!delegates.TryGetValue(path, out var cachedDelegate))
        {
            try
            {
                var method = typeof(DelegateManager).GetMethod(
                    nameof(DelegateManager.CompileGetter),
                    BindingFlags.NonPublic | BindingFlags.Static);

                if (method == null)
                {
                    throw new InvalidOperationException("CompileGetter method not found.");
                }

                var genericMethod = method.MakeGenericMethod(source.GetType());
                cachedDelegate = (Delegate)genericMethod.Invoke(null, [path.Split('.')])!;
                delegates[path] = cachedDelegate;
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
            var rawValue = cachedDelegate.DynamicInvoke(source);
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
                IEnumerable<int> intList => new ComparableList<int>(intList, ComparableInt.Equals),
                IEnumerable<string> stringList => new ComparableList<string>(stringList, ComparableString.Equals),
                IEnumerable<IHaveModData> otherList => new ComparableList<IHaveModData>(
                    otherList,
                    (modData, expression) => ComparableModel.Equals(modData, this.TryGetValue, expression)),
                IDictionary
                {
                    Values: IEnumerable<int> intDict,
                } => new ComparableList<int>(intDict, ComparableInt.Equals),
                IDictionary
                {
                    Values: IEnumerable<string> stringDict,
                } => new ComparableList<string>(stringDict, ComparableString.Equals),
                IDictionary
                {
                    Values: IEnumerable<IHaveModData> otherDict,
                } => new ComparableList<IHaveModData>(
                    otherDict,
                    (modData, expression) => ComparableModel.Equals(modData, this.TryGetValue, expression)),
                IHaveModData otherValue => new ComparableModel(otherValue, this.TryGetValue),
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
            // Check if member is a method call
            if (member.EndsWith("()", StringComparison.OrdinalIgnoreCase))
            {
                var methodName = member[..^2];
                var methodInfo = body.Type.GetMethod(methodName);
                if (methodInfo == null || methodInfo.GetParameters().Length != 0)
                {
                    throw new NotSupportedException($"Method '{methodName}' is not supported.");
                }

                body = Expression.Call(body, methodInfo);
                continue;
            }

            // Check if member is a dictionary with constant key type
            var bracketStart = member.IndexOf('[', StringComparison.OrdinalIgnoreCase);
            var bracketEnd = member.IndexOf(']', StringComparison.OrdinalIgnoreCase);
            if (bracketStart != -1 && bracketEnd != -1 && bracketEnd > bracketStart)
            {
                var dictionaryName = member[..bracketStart];
                var key = member.Substring(bracketStart + 1, bracketEnd - bracketStart - 1);

                var dictionaryExpression = Expression.PropertyOrField(body, dictionaryName);
                if (dictionaryExpression.Type.IsGenericType
                    && dictionaryExpression.Type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    var keyType = dictionaryExpression.Type.GetGenericArguments()[0];
                    var itemProperty = dictionaryExpression.Type.GetProperty("Item")!;
                    var keyExpression =
                        Expression.Constant(Convert.ChangeType(key, keyType, CultureInfo.InvariantCulture));

                    body = Expression.Property(dictionaryExpression, itemProperty, keyExpression);
                    continue;
                }

                throw new NotSupportedException($"Dictionary access {member} is not valid for type '{body.Type}'.");
            }

            // Default to any property or field
            body = Expression.PropertyOrField(body, member);
        }

        body = Expression.Convert(body, typeof(object));
        var lambda = Expression.Lambda<Func<T, object>>(body, parameterExpression);
        return lambda.Compile();
    }
}