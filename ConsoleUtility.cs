using System;
using System.Reflection;

namespace Haxxor {

    static class ConsoleUtility {
        public static void AutoregisterInternalCommands(HaxxorComputer c) {
            var types = ReflectionUtility.FindImplementingTypesInSolution<HaxCommand>(true);
            foreach (var type in types) {
                c.Commands.RegisterCommand(type, type.Name.ToUpperInvariant());
                var aliasAttrs = type.GetCustomAttributes<Alias>();
                foreach (var aa in aliasAttrs) {
                    c.Commands.RegisterCommand(type, aa.AliasAs);
                }
            }
        }

        //public static HaxxorToken Duplicate(HaxxorTokenizer tokenizer, HaxxorToken originalToken) {
        //    if (originalToken is HaxNewLine nl) return new HaxNewLine();
        //    return tokenizer.CompileToken(originalToken.originalText);
        //    //if (!(console.Commands is ICommandInstantiator ici)) throw new Exception("Console cannot duplicate commmands");
        //    //var t = originalToken.GetType();
        //    //var clone = ici.CreateCommand(t.Name.ToUpperInvariant());
        //    //if (originalToken is ICommandWithParams prmToken) {
        //    //    if (clone is ICommandWithParams icwp) {
        //    //        var field = t.GetField("prms", BindingFlags.Instance | BindingFlags.NonPublic);
        //    //        icwp.AcceptParams((string[])field.GetValue(originalToken));
        //    //        return clone;
        //    //    }
        //    //}
        //    //throw new Exception("Wuuuut");
        //}
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class Alias : Attribute {
        public string AliasAs { get; }
        public Alias(string alias) {
            AliasAs = alias;
        }
    }
}