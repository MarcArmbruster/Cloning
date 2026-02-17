namespace Cloning.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;

    internal static class TypeExtensions
    {
        internal static bool HasDefaultConstructor(this Type type)
        {
            ConstructorInfo[] ctors = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            return ctors.Any(c => c.IsPublic && c.GetParameters().Length == 0);
        }

        internal static ParameterInfo[] GetConstructorParameterTypes(this Type type)
        {
            ConstructorInfo[] ctors = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var ctor = ctors.FirstOrDefault(c => c.GetParameters().Length > 0);

            if (ctor == null)
            {
                return [];
            }

            return ctor.GetParameters();
        }
    }
}
