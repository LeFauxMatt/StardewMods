namespace XSPlus
{
    using System.Collections.Generic;
    using CommonHarmony.Services;
    using Services;
    using StardewValley;

    /// <summary>
    ///     Encapsulates logic for features added by this mod.
    /// </summary>
    internal abstract class BaseFeature : BaseService
    {
        private readonly IDictionary<KeyValuePair<string, string>, bool> _enabledByModData = new Dictionary<KeyValuePair<string, string>, bool>();
        private readonly ModConfigService _modConfigService;

        /// <summary>Initializes a new instance of the <see cref="BaseFeature" /> class.</summary>
        /// <param name="featureName">The name of the feature used for config/API.</param>
        /// <param name="modConfigService">Service to handle read/write to <see cref="Models.ModConfig" />.</param>
        private protected BaseFeature(string featureName, ModConfigService modConfigService)
            : base(featureName)
        {
            this.FeatureName = featureName;
            this._modConfigService = modConfigService;
        }

        /// <summary>Gets the name of the feature used for config/API.</summary>
        public string FeatureName { get; }

        /// <summary>Add events and apply patches used to enable this feature.</summary>
        public abstract void Activate();

        /// <summary>Disable events and reverse patches used by this feature.</summary>
        public abstract void Deactivate();

        /// <summary>Allows items containing particular mod data to have feature enabled.</summary>
        /// <param name="key">The mod data key to enable feature for.</param>
        /// <param name="value">The mod data value to enable feature for.</param>
        /// <param name="enable">Whether to enable or disable this feature.</param>
        public void EnableWithModData(string key, string value, bool enable)
        {
            var modDataKey = new KeyValuePair<string, string>(key, value);

            if (this._enabledByModData.ContainsKey(modDataKey))
            {
                this._enabledByModData[modDataKey] = enable;
            }
            else
            {
                this._enabledByModData.Add(modDataKey, enable);
            }
        }

        /// <summary>Checks whether a feature is currently enabled for an item.</summary>
        /// <param name="item">The item to check if it supports this feature.</param>
        /// <returns>Returns true if the feature is currently enabled for the item.</returns>
        internal virtual bool IsEnabledForItem(Item item)
        {
            var isEnabledByModData = this._modConfigService.ModConfig.Global.TryGetValue(this.FeatureName, out var option) && option;

            foreach (var modData in this._enabledByModData)
            {
                if (!item.modData.TryGetValue(modData.Key.Key, out var value) || value != modData.Key.Value)
                {
                    continue;
                }

                // Disabled by ModData
                if (!modData.Value)
                {
                    return false;
                }

                isEnabledByModData = true;
            }

            return isEnabledByModData;
        }
    }
}