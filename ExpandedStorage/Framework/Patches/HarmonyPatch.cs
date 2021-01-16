using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;
using Harmony;
using StardewModdingAPI;

namespace ExpandedStorage.Framework.Patches
{
    internal abstract class HarmonyPatch
    {
        internal static IMonitor Monitor;
        internal static ModConfig Config;
        internal HarmonyPatch(IMonitor monitor, ModConfig config)
        {
            Monitor = monitor;
            Config = config;
        }
        protected internal abstract void Apply(HarmonyInstance harmony);

        internal static CodeInstruction Label(CodeInstruction code, Label label)
        {
            code.labels.Add(label);
            return code;
        }

        // Aliases to Common Types
        internal static class T
        {
            internal static readonly Type Bool = typeof(bool);
            internal static readonly Type Int = typeof(int);
            internal static readonly Type Float = typeof(float);
            internal static readonly Type String = typeof(string);
            internal static readonly Type Object = typeof(object);
        }
        
        // Aliases to Common CodeInstructions
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        internal static class IL
        {
            internal static CodeInstruction Beq(object operand = null) =>
                new CodeInstruction(OpCodes.Beq, operand);
            internal static CodeInstruction Bge(object operand = null) =>
                new CodeInstruction(OpCodes.Bge, operand);
            internal static CodeInstruction Br_S(object operand = null) =>
                new CodeInstruction(OpCodes.Br_S, operand);
            internal static CodeInstruction Brfalse_S(object operand = null) =>
                new CodeInstruction(OpCodes.Brfalse_S, operand);
            internal static CodeInstruction Brtrue_S(object operand = null) =>
                new CodeInstruction(OpCodes.Brtrue_S, operand);
            internal static CodeInstruction Call(Type type, string method = null, params Type[] parameters) =>
                new CodeInstruction(OpCodes.Call, AccessTools.Method(type, method, parameters));
            internal static CodeInstruction Call_Get(Type type, string method = null) =>
                new CodeInstruction(OpCodes.Call, AccessTools.Property(type, method).GetGetMethod());
            internal static CodeInstruction Callvirt(Type type, string method = null, params Type[] parameters) =>
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(type, method, parameters));
            internal static CodeInstruction Callvirt_Get(Type type, string method = null) =>
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Property(type, method).GetGetMethod());
            internal static CodeInstruction Isinst(Type type) =>
                new CodeInstruction(OpCodes.Isinst, type);
            internal static CodeInstruction Ldarg_S(object operand = null) =>
                new CodeInstruction(OpCodes.Ldarg_S, operand);
            internal static CodeInstruction Ldc_I4_S(object operand = null) =>
                new CodeInstruction(OpCodes.Ldc_I4_S, operand);
            internal static CodeInstruction Ldfld(Type type, string field) =>
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(type, field));
            internal static CodeInstruction Newobj(Type type, params Type[] parameters) =>
                new CodeInstruction(OpCodes.Newobj, AccessTools.Constructor(type, parameters));
            internal static CodeInstruction Stfld(Type type, string field = null) =>
                new CodeInstruction(OpCodes.Stfld, AccessTools.Field(type, field));
        }
        
        // Aliases to Common CodeInstructions (Opcode only)
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        internal static class OC
        {
            internal static CodeInstruction Add => new CodeInstruction(OpCodes.Add);
            internal static CodeInstruction Beq => new CodeInstruction(OpCodes.Beq);
            internal static CodeInstruction Ldarg_0 => new CodeInstruction(OpCodes.Ldarg_0);
            internal static CodeInstruction Ldarg_1 => new CodeInstruction(OpCodes.Ldarg_1);
            internal static CodeInstruction Ldc_I4_0 => new CodeInstruction(OpCodes.Ldc_I4_0);
            internal static CodeInstruction Ldc_I4_1 => new CodeInstruction(OpCodes.Ldc_I4_1);
            internal static CodeInstruction Ldc_I4_6 => new CodeInstruction(OpCodes.Ldc_I4_6);
            internal static CodeInstruction Ldc_I4_M1 => new CodeInstruction(OpCodes.Ldc_I4_M1);
            internal static CodeInstruction Ldc_I4_S => new CodeInstruction(OpCodes.Ldc_I4_S);
            internal static CodeInstruction Ldloc_S => new CodeInstruction(OpCodes.Ldloc_S);
            internal static CodeInstruction Sub => new CodeInstruction(OpCodes.Sub);
        }
    }
}