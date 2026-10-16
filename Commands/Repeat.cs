using System.Collections.Generic;
using System.Linq;

namespace Haxxor.Commands {
    class Repeat : HaxCommand, IModifiesConsoleQueue {
        public IEnumerable<HaxxorToken> ModifyQueue(IEnumerable<HaxxorToken> tokens) {
            var l = tokens.ToList();
            var from = -1;
            var to = -1;
            int repeats = PInt(0);
            for (var i = 0; i < l.Count; i++) {
                if (l[i] == this) from = i+1;
                if (from > -1 && l[i] is End endToken) { to = i; break;}
            }
            if (to > from) {
                for (var i = 0; i < from; i++) yield return l[i];
                for (var r = 0; r < repeats; r++) {
                    for (var z = from; z < to; z++) {
                        if (r == 0) yield return l[z];
                        else yield return l[z].CloneMe(); // ConsoleUtility.Duplicate(tokenizer, l[z]);
                    }
                }
                for (var i = to; i < l.Count; i++) yield return l[i];
            }
        }
    }

    class End : HaxCommand {

    }

}