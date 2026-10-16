namespace Haxxor.Commands {
    class Progress : HaxCommand {
        int progressInterval;
        char full, zero;
        int size;

        int progress = 0;

        protected override void Execute() {
            full = PStr(0)[0];
            zero = PStr(1)[0];
            size = PInt(2, 10);
            progressInterval = PInt(3,10);

        }

        protected override void Cycle() {
            progress++;
            int fullBars = progress / progressInterval;
            for (var i = 0; i < size; i++) {
                cpu.PrintAt(cmdX + i, cmdY, (i < fullBars) ? full : zero);
            }
            Completed = fullBars >= size;
        }

        protected override void OnComplete() {
            if (Blocking) cpu.AdvanceCursor(size);
        }
    }

}