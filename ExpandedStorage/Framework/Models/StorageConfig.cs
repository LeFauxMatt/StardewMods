using System.Collections.Generic;
using System.Linq;
using ImJustMatt.ExpandedStorage.API;

namespace ImJustMatt.ExpandedStorage.Framework.Models
{
    public class StorageConfig : IStorageConfig
    {
        public enum Choice
        {
            Unspecified,
            Enable,
            Disable
        }

        public static readonly IDictionary<string, string> StorageOptions = new Dictionary<string, string>()
        {
            {"AccessCarried", "Allow storage to be access while carried"},
            {"CanCarry", "Allow storage to be picked up"},
            {"ShowSearchBar", "Show search bar above chest inventory"},
            {"VacuumItems", "Allow storage to automatically collect dropped items"}
        };

        public int Capacity { get; set; }
        public HashSet<string> EnabledFeatures { get; set; } = new() {"CanCarry", "ShowSearchBar"};
        public HashSet<string> DisabledFeatures { get; set; } = new();
        public IList<string> Tabs { get; set; } = new List<string>();

        internal Choice Option(string option)
        {
            if (DisabledFeatures.Contains(option))
                return Choice.Disable;
            return EnabledFeatures.Contains(option) ? Choice.Enable : Choice.Unspecified;
        }

        internal void SetOption(string option, Choice choice)
        {
            EnabledFeatures.Remove(option);
            DisabledFeatures.Remove(option);
            switch (choice)
            {
                case Choice.Disable:
                    DisabledFeatures.Add(option);
                    break;
                case Choice.Enable:
                    EnabledFeatures.Add(option);
                    break;
            }
        }

        internal void CopyFrom(IStorageConfig config)
        {
            if (config.Capacity != 0) Capacity = config.Capacity;

            foreach (var enabledFeature in config.EnabledFeatures)
            {
                SetOption(enabledFeature, Choice.Enable);
            }

            foreach (var disabledFeature in config.DisabledFeatures)
            {
                SetOption(disabledFeature, Choice.Disable);
            }

            if (config.Tabs.Any())
                Tabs.Clear();
            foreach (var tab in config.Tabs)
            {
                Tabs.Add(tab);
            }
        }
    }
}