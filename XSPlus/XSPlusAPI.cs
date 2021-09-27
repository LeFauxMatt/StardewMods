namespace XSPlus
{
    using System.Collections.Generic;
    using Common.Integrations.XSPlus;

    /// <inheritdoc />
    public class XSPlusAPI : IXSPlusAPI
    {
        /// <inheritdoc/>
        public void EnableWithModData(string featureName, string key, string value, bool param)
        {
            FeatureManager.EnableFeatureWithModData(featureName, key, value, param);
        }

        /// <inheritdoc/>
        public void EnableWithModData(string featureName, string key, string value, float param)
        {
            FeatureManager.EnableFeatureWithModData(featureName, key, value, param);
        }

        /// <inheritdoc/>
        public void EnableWithModData(string featureName, string key, string value, int param)
        {
            FeatureManager.EnableFeatureWithModData(featureName, key, value, param);
        }

        /// <inheritdoc/>
        public void EnableWithModData(string featureName, string key, string value, string param)
        {
            FeatureManager.EnableFeatureWithModData(featureName, key, value, param);
        }

        /// <inheritdoc/>
        public void EnableWithModData(string featureName, string key, string value, HashSet<string> param)
        {
            FeatureManager.EnableFeatureWithModData(featureName, key, value, param);
        }

        /// <inheritdoc/>
        public void EnableWithModData(string featureName, string key, string value, Dictionary<string, bool> param)
        {
            FeatureManager.EnableFeatureWithModData(featureName, key, value, param);
        }
    }
}