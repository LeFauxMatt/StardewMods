using System.Collections.Generic;

namespace Common.Integrations.XSPlus
{
    public interface IXSPlusAPI
    {
        public void EnableWithModData(string featureName, string key, string value, bool param);
        public void EnableWithModData(string featureName, string key, string value, float param);
        public void EnableWithModData(string featureName, string key, string value, int param);
        public void EnableWithModData(string featureName, string key, string value, string param);
        public void EnableWithModData(string featureName, string key, string value, HashSet<string> param);
        public void EnableWithModData(string featureName, string key, string value, Dictionary<string, bool> param);
    }
}