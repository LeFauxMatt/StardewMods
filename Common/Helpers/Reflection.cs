namespace Common.Helpers
{
    using StardewModdingAPI;

    internal static class Reflection
    {
        /// <inheritdoc cref="IReflectionHelper" />
        private static IReflectionHelper Helper { get; set; }

        /// <summary>Initializes the <see cref="Reflection" /> class.</summary>
        /// <param name="reflectionHelper">The instance of IReflectionHelper from the Mod.</param>
        public static void Init(IReflectionHelper reflectionHelper)
        {
            Reflection.Helper = reflectionHelper;
        }

        public static IReflectedMethod Method<TStaticMethodType>(string name, bool required = true)
        {
            return Reflection.Helper.GetMethod(typeof(TStaticMethodType), name, required);
        }

        public static IReflectedMethod Method(object obj, string name, bool required = true)
        {
            return Reflection.Helper.GetMethod(obj, name, required);
        }

        public static IReflectedField<TValue> Field<TValue, TStaticFieldType>(string name, bool required = true)
        {
            return Reflection.Helper.GetField<TValue>(typeof(TStaticFieldType), name, required);
        }

        public static IReflectedField<TValue> Field<TValue>(object obj, string name, bool required = true)
        {
            return Reflection.Helper.GetField<TValue>(obj, name, required);
        }

        public static IReflectedProperty<TValue> Property<TValue, TStaticFieldType>(string name, bool required = true)
        {
            return Reflection.Helper.GetProperty<TValue>(typeof(TStaticFieldType), name, required);
        }

        public static IReflectedProperty<TValue> Property<TValue>(object obj, string name, bool required = true)
        {
            return Reflection.Helper.GetProperty<TValue>(obj, name, required);
        }
    }
}