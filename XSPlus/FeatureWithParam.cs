using System.Collections.Generic;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace XSPlus
{
    internal abstract class FeatureWithParam<TParam> : BaseFeature
    {
        private readonly IDictionary<string, TParam> _values = new Dictionary<string, TParam>();
        protected FeatureWithParam(string featureName, IModHelper helper, IMonitor monitor, Harmony harmony) : base(featureName, helper, monitor, harmony)
        {
        }
        public void EnableWithModData<T>(string key, string value, bool enable, T param)
        {
            EnableWithModData(key, value, enable);
            var modDataKey = $"{key}={value}";
            if (_values.ContainsKey(modDataKey))
            {
                if (param is null || param is not TParam tParam)
                    _values.Remove(modDataKey);
                else
                    _values[modDataKey] = tParam;
            }
            else if (param is TParam tParam)
                _values.Add(modDataKey, tParam);
        }
        protected bool TryGetValue(Item item, out TParam param)
        {
            param = default;
            foreach (var modDataValue in _values)
            {
                var enabledKey = modDataValue.Key.Split('=')[0];
                var enabledValue = modDataValue.Key.Split('=')[1];
                if (!item.modData.TryGetValue(enabledKey, out var value) || value != enabledValue)
                    continue;
                param = modDataValue.Value;
                return true;
            }
            return false;
        }
    }
}