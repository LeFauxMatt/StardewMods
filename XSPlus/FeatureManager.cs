namespace XSPlus
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using HarmonyLib;
    using Services;
    using StardewModdingAPI;

    /// <summary>Manages all feature instances.</summary>
    internal class FeatureManager
    {
        private static FeatureManager? Instance;
        private readonly IDictionary<string, BaseFeature> _features = new Dictionary<string, BaseFeature>();
        private readonly HashSet<string> _activatedFeatures = new();
        private readonly IModHelper _helper;
        private readonly Harmony _harmony;
        private readonly ModConfigService _modConfigService;

        private FeatureManager(IModHelper helper, Harmony harmony, ModConfigService modConfigService)
        {
            this._helper = helper;
            this._harmony = harmony;
            this._modConfigService = modConfigService;
        }

        /// <summary>Returns and creates if needed an instance of the <see cref="FeatureManager"/> class.</summary>
        /// <param name="modHelper">SMAPIs APIs for events, content, input, etc.</param>
        /// <param name="harmony">The Harmony instance for patching the games internal code.</param>
        /// <param name="modConfigService">Service to handle read/write to ModConfig.</param>
        /// <returns>An instance of <see cref="FeatureManager"/> class.</returns>
        public static FeatureManager Init(IModHelper modHelper, Harmony harmony, ModConfigService modConfigService)
        {
            return FeatureManager.Instance ??= new FeatureManager(modHelper, harmony, modConfigService);
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
            if (!FeatureManager.Instance!._features.TryGetValue(featureName, out BaseFeature feature))
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
            if (!FeatureManager.Instance!._features.TryGetValue(featureName, out BaseFeature feature))
            {
                throw new InvalidOperationException($"Unknown feature {featureName}");
            }

            if (FeatureManager.Instance._activatedFeatures.Contains(featureName))
            {
                return;
            }

            FeatureManager.Instance._activatedFeatures.Add(featureName);
            feature.Activate(FeatureManager.Instance._helper.Events, FeatureManager.Instance._harmony);
        }

        /// <summary>Allow feature to remove its events and reverse patches.</summary>
        /// <param name="featureName">The feature to disable.</param>
        /// <exception cref="InvalidOperationException">When a feature is unknown.</exception>
        public static void DeactivateFeature(string featureName)
        {
            if (!FeatureManager.Instance!._features.TryGetValue(featureName, out BaseFeature feature))
            {
                throw new InvalidOperationException($"Unknown feature {featureName}");
            }

            if (!FeatureManager.Instance._activatedFeatures.Contains(featureName))
            {
                return;
            }

            FeatureManager.Instance._activatedFeatures.Remove(featureName);
            feature.Deactivate(FeatureManager.Instance._helper.Events, FeatureManager.Instance._harmony);
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
        /// <param name="feature">Instance of feature to add.</param>
        public void AddFeature(BaseFeature feature)
        {
            this._features.Add(feature.FeatureName, feature);
        }
    }
}