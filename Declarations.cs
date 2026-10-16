using System;
using System.Collections.Generic;

namespace Haxxor {
    interface ICommandInstantiator {
        HaxCommand CreateCommand(string identifier);
    }

    interface IRegistersCommands {
        void RegisterCommand(Type t, string identifier);
    }

    interface ICommandWithParams {
        void AcceptParams(string[] funcParameters);
    }
    interface ICloneable {
        object CloneSelf();
    }

    public class HaxxorBUS {
        internal HaxxorCPU cpu;
        internal HaxxorBIOS bios;
        internal HaxxorRAM ram;
        internal IHaxxorDisk disk;
    }

    public class HaxxorBIOS {
        public byte cursorX;
        public byte cursorY;
        public byte cyclesPerSecond = 3;
        public byte charsPerCycle = 1;
        public byte cursorBlinkFreq = 2;
        public bool advancedTextBreak = true;
        public byte originalCycleN, origCharsPerCycle;
    }

    public class HaxxorRAM {
        public HaxxorRAM(int size) {
            bytes = new byte[size];
        }
        public byte[] bytes;
    }

    public delegate void LineTextChangeAction(int lineID, string newText);

    interface IDispatchesConsoleEvents {
        event Action MatrixShiftedUp;
        event LineTextChangeAction LineTextChanged;
    }

    interface IModifiesConsoleQueue {
        IEnumerable<HaxxorToken> ModifyQueue(IEnumerable<HaxxorToken> tokens);
    }

    interface ICanInsertAdditionalTokens {
        bool RemoveSelf { get; }
        IEnumerable<HaxxorToken> GetInsertions();
    }

    interface IHaxxorCPU {
        byte W { get; }
        byte H { get; }

        void PrintNext(char c);
        void NextLine();

        void PrintAt(int x, int y, char c);
        void Blit(int x, int y, string s);
        void Blit(int x, int y, string[] s);
        void AdvanceCursor(int x);
        void ClearLine(int y);
    }
}