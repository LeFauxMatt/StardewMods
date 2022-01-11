namespace CommonHarmony;

using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;

internal class AssemblyPatch
{
    private readonly Assembly _assembly;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AssemblyPatch" /> class.
    /// </summary>
    /// <param name="name"></param>
    public AssemblyPatch(string name)
        : this(a => a.FullName.StartsWith($"{name},"))
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AssemblyPatch" /> class.
    /// </summary>
    /// <param name="matcher"></param>
    public AssemblyPatch(Func<Assembly, bool> matcher)
    {
        this._assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(matcher);
    }

    public AssemblyPatchType Type(string name)
    {
        return this._assembly is not null ? new AssemblyPatchType(this._assembly.GetType(name)) : null;
    }

    public MethodInfo Method(string type, string method)
    {
        return this.Type(type).Method(method);
    }

    internal class AssemblyPatchType
    {
        private readonly Type _type;

        internal AssemblyPatchType(Type type)
        {
            this._type = type;
        }

        public MethodInfo Method(string name)
        {
            return AccessTools.Method(this._type, name);
        }

        public MethodInfo Method(Func<MethodInfo, bool> matcher)
        {
            return AccessTools.GetDeclaredMethods(this._type).FirstOrDefault(matcher);
        }
    }
}