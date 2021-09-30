namespace XSPlus
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Common.Services;
    using Services;

    /// <summary>
    /// Adds methods to handle feature instances.
    /// </summary>
    internal static class ServiceManagerExtensions
    {
        private static readonly HashSet<string> ActivatedFeatures = new();

        /// <summary>Allows items containing particular mod data to have feature enabled.</summary>
        /// <param name="serviceManager">The service manager.</param>
        /// <param name="featureName">The feature to enable.</param>
        /// <param name="key">The mod data key to enable feature for.</param>
        /// <param name="value">The mod data value to enable feature for.</param>
        /// <param name="param">The parameter value to store for this feature.</param>
        /// <typeparam name="T">The parameter type to store for this feature.</typeparam>
        /// <exception cref="InvalidOperationException">When a feature is unknown.</exception>
        [SuppressMessage("ReSharper", "HeapView.PossibleBoxingAllocation", Justification = "Support dynamic param types for API access.")]
        public static void EnableFeatureWithModData<T>(this ServiceManager serviceManager, string featureName, string key, string value, T param)
        {
            var feature = serviceManager.RequestService<BaseFeature>(featureName);
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
            }
        }

        /// <summary>Checks if a feature is configured as globally enabled.</summary>
        /// <param name="serviceManager">The service manager.</param>
        /// <param name="featureName">The feature to check.</param>
        /// <returns>Returns true if feature is enabled globally.</returns>
        public static bool IsFeatureEnabledGlobally(this ServiceManager serviceManager, string featureName)
        {
            var modConfigService = serviceManager.RequestService<ModConfigService>();
            return modConfigService.ModConfig.Global.TryGetValue(featureName, out var option) && option;
        }

        /// <summary>Allow feature to add its events and apply patches.</summary>
        /// <param name="serviceManager">The service manager.</param>
        /// <param name="featureName">The feature to enable.</param>
        /// <exception cref="InvalidOperationException">When a feature is unknown.</exception>
        public static void ActivateFeature(this ServiceManager serviceManager, string featureName)
        {
            var feature = serviceManager.RequestService<BaseFeature>(featureName);
            if (feature is null)
            {
                throw new InvalidOperationException($"Unknown feature {featureName}");
            }

            if (ServiceManagerExtensions.ActivatedFeatures.Contains(featureName))
            {
                return;
            }

            ServiceManagerExtensions.ActivatedFeatures.Add(featureName);
            feature.Activate();
        }

        /// <summary>Allow feature to remove its events and reverse patches.</summary>
        /// <param name="serviceManager">The service manager.</param>
        /// <param name="featureName">The feature to disable.</param>
        /// <exception cref="InvalidOperationException">When a feature is unknown.</exception>
        public static void DeactivateFeature(this ServiceManager serviceManager, string featureName)
        {
            var feature = serviceManager.RequestService<BaseFeature>(featureName);
            if (feature is null)
            {
                throw new InvalidOperationException($"Unknown feature {featureName}");
            }

            if (!ServiceManagerExtensions.ActivatedFeatures.Contains(featureName))
            {
                return;
            }

            ServiceManagerExtensions.ActivatedFeatures.Remove(featureName);
            feature.Deactivate();
        }

        /// <summary>Calls all feature activation methods for enabled/default features.</summary>
        /// <param name="serviceManager">The service manager.</param>
        public static void ActivateFeatures(this ServiceManager serviceManager)
        {
            var modConfigService = serviceManager.RequestService<ModConfigService>();
            foreach (var feature in serviceManager.RequestServices<BaseFeature>())
            {
                // Skip any feature that is globally disabled
                if (modConfigService.ModConfig.Global.TryGetValue(feature.FeatureName, out var option) && !option)
                {
                    continue;
                }

                serviceManager.ActivateFeature(feature.FeatureName);
            }
        }
    }
}