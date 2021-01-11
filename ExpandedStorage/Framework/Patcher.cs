using Harmony;
using HarmonyPatch = ExpandedStorage.Framework.Patches.HarmonyPatch;

namespace ExpandedStorage.Framework
{
    internal class Patcher
    {
        private readonly string _uniqueId;

        internal Patcher(string uniqueId)
        {
            _uniqueId = uniqueId;
        }

        internal void ApplyAll(params HarmonyPatch[] patches)
        {
            var harmony = HarmonyInstance.Create(_uniqueId);

            foreach (var patch in patches)
            {
                patch.Apply(harmony);
            }
        }
    }
}