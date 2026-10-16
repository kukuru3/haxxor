using System;
using System.Collections.Generic;
using System.Linq;

namespace Haxxor {

    public class HaxxorComputer
    {
        Queue<HaxxorToken> tokenQueue;

        readonly HaxxorTokenizer tokenizer;
        readonly HaxxorCommandInstantiator cmdInstantiator;
        internal readonly HaxxorCPU cpu;
        internal readonly HaxxorBIOS bios;
        internal readonly HaxxorRAM ram;
        internal readonly HaxxorBUS bus;
        internal readonly IHaxxorDisk disk;

        double accumulatedTime;

        public HaxxorComputer(byte numLines, byte charactersPerLine, int bytesOfRam) {
            tokenQueue = new Queue<HaxxorToken>();
            cmdInstantiator = new HaxxorCommandInstantiator();
            tokenizer = new HaxxorTokenizer(cmdInstantiator, this);

            bus = new HaxxorBUS();

            ram = new HaxxorRAM(bytesOfRam);
            bios = new HaxxorBIOS() {  };
            cpu = new HaxxorCPU(charactersPerLine, numLines, bus);
            disk = new HaxxorDiskFromResources("Disk");

            
            bus.bios = bios;
            bus.cpu = cpu;
            bus.disk = disk;
            bus.ram = ram;
            
        }

        public void ForceExecuteCycle() {
            ExecuteCycle();
        }

        void ExecuteCycle() {
            if (cpu.AcceptsNewTokens) InsertNextBatchOfTokensIntoCPU();
            cpu.ExecuteCycle();
        }

        /// <summary>Instead of seconds, we opt for a frame-based timing.</summary>
        internal void ProcessFrame() {
            // if (UnityEngine.Time.frameCount % bus.bios.cyclesPerSecond == 0) ExecuteCycle();
        }

        public bool IsIdle => this.tokenQueue.Count == 0 && cpu.AcceptsNewTokens;

        internal void ProcessTimePassed(double realtimeSeconds) {
            //if (ManualExecution) return;
            accumulatedTime += realtimeSeconds;
            var deltaTime = 1.0 / bios.cyclesPerSecond;
            if (accumulatedTime > deltaTime) {
                ExecuteCycle();
                accumulatedTime -= deltaTime;
            }
        }

        private void InsertNextBatchOfTokensIntoCPU() {
            bool keepInsertingTokens = tokenQueue.Count > 0;
            while (keepInsertingTokens) { // dequeue 
                var nextToken = tokenQueue.Dequeue();
                cpu.StartExecutingToken(nextToken);
                keepInsertingTokens = (tokenQueue.Count > 0) && !nextToken.Blocking;
            }
        }

        internal IDispatchesConsoleEvents Events => cpu;
        internal IRegistersCommands Commands => cmdInstantiator;

        // printing to console should be done in batches, not small bits.
        public void Print(string rawText) {
            // basically we apply a primitive "tokenization" where we break the text up into COMMANDS
            // we are not making a turing complete thing here. Just a few utilities.
            var tokens = tokenizer.Tokenize(rawText).ToArray();
            foreach (var token in tokens) {
                DoInsertToken(token);
            }

            // allow tokens to modify queue:
            foreach (var token in tokens.OfType<IModifiesConsoleQueue>().ToArray()) {
                var oldTokens = new HashSet<HaxxorToken>(tokenQueue);
                var modifiedQueue = token.ModifyQueue(tokenQueue).ToArray();
                tokenQueue = new Queue<HaxxorToken>(
                    modifiedQueue
                );
                var newTokens = new HashSet<HaxxorToken>(tokenQueue);
                newTokens.ExceptWith(oldTokens);
                foreach (var t in newTokens) {
                    t.Inject(bus);
                    t.HaxxorEnqueue();
                }
            }
        }

        private void DoInsertToken(HaxxorToken token) {
            token.Inject(bus);
            bool skipInsertion = false;

            List<HaxxorToken> insertions = new List<HaxxorToken>();
            if (token is ICanInsertAdditionalTokens iciat) {
                insertions.AddRange(iciat.GetInsertions());
                skipInsertion = iciat.RemoveSelf;
            }

            if (!skipInsertion) {
                tokenQueue.Enqueue(token);
                token.HaxxorEnqueue();
            }

            foreach (var t in insertions) DoInsertToken(t);
        }

        /// <summary> Note: does not wrap</summary>
        public void DirectPrint(string text, int x, int y) {
            throw new NotImplementedException();
            // todo: create a special direct print command token, and immediately add it to the
            // execution of the console.
        }

    }
}