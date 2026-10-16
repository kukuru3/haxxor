using System.Collections.Generic;
using System.Linq;

namespace Haxxor {
    public class HaxxorCPU : IHaxxorCPU, IDispatchesConsoleEvents {
        public byte W { get; }
        public byte H { get; }

        internal readonly HaxxorBUS bus;

        char[][] charMatrix;
        bool[] dirtyLines;

        readonly HashSet<HaxxorToken> currentlyExecutingTokens;

        internal void Reset() {
            currentlyExecutingTokens.Clear();
            for (var y = 0; y < H; y++) ClearLine(y);
            bus.bios.cursorX = 0;
            bus.bios.cursorY = 0;
            bus.ram.bytes = new byte[bus.ram.bytes.Length];
        }

        readonly Dictionary<string, int> registers;

        public event System.Action MatrixShiftedUp;
        public event LineTextChangeAction LineTextChanged;

        public HaxxorCPU(byte consoleW, byte consoleH, HaxxorBUS bus) {
            this.W = consoleW;
            this.H = consoleH;
            this.bus = bus;
            registers = new Dictionary<string, int>();
            currentlyExecutingTokens = new HashSet<HaxxorToken>();
            charMatrix = new char[H][];
            dirtyLines = new bool[H];
            for (var i = 0; i < H; i++) charMatrix[i] = new char[W];
        }

        public void AdvanceCursor(int n) {
            bus.bios.cursorX += (byte)n;
            ValidatePosition();
        }

        /// <returns>true if line end was reached</returns>
        void ValidatePosition() {
            if (bus.bios.cursorX >= W) { NextLine(); }
        }

        /// <returns>true if matrix was also shifted</returns>
        public void NextLine() {
            bus.bios.cursorX = 0;
            bus.bios.cursorY++;
            if (bus.bios.cursorY >= H) { ShiftMatrix(); }
        }

        public void ShiftMatrix() {
            var m0 = charMatrix[0];
            // dirtyLines[0] = true; // we are expecting whatever is listening to know when the matrix is shifted
            for (var y = 1; y < H; y++) {
                charMatrix[y - 1] = charMatrix[y];
                // dirtyLines[y] = true;
            }
            charMatrix[H-1] = m0;
            ClearLine(H-1);
            bus.bios.cursorY--;
            MatrixShiftedUp?.Invoke();
        }

        public void ClearLine(int y) {
            for (var x = 0; x < W; x++) charMatrix[y][x] = ' ';
            dirtyLines[y] = true;
        }

        public void PrintNext(char c) {
            ValidatePosition();
            charMatrix[bus.bios.cursorY][bus.bios.cursorX] = c;
            dirtyLines[bus.bios.cursorY] = true;
            bus.bios.cursorX++;
        }

        public void PrintAt(int x, int y, char c) {
            charMatrix[y][x] = c;
            dirtyLines[y] = true;
        }
        bool IsLegalIndex(int x, int y) => x >= 0 && x < W && y>= 0 && y < H;

        /// <summary> Ignores overflow - does not linebreak</summary>
        public void Blit(int x, int y, string s) {
            var lastLegalI = W - x - 1;
            for (var i = 0; i < UnityEngine.Mathf.Min(s.Length, lastLegalI); i++) charMatrix[y][x+i] = s[i];
            dirtyLines[y] = true;
        }
        public void Blit(int x, int y, string[] s) {
            for (var iy = 0; iy < s.Length; iy++) 
            for (var ix = 0; ix < s.Length; ix++)
                if (IsLegalIndex(x, y))
                    charMatrix[y+iy][x + ix] = s[iy][ix];
            dirtyLines[y] = true;
        }

        void FlushTextChanges() {
            for (var y = 0; y < H; y++) {
                if (dirtyLines[y]) {
                    LineTextChanged?.Invoke(y, CompileLineText(y));
                    dirtyLines[y] = false;
                }
            }
        }

        private string CompileLineText(int y) => new string(charMatrix[y]);

        internal void ExecuteCycle() {
            foreach (var token in currentlyExecutingTokens) token.HaxxorCycle();

            PoliceQueues();

            AcceptsNewTokens = DetermineIfAcceptsNewToken();

            FlushTextChanges();
        }

        private void PoliceQueues() {
            // for COMPLETED tokens, remove them from queue and call OnComplete
            var toRemove = currentlyExecutingTokens.Where(token => token.Completed).ToList();
            currentlyExecutingTokens.ExceptWith(toRemove);
            foreach (var removedToken in toRemove) removedToken.HaxxorComplete();
        }

        // determine if we accept new tokens:
        // remember, at this stage, NEWLY EXECUTING TOKENS IS EMPTY
        private bool DetermineIfAcceptsNewToken() {
            foreach (var t in currentlyExecutingTokens) if (t.Blocking) return false;
            return true;
        }

        internal bool AcceptsNewTokens { get; private set; }

        internal void StartExecutingToken(HaxxorToken token) {
            if (token is HaxCommand cmd) { cmd.cmdX = bus.bios.cursorX; cmd.cmdY = bus.bios.cursorY; }
            token.HaxxorExecute();
            currentlyExecutingTokens.Add(token);
        }

        internal void ImmediatelyInsertAndStartExecutingToken(HaxxorToken token) {
            token.Inject(bus);
            token.HaxxorEnqueue();
            StartExecutingToken(token);
        }
    }
}