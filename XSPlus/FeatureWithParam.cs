namespace XSPlus
{
    using System.Collections.Generic;
    using Common.Services;
    using StardewValley;

    /// <inheritdoc />
    internal abstract class FeatureWithParam<TParam> : BaseFeature
    {
        /// <summary>Initializes a new instance of the <see cref="FeatureWithParam{TParam}" /> class.</summary>
        /// <param name="featureName">The name of the feature used for config/API.</param>
        /// <param name="serviceManager">Service manager to request shared services.</param>
        internal FeatureWithParam(string featureName, ServiceManager serviceManager)
            : base(featureName, serviceManager)
        {
        }

        private IDictionary<KeyValuePair<string, string>, TParam> Values { get; } = new Dictionary<KeyValuePair<string, string>, TParam>();

        /// <summary>Stores feature parameter value for items containing ModData.</summary>
        /// <param name="key">The mod data key to enable feature for.</param>
        /// <param name="value">The mod data value to enable feature for.</param>
        /// <param name="param">The parameter value to store for this feature.</param>
        public void StoreValueWithModData(string key, string value, TParam param)
        {
            var modDataKey = new KeyValuePair<string, string>(key, value);
            if (this.Values.ContainsKey(modDataKey))
            {
                this.Values[modDataKey] = param;
            }
            else
            {
                this.Values.Add(modDataKey, param);
            }
        }

        /// <summary>Attempts to return the stored value for item based on ModData.</summary>
        /// <param name="item">The item to test ModData against.</param>
        /// <param name="param">The stored value for this item.</param>
        /// <returns>Returns true if there is a stored value for this item.</returns>
        internal virtual bool TryGetValueForItem(Item item, out TParam param)
        {
            foreach (var modData in this.Values)
            {
                if (!item.modData.TryGetValue(modData.Key.Key, out var value) || value != modData.Key.Value)
                {
                    continue;
                }

                param = modData.Value;
                return true;
            }

            param = default;
            return false;
        }
    }
}