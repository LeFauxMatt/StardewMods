using System.Collections.Generic;
using Common.Integrations.XSPlus;

namespace XSPlus
{
    public class XSPlusAPI : IXSPlusAPI
    {
        public void EnableWithModData(string featureName, string key, string value, bool param)
        {
            XSPlusAPI.EnableWithModData(featureName, key, value, param);
        }
        public void EnableWithModData(string featureName, string key, string value, float param)
        {
            XSPlusAPI.EnableWithModData(featureName, key, value, param);
        }
        public void EnableWithModData(string featureName, string key, string value, int param)
        {
            XSPlusAPI.EnableWithModData(featureName, key, value, param);
        }
        public void EnableWithModData(string featureName, string key, string value, string param)
        {
            XSPlusAPI.EnableWithModData(featureName, key, value, param);
        }
        public void EnableWithModData(string featureName, string key, string value, HashSet<string> param)
        {
            XSPlusAPI.EnableWithModData(featureName, key, value, param);
        }
        public void EnableWithModData(string featureName, string key, string value, Dictionary<string, bool> param)
        {
            XSPlusAPI.EnableWithModData(featureName, key, value, param);
        }
        private static void EnableWithModData<T>(string featureName, string key, string value, T param)
        {
            if (!XSPlus.Features.TryGetValue(featureName, out var feature))
                return;
            if (param is bool bParam)
            {
                feature.EnableWithModData(key, value, bParam);
            }
            else
            {
                var featureWithParam = feature as FeatureWithParam<T>;
                featureWithParam?.EnableWithModData(key, value, true, param);
            }
        }
    }
}