namespace XSPlus
{
    using System.Collections.Generic;
    using StardewValley;

    /// <inheritdoc />
    internal abstract class FeatureWithParam<TParam> : BaseFeature
    {
        private readonly IDictionary<KeyValuePair<string, string>, TParam> _values = new Dictionary<KeyValuePair<string, string>, TParam>();

        /// <summary>Initializes a new instance of the <see cref="FeatureWithParam{TParam}"/> class.</summary>
        /// <param name="featureName">The name of the feature used for config/API.</param>
        internal FeatureWithParam(string featureName)
            : base(featureName)
        {
        }

        /// <summary>Stores feature parameter value for items containing ModData.</summary>
        /// <param name="key">The mod data key to enable feature for.</param>
        /// <param name="value">The mod data value to enable feature for.</param>
        /// <param name="param">The parameter value to store for this feature.</param>
        public void StoreValueWithModData(string key, string value, TParam param)
        {
            var modDataKey = new KeyValuePair<string, string>(key, value);
            if (this._values.ContainsKey(modDataKey))
            {
                this._values[modDataKey] = param;
            }
            else
            {
                this._values.Add(modDataKey, param);
            }
        }

        /// <summary>Attempts to return the stored value for item based on ModData.</summary>
        /// <param name="item">The item to test ModData against.</param>
        /// <param name="param">The stored value for this item.</param>
        /// <returns>Returns true if there is a stored value for this item.</returns>
        protected virtual bool TryGetValueForItem(Item item, out TParam param)
        {
            foreach (var modData in this._values)
            {
                if (!item.modData.TryGetValue(modData.Key.Key, out string value) || value != modData.Key.Value)
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