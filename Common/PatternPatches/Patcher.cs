using Harmony;

namespace Common.PatternPatches
{
    public class Patcher<T>
    {
        private readonly string _uniqueId;

        internal Patcher(string uniqueId)
        {
            _uniqueId = uniqueId;
        }

        internal void ApplyAll(params Patch<T>[] patches)
        {
            var harmony = HarmonyInstance.Create(_uniqueId);

            foreach (var patch in patches)
            {
                patch.Apply(harmony);
            }
        }
    }
}