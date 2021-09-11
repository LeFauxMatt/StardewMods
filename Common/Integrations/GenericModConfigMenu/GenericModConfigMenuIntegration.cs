using System;
using StardewModdingAPI;
using Common.Helpers.ConfigData;

namespace Common.Integrations.GenericModConfigMenu
{
    internal class GenericModConfigMenuIntegration : ModIntegration<IGenericModConfigMenuAPI>
    {
        public GenericModConfigMenuIntegration(IModRegistry modRegistry)
            : base(modRegistry, "spacechase0.GenericModConfigMenu")
        {
        }

        public Action RevertToDefault(ConfigHelper configHelper, object instance)
        {
            return delegate
            {
                foreach (var field in configHelper.Fields)
                {
                    configHelper.FieldHandler.SetValue(instance, field, field.DefaultValue);
                }
            };
        }

        public void RegisterConfigOptions(IManifest manifest, ConfigHelper configHelper, object instance)
        {
            if (!IsLoaded)
                return;

            foreach (var property in configHelper.Fields)
            {
                configHelper.FieldHandler.RegisterConfigOption(manifest, this, instance, property);
            }
        }
    }
}