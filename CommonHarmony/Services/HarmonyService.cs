namespace CommonHarmony.Services
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Common.Services;
    using Enums;
    using HarmonyLib;
    using Models;

    internal class HarmonyService : BaseService
    {
        private readonly Harmony _harmony;
        private readonly Dictionary<string, IList<SavedPatch>> _savedPatches = new();

        internal HarmonyService(ServiceManager serviceManager)
            : base("Harmony")
        {
            this._harmony = new(serviceManager.ModManifest.UniqueID);
        }

        public void AddPatch(string group, MethodBase original, Type type, string name, PatchType patchType = PatchType.Prefix)
        {
            if (!this._savedPatches.TryGetValue(group, out var patches))
            {
                patches = new List<SavedPatch>();
                this._savedPatches.Add(group, patches);
            }

            patches.Add(new(original, type, name, patchType));
        }

        public void ApplyPatches(string group)
        {
            if (this._savedPatches.TryGetValue(group, out var patches))
            {
                foreach (var patch in patches)
                {
                    switch (patch.PatchType)
                    {
                        case PatchType.Prefix:
                            this._harmony.Patch(patch.Original, patch.Patch);
                            break;
                        case PatchType.Postfix:
                            this._harmony.Patch(patch.Original, postfix: patch.Patch);
                            break;
                        case PatchType.Transpiler:
                            this._harmony.Patch(patch.Original, transpiler: patch.Patch);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        public void UnapplyPatches(string group)
        {
            if (this._savedPatches.TryGetValue(group, out var patches))
            {
                foreach (var patch in patches)
                {
                    this._harmony.Unpatch(patch.Original, patch.Method);
                }
            }
        }
    }
}