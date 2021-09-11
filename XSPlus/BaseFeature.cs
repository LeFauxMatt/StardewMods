using System.Collections.Generic;
using HarmonyLib;
using StardewModdingAPI;
using StardewValley;

namespace XSPlus
{
    internal abstract class BaseFeature
    {
        protected readonly IModHelper Helper;
        protected readonly IMonitor Monitor;
        protected readonly Harmony Harmony;
        private readonly string _featureName;
        private readonly IDictionary<string, bool> _enabledByModData = new Dictionary<string, bool>();
        private bool _isDisabled;
        public bool IsDisabled
        {
            get => _isDisabled;
            set
            {
                if (_isDisabled == value)
                    return;
                _isDisabled = value;
                if (_isDisabled)
                    DisableFeature();
                else
                    EnableFeature();
            }
        }
        protected BaseFeature(string featureName, IModHelper helper, IMonitor monitor, Harmony harmony)
        {
            _featureName = featureName;
            Helper = helper;
            Monitor = monitor;
            Harmony = harmony;
            _isDisabled = true;
        }
        protected abstract void EnableFeature();
        protected abstract void DisableFeature();
        public virtual bool IsEnabled(Item item)
        {
            // Globally disabled
            if (_isDisabled)
                return false;
            // Individually Enabled/Disabled
            foreach (var enabledByModData in _enabledByModData)
            {
                var enabledKey = enabledByModData.Key.Split('=')[0];
                var enabledValue = enabledByModData.Key.Split('=')[1];
                if (item.modData.TryGetValue(enabledKey, out var value) && value == enabledValue)
                    return enabledByModData.Value;
            }
            // Global enabled or by default disabled
            return XSPlus.Config.Global.TryGetValue(_featureName, out var globalOverride) && globalOverride;
        }
        public void EnableWithModData(string key, string value, bool enable)
        {
            var modDataKey = $"{key}={value}";
            if (_enabledByModData.ContainsKey(modDataKey))
                _enabledByModData[modDataKey] = enable;
            else
                _enabledByModData.Add(modDataKey, enable);
        }
    }
}