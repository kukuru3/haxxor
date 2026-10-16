using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Haxxor {
    class HaxxorCommandInstantiator : ICommandInstantiator, IRegistersCommands {
        readonly Dictionary<string, Func<HaxCommand>> cachedConstructors = new Dictionary<string, Func<HaxCommand>>();

        public void RegisterCommand(Type t, string identifier) {
            // create and cache a constructor:
            if (cachedConstructors.TryGetValue(identifier, out var ctor)) {
                UnityEngine.Debug.Log($"Re-aliasing {identifier}: was for {ctor}, trying to add for {t}");
            } else {
                Expression body = Expression.New(t);
                ctor = (Func<HaxCommand>)Expression.Lambda(body).Compile();
                cachedConstructors[identifier] = (Func<HaxCommand>)ctor;
            }
            
        }

        HaxCommand ICommandInstantiator.CreateCommand(string cmdIdentifier) {
            if (cachedConstructors.TryGetValue(cmdIdentifier, out var ctor)) {
                var commandInstance = ctor();
                return commandInstance;
            }
            return null;
        }
    }
}