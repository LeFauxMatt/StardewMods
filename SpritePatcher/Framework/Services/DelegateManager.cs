namespace StardewMods.SpritePatcher.Framework.Services;

using System.Collections;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Xna.Framework;
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
        value = null;
        if (string.IsNullOrWhiteSpace(path))
        {
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
                cachedDelegate = DelegateManager.CompileGetter(type, path);
                delegates[path] = cachedDelegate;
            }
            catch (Exception e)
            {
                this.Log.TraceOnce("Failed to compile getter on {0} for path '{1}'.\nError: {2}", type.Name, path, e.Message);
                return false;
            }
        }

        try
        {
            var rawValue = cachedDelegate.DynamicInvoke(source);
            if (rawValue is null)
            {
                return false;
            }

            value = this.ConvertToComparable(cachedDelegate, source);
            return value is not null;
        }
        catch (Exception e)
        {
            this.Log.TraceOnce("Failed to retrieve value on {0} for path '{1}'.\nError: {2}", type.Name, path, e.Message);
            return false;
        }
    }

    private static Delegate CompileGetter(Type sourceType, string path)
    {
        var parts = path.Split('.');
        var parameter = Expression.Parameter(sourceType, "source");
        var body = DelegateManager.BuildExpressionTree(parameter, parts);
        var lambdaType = typeof(Func<,>).MakeGenericType(sourceType, typeof(object));
        return Expression.Lambda(lambdaType, body, parameter).Compile();
    }

    private static Expression BuildExpressionTree(Expression parameter, IEnumerable<string> parts)
    {
        var body = parameter;
        foreach (var member in parts)
        {
            body = member.EndsWith("()", StringComparison.OrdinalIgnoreCase)
                ? DelegateManager.ProcessMethodCall(body, member)
                : DelegateManager.ProcessMemberAccess(body, member);
        }

        return Expression.Convert(body, typeof(object));
    }

    private static Expression ProcessMethodCall(Expression body, string member)
    {
        var methodName = member[..^2];
        var methodInfo = body.Type.GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (methodInfo == null || methodInfo.GetParameters().Length != 0)
        {
            throw new NotSupportedException($"Method '{methodName}' is not supported.");
        }

        return Expression.Call(body, methodInfo);
    }

    private static Expression ProcessMemberAccess(Expression body, string member)
    {
        var bracketStart = member.IndexOf('[', StringComparison.OrdinalIgnoreCase);
        var bracketEnd = member.IndexOf(']', StringComparison.OrdinalIgnoreCase);
        if (bracketStart == -1 || bracketEnd == -1 || bracketEnd <= bracketStart)
        {
            return Expression.PropertyOrField(body, member);
        }

        var dictionaryName = member[..bracketStart];
        var key = member.Substring(bracketStart + 1, bracketEnd - bracketStart - 1);

        var dictionaryExpression = Expression.PropertyOrField(body, dictionaryName);
        if (dictionaryExpression.Type.IsGenericType
            && dictionaryExpression.Type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            var keyType = dictionaryExpression.Type.GetGenericArguments()[0];
            var itemProperty = dictionaryExpression.Type.GetProperty("Item")!;
            var keyExpression = Expression.Constant(Convert.ChangeType(key, keyType, CultureInfo.InvariantCulture));

            return Expression.Property(dictionaryExpression, itemProperty, keyExpression);
        }

        throw new NotSupportedException($"Dictionary access {member} is not valid for type '{body.Type}'.");
    }

    private IEquatable<string>? ConvertToComparable(Delegate cachedDelegate, IHaveModData source)
    {
        var rawValue = cachedDelegate.DynamicInvoke(source);
        if (rawValue is null)
        {
            return null;
        }

        return rawValue switch
        {
            Color => new ComparableColor(CreateGetter<Color>()),
            string => new ComparableString(CreateGetter<string>()),
            int => new ComparableInt(CreateGetter<int>()),
            bool => new ComparableBool(CreateGetter<bool>()),
            Enum enumValue =>
                new ComparableString(
                    () => Enum.GetName(enumValue.GetType(), cachedDelegate.DynamicInvoke(source)!)!),
            IEnumerable<int> => new ComparableList<int>(CreateGetter<IEnumerable<int>>(), ComparableInt.Equals),
            IEnumerable<string> =>
                new ComparableList<string>(CreateGetter<IEnumerable<string>>(), ComparableString.Equals),
            IEnumerable<IHaveModData> => new ComparableList<IHaveModData>(
                CreateGetter<IEnumerable<IHaveModData>>(),
                (modData, expression) => ComparableModel.Equals(modData, this.TryGetValue, expression)),
            IDictionary dictionary => dictionary.Values switch
            {
                IEnumerable<int> =>
                    new ComparableList<int>(CreateDictionaryGetter<int>(), ComparableInt.Equals),
                IEnumerable<string> => new ComparableList<string>(
                    CreateDictionaryGetter<string>(),
                    ComparableString.Equals),
                IEnumerable<IHaveModData> => new ComparableList<IHaveModData>(
                    CreateDictionaryGetter<IHaveModData>(),
                    (modData, expression) => ComparableModel.Equals(modData, this.TryGetValue, expression)),
                _ => null,
            },
            IHaveModData => new ComparableModel(CreateGetter<IHaveModData>(), this.TryGetValue),
            _ => null,
        };

        // Lambda to get the live value
        Func<T> CreateGetter<T>() => () => (T)cachedDelegate.DynamicInvoke(source)!;

        // Lambda to get the live dictionary values
        Func<IEnumerable<T>> CreateDictionaryGetter<T>() =>
            () => (cachedDelegate.DynamicInvoke(source) as IDictionary)?.Values.Cast<T>() ?? Enumerable.Empty<T>();
    }
}