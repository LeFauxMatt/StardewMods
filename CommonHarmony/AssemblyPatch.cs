namespace CommonHarmony
{
    using System;
    using System.Linq;
    using System.Reflection;
    using HarmonyLib;

    internal class AssemblyPatch
    {
        private readonly Assembly Assembly;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyPatch"/> class.
        /// </summary>
        /// <param name="name"></param>
        public AssemblyPatch(string name)
            : this(a => a.FullName.StartsWith($"{name},"))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyPatch"/> class.
        /// </summary>
        /// <param name="matcher"></param>
        public AssemblyPatch(Func<Assembly, bool> matcher)
        {
            this.Assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(matcher);
        }

        public AssemblyPatchType Type(string name)
        {
            return this.Assembly != null ? new AssemblyPatchType(this.Assembly.GetType(name)) : null;
        }

        public MethodInfo Method(string type, string method)
        {
            return Type(type).Method(method);
        }

        internal class AssemblyPatchType
        {
            private readonly Type Type;

            internal AssemblyPatchType(Type type)
            {
                this.Type = type;
            }

            public MethodInfo Method(string name)
            {
                return AccessTools.Method(this.Type, name);
            }

            public MethodInfo Method(Func<MethodInfo, bool> matcher)
            {
                return AccessTools.GetDeclaredMethods(this.Type).FirstOrDefault(matcher);
            }
        }
    }
}