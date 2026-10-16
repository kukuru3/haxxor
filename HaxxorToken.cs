using System.Collections.Generic;
using System.Text;
namespace Haxxor {
    public abstract class HaxxorToken {

        protected HaxxorBIOS bios => bus.bios;
        protected HaxxorRAM ram => bus.ram;
        protected HaxxorCPU cpu => bus.cpu;
        protected IHaxxorDisk disk => bus.disk;

        private HaxxorBUS bus;

        internal void Inject(HaxxorBUS bus) {
            this.bus = bus;
        }

        public bool Completed { get; protected set; } = true;
        public bool Blocking { get; protected set; } = true;

        /// <summary> executes once, when enqueued.</summary>
        internal void HaxxorEnqueue() { }

        internal void HaxxorExecute() => Execute();

        internal void HaxxorCycle() => Cycle();

        internal void HaxxorComplete() => OnComplete();

        /// <summary> executes once, when it is reached</summary>
        protected virtual void Execute() { }

        /// <summary> called once every console cycle while active.</summary>
        protected virtual void Cycle() {  }

        /// <summary> executes once, after Completed is set to true.</summary>
        protected virtual void OnComplete() { }

        internal virtual HaxxorToken CloneMe() => (HaxxorToken)MemberwiseClone();

    }

    class HaxText : HaxxorToken { // will output the given text to the next bit in sequence.
        public readonly string txt;
        public HaxText(string txt) {
            this.txt = txt;
        }
        
        // bool didWrapLastTimeAround;
        int cursor;

        protected override void Cycle() {
            for (var i = 0; i < bios.charsPerCycle; i++) {
                if (cursor >= txt.Length) break;
                if (bios.cursorX >= cpu.W && cursor > 0) {
                    CarryOverPreviousLetters();
                    cpu.PrintNext(txt[cursor]);
                    if (txt[cursor] == ' ') bios.cursorX--;
                } else {
                    cpu.PrintNext(txt[cursor]);
                }
                cursor++;
            }
            Completed = cursor >= txt.Length;
        }

        // might need to be carried over into the OS itself.
        void CarryOverPreviousLetters() {
            // if we wrapped and current and previous chars are not break-characters,
            // find all the non-break-chars from the end of the previous line.
            // if there is less than W / 2 of them, replace them with ' ' and print them in this line instead.
            if (IsLetter(txt[cursor - 1]) && IsLetter(txt[cursor])) {
                var prevY = bios.cursorY;
                int foundBacktrack = -1;
                for (var backtrack = 0; backtrack < cpu.W / 2; backtrack++) {
                    if (backtrack >= cursor) break;
                    if (!IsLetter(txt[cursor - 1 - backtrack])) {
                        foundBacktrack = backtrack;
                        break;
                    }
                }

                if (foundBacktrack == -1 && cursor < cpu.W / 2) foundBacktrack = cursor;

                List<char> carryOver = new List<char>(foundBacktrack + 2);
                if (foundBacktrack > 0) {
                    for (var x = -foundBacktrack; x < 0; x++) {
                        cpu.PrintAt(cpu.W + x, prevY, ' ');
                        carryOver.Add(txt[cursor + x]);
                    }
                    foreach (var item in carryOver) cpu.PrintNext(item);
                }
            }
        }

        bool IsLetter(char c) => char.IsLetterOrDigit(c);
    }

    class HaxNewLine : HaxxorToken {
        protected override void Execute() {
            cpu.NextLine();
        }
    }

    public abstract class HaxCommand : HaxxorToken, ICommandWithParams {
        string[] prms;

        internal int cmdX, cmdY;

        public void AcceptParams(string[] funcParameters) {
            this.prms = funcParameters;
            if (funcParameters.Length > 0) if (PStr(funcParameters.Length - 1) == "NOBLOCK") Blocking = false;
        }
        
        protected int PInt(int index, int @default = 0) {
            if (!int.TryParse(PStr(index), out var result)) result = @default;
            return result;
        }

        protected float PFlt(int index, float @default = 0f) {
            if (!float.TryParse(PStr(index), out var result)) result = 0f;
            return result;
        }

        protected string PStr(int index) {
            if (index >= 0 && index < prms?.Length) return prms[index];
            return "";
        }

        protected bool HasParam(int index) => index >= 0 && index < prms?.Length;

        protected int NumParams => prms?.Length ?? 0;

        protected void AsPrintedText(string txt) {
            cpu.ImmediatelyInsertAndStartExecutingToken(new HaxText(txt));
        }
    }

}