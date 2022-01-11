namespace XSPlus;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Common.Helpers;
using Services;
using StardewValley.Objects;

/// <summary>
///     Adds methods to handle feature instances.
/// </summary>
internal static class Extensions
{
    private static readonly HashSet<string> ActivatedFeatures = new();

    /// <summary>Allows items containing particular mod data to have feature enabled.</summary>
    /// <param name="serviceLocator">The service manager.</param>
    /// <param name="featureName">The feature to enable.</param>
    /// <param name="key">The mod data key to enable feature for.</param>
    /// <param name="value">The mod data value to enable feature for.</param>
    /// <param name="param">The parameter value to store for this feature.</param>
    /// <typeparam name="T">The parameter type to store for this feature.</typeparam>
    /// <exception cref="InvalidOperationException">When a feature is unknown.</exception>
    [SuppressMessage("ReSharper", "HeapView.PossibleBoxingAllocation", Justification = "Support dynamic param types for API access.")]
    public static void EnableFeatureWithModData<T>(this ServiceLocator serviceLocator, string featureName, string key, string value, T param)
    {
        if (!BaseFeature.ValidFeatures.Contains(featureName))
        {
            return;
        }

        var feature = serviceLocator.GetByName<BaseFeature>(featureName);
        if (feature is null)
        {
            return;
        }

        switch (param)
        {
            case bool bParam:
                feature.EnableWithModData(key, value, bParam);
                break;
            case float fParam when feature is FeatureWithParam<float> fFeature:
                fFeature.StoreValueWithModData(key, value, fParam);
                feature.EnableWithModData(key, value, true);
                break;
            case int iParam when feature is FeatureWithParam<int> iFeature:
                iFeature.StoreValueWithModData(key, value, iParam);
                feature.EnableWithModData(key, value, true);
                break;
            case string sParam when feature is FeatureWithParam<string> sFeature:
                sFeature.StoreValueWithModData(key, value, sParam);
                feature.EnableWithModData(key, value, true);
                break;
            case HashSet<string> hParam when feature is FeatureWithParam<HashSet<string>> hFeature:
                hFeature.StoreValueWithModData(key, value, hParam);
                feature.EnableWithModData(key, value, true);
                break;
            case Dictionary<string, bool> dParam when feature is FeatureWithParam<Dictionary<string, bool>> dFeature:
                dFeature.StoreValueWithModData(key, value, dParam);
                feature.EnableWithModData(key, value, true);
                break;
            case Tuple<int, int, int> tParam when feature is FeatureWithParam<Tuple<int, int, int>> tFeature:
                tFeature.StoreValueWithModData(key, value, tParam);
                feature.EnableWithModData(key, value, true);
                break;
        }
    }

    /// <summary>Checks if a feature is configured as globally enabled.</summary>
    /// <param name="serviceLocator">The service manager.</param>
    /// <param name="featureName">The feature to check.</param>
    /// <returns>Returns true if feature is enabled globally.</returns>
    public static bool IsFeatureEnabledGlobally(this ServiceLocator serviceLocator, string featureName)
    {
        var modConfigService = serviceLocator.GetInstance<ModConfigService>();
        return modConfigService.ModConfig.Global.TryGetValue(featureName, out var option) && option;
    }

    /// <summary>Allow feature to add its events and apply patches.</summary>
    /// <param name="serviceLocator">The service manager.</param>
    /// <param name="featureName">The feature to enable.</param>
    /// <exception cref="InvalidOperationException">When a feature is unknown.</exception>
    public static void ActivateFeature(this ServiceLocator serviceLocator, string featureName)
    {
        var feature = serviceLocator.GetByName<BaseFeature>(featureName);
        if (feature is null)
        {
            throw new InvalidOperationException($"Unknown feature {featureName}");
        }

        if (Extensions.ActivatedFeatures.Contains(featureName))
        {
            return;
        }

        Extensions.ActivatedFeatures.Add(featureName);
        feature.Activate();
    }

    /// <summary>Allow feature to remove its events and reverse patches.</summary>
    /// <param name="serviceLocator">The service manager.</param>
    /// <param name="featureName">The feature to disable.</param>
    /// <exception cref="InvalidOperationException">When a feature is unknown.</exception>
    public static void DeactivateFeature(this ServiceLocator serviceLocator, string featureName)
    {
        var feature = serviceLocator.GetByName<BaseFeature>(featureName);
        if (feature is null)
        {
            throw new InvalidOperationException($"Unknown feature {featureName}");
        }

        if (!Extensions.ActivatedFeatures.Contains(featureName))
        {
            return;
        }

        Extensions.ActivatedFeatures.Remove(featureName);
        feature.Deactivate();
    }

    /// <summary>Calls all feature activation methods for enabled/default features.</summary>
    /// <param name="serviceLocator">The service manager.</param>
    public static void ActivateFeatures(this ServiceLocator serviceLocator)
    {
        var modConfigService = serviceLocator.GetInstance<ModConfigService>();
        foreach (var feature in serviceLocator.GetAll<BaseFeature>())
        {
            // Skip any feature that is globally disabled
            if (modConfigService.ModConfig.Global.TryGetValue(feature.ServiceName, out var option) && !option)
            {
                continue;
            }

            serviceLocator.ActivateFeature(feature.ServiceName);
        }
    }

    public static string GetFilterItems(this Chest chest)
    {
        return chest.modData.TryGetValue($"{XSPlus.ModPrefix}/FilterItems", out var filterItems)
            ? filterItems.Replace("#", string.Empty)
            : string.Empty;
    }

    public static void SetFilterItems(this Chest chest, string value)
    {
        value = value.Replace("#", string.Empty);
        if (string.IsNullOrWhiteSpace(value))
        {
            Log.Trace($"Removing item filters for Chest {chest.DisplayName}");
            chest.modData.Remove($"{XSPlus.ModPrefix}/FilterItems");
        }
        else
        {
            Log.Trace($"Updating item filters for Chest {chest.DisplayName}");
            chest.modData[$"{XSPlus.ModPrefix}/FilterItems"] = value;
        }
    }
}