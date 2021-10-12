namespace Common.Integrations.XSPlus
{
    using System;
    using System.Collections.Generic;

    public interface IXSPlusAPI
    {
        public void EnableWithModData(string featureName, string key, string value, bool param);

        public void EnableWithModData(string featureName, string key, string value, float param);

        public void EnableWithModData(string featureName, string key, string value, int param);

        public void EnableWithModData(string featureName, string key, string value, string param);

        public void EnableWithModData(string featureName, string key, string value, HashSet<string> param);

        public void EnableWithModData(string featureName, string key, string value, Dictionary<string, bool> param);

        public void EnableWithModData(string featureName, string key, string value, Tuple<int, int, int> param);
    }
}