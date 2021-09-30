namespace XSPlus
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using HarmonyLib;
    using Services;
    using StardewModdingAPI;

    /// <summary>Manages all feature instances.</summary>
    internal class FeatureManager
    {
        private static FeatureManager? Instance;
        private readonly ServiceManager _serviceManager;
        private readonly ModConfigService _modConfigService;
        private readonly IDictionary<string, BaseFeature> _features = new Dictionary<string, BaseFeature>();
        private readonly HashSet<string> _activatedFeatures = new();

        private FeatureManager(ServiceManager serviceManager, ModConfigService modConfigService)
        {
            this._serviceManager = serviceManager;
            this._modConfigService = modConfigService;
        }

        /// <summary>Returns and creates if needed an instance of the <see cref="FeatureManager"/> class.</summary>
        /// <param name="serviceManager">Service manager to request shared services.</param>
        /// <returns>An instance of <see cref="FeatureManager"/> class.</returns>
        public static FeatureManager GetSingleton(ServiceManager serviceManager)
        {
            var modConfigService = serviceManager.RequestService<ModConfigService>("ModConfig");
            return FeatureManager.Instance ??= new FeatureManager(serviceManager, modConfigService);
        }

        /// <summary>Allows items containing particular mod data to have feature enabled.</summary>
        /// <param name="featureName">The feature to enable.</param>
        /// <param name="key">The mod data key to enable feature for.</param>
        /// <param name="value">The mod data value to enable feature for.</param>
        /// <param name="param">The parameter value to store for this feature.</param>
        /// <typeparam name="T">The parameter type to store for this feature.</typeparam>
        /// <exception cref="InvalidOperationException">When a feature is unknown.</exception>
        [SuppressMessage("ReSharper", "HeapView.PossibleBoxingAllocation", Justification = "Support dynamic param types for API access.")]
        public static void EnableFeatureWithModData<T>(string featureName, string key, string value, T param)
        {
            if (!FeatureManager.Instance!._features.TryGetValue(featureName, out var feature))
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
        /// <param name="featureName">The feature to check.</param>
        /// <returns>Returns true if feature is enabled globally.</returns>
        public static bool IsFeatureEnabledGlobally(string featureName)
        {
            return FeatureManager.Instance!._modConfigService.ModConfig.Global.TryGetValue(featureName, out bool option) && option;
        }

        /// <summary>Allow feature to add its events and apply patches.</summary>
        /// <param name="featureName">The feature to enable.</param>
        /// <exception cref="InvalidOperationException">When a feature is unknown.</exception>
        public static void ActivateFeature(string featureName)
        {
            if (!FeatureManager.Instance!._features.TryGetValue(featureName, out var feature))
            {
                throw new InvalidOperationException($"Unknown feature {featureName}");
            }

            if (FeatureManager.Instance._activatedFeatures.Contains(featureName))
            {
                return;
            }

            FeatureManager.Instance._activatedFeatures.Add(featureName);
            feature.Activate();
        }

        /// <summary>Allow feature to remove its events and reverse patches.</summary>
        /// <param name="featureName">The feature to disable.</param>
        /// <exception cref="InvalidOperationException">When a feature is unknown.</exception>
        public static void DeactivateFeature(string featureName)
        {
            if (!FeatureManager.Instance!._features.TryGetValue(featureName, out var feature))
            {
                throw new InvalidOperationException($"Unknown feature {featureName}");
            }

            if (!FeatureManager.Instance._activatedFeatures.Contains(featureName))
            {
                return;
            }

            FeatureManager.Instance._activatedFeatures.Remove(featureName);
            feature.Deactivate();
        }

        /// <summary>Calls all feature activation methods for enabled/default features.</summary>
        public void ActivateFeatures()
        {
            foreach (string featureName in this._features.Keys)
            {
                // Skip any feature that is globally disabled
                if (this._modConfigService.ModConfig.Global.TryGetValue(featureName, out bool option) && !option)
                {
                    continue;
                }

                FeatureManager.ActivateFeature(featureName);
            }
        }

        /// <summary>Add to collection of active feature instances.</summary>
        /// <typeparam name="TFeatureType">Type of feature to add.</typeparam>
        public void AddSingleton<TFeatureType>()
        {
            var feature = (BaseFeature)typeof(TFeatureType).GetMethod("GetSingleton", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object[] { this._serviceManager });
            if (feature != null)
            {
                this._features.Add(feature.FeatureName, feature);
            }
        }
    }
}