using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Haxxor { 
    internal static class ReflectionUtility {
         public static IEnumerable<Type> FindImplementingTypesInSolution<T>(bool concreteOnly) {
            var t = typeof(T);
            var allAssemblies = GetGameAssemblies(true).ToList();
            foreach (var ass in allAssemblies) {
                foreach (var type in ass.GetTypes()) {
                    if (concreteOnly && type.IsAbstract) continue;
                    if (t.IsAssignableFrom(type)) yield return type;
                }
            }
        }

        static IEnumerable<Assembly> GetGameAssemblies(bool proprietaryOnly) {
            foreach (var ass in AppDomain.CurrentDomain.GetAssemblies()) {
                if (proprietaryOnly && !IsProprietaryAssembly(ass)) continue;
                yield return ass;
            }
        }

        // this is by no means intended to be an actual accurate representation.
        // it's just there to filter out some common assemblies that we know are guaranteed to not be proprietary.
        static private bool IsProprietaryAssembly(Assembly ass) {
            var n = ass.FullName;
            if (n.Contains("Assembly-CSharp")) return true;
            if (n.Contains("System.")) return false;
            if (n.Contains("VisualStudio")) return false;
            if (n == "mscorlib" || n == "netstandard" || n == "Boo.Lang") return false;
            if (n.Contains("Microsoft.")) return false;
            if (n.Contains("UnityEditor")) return false;
            if (n.Contains("UnityEngine")) return false;
            if (n.Contains("UnityScript")) return false;
            if (n.Contains("Unity")) return false;
            return true;
        }
    }
}