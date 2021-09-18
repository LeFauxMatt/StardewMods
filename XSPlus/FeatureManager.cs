namespace XSPlus
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using HarmonyLib;
    using StardewModdingAPI;

    /// <summary>Manages all feature instances.</summary>
    internal class FeatureManager
    {
        private static FeatureManager Instance;
        private readonly IDictionary<string, BaseFeature> Features = new Dictionary<string, BaseFeature>();
        private readonly HashSet<string> ActivatedFeatures = new();
        private readonly IModHelper Helper;
        private readonly Harmony Harmony;
        private readonly IDictionary<string, bool> Global;

        private FeatureManager(IModHelper helper, Harmony harmony, IDictionary<string, bool> global)
        {
            this.Helper = helper;
            this.Harmony = harmony;
            this.Global = global;
        }

        /// <summary>Returns and optionally creates the <see cref="FeatureManager"/> instance.</summary>
        /// <param name="modHelper">SMAPIs APIs for events, content, input, etc.</param>
        /// <param name="harmony">The Harmony instance for patching the games internal code.</param>
        /// <param name="global">Globally enabled/disabled features from player config.</param>
        /// <returns>An instance of <see cref="FeatureManager"/> class.</returns>
        public static FeatureManager Init(IModHelper modHelper, Harmony harmony, IDictionary<string, bool> global)
        {
            return FeatureManager.Instance ??= new FeatureManager(modHelper, harmony, global);
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
            if (!FeatureManager.Instance.Features.TryGetValue(featureName, out BaseFeature feature))
            {
                return;
            }

            switch (param)
            {
                case bool bParam:
                    feature.EnableWithModData(key, value, bParam);
                    break;
                case int iParam when feature is FeatureWithParam<int> iFeature:
                    iFeature.StoreValueWithModData(key, value, iParam);
                    feature.EnableWithModData(key, value, true);
                    break;
                case string sParam when feature is FeatureWithParam<string> sFeature:
                    sFeature.StoreValueWithModData(key, value, sParam);
                    feature.EnableWithModData(key, value, true);
                    break;
            }
        }

        /// <summary>Checks if a feature is configured as globally enabled.</summary>
        /// <param name="featureName">The feature to check.</param>
        /// <returns>Returns true if feature is enabled globally.</returns>
        public static bool IsFeatureEnabledGlobally(string featureName)
        {
            return FeatureManager.Instance.Global.TryGetValue(featureName, out bool option) && option;
        }

        /// <summary>Allow feature to add its events and apply patches.</summary>
        /// <param name="featureName">The feature to enable.</param>
        /// <exception cref="InvalidOperationException">When a feature is unknown.</exception>
        public static void ActivateFeature(string featureName)
        {
            if (!FeatureManager.Instance.Features.TryGetValue(featureName, out BaseFeature feature))
            {
                throw new InvalidOperationException($"Unknown feature {featureName}");
            }

            if (FeatureManager.Instance.ActivatedFeatures.Contains(featureName))
            {
                return;
            }

            FeatureManager.Instance.ActivatedFeatures.Add(featureName);
            feature.Activate(FeatureManager.Instance.Helper.Events, FeatureManager.Instance.Harmony);
        }

        /// <summary>Allow feature to remove its events and reverse patches.</summary>
        /// <param name="featureName">The feature to disable.</param>
        /// <exception cref="InvalidOperationException">When a feature is unknown.</exception>
        public static void DeactivateFeature(string featureName)
        {
            if (!FeatureManager.Instance.Features.TryGetValue(featureName, out BaseFeature feature))
            {
                throw new InvalidOperationException($"Unknown feature {featureName}");
            }

            if (!FeatureManager.Instance.ActivatedFeatures.Contains(featureName))
            {
                return;
            }

            FeatureManager.Instance.ActivatedFeatures.Remove(featureName);
            feature.Deactivate(FeatureManager.Instance.Helper.Events, FeatureManager.Instance.Harmony);
        }

        /// <summary>Calls all feature activation methods for enabled/default features.</summary>
        public void ActivateFeatures()
        {
            foreach (string featureName in this.Features.Keys)
            {
                // Skip any feature that is globally disabled
                if (this.Global.TryGetValue(featureName, out bool option) && !option)
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
            this.Features.Add(feature.FeatureName, feature);
        }
    }
}