namespace Haxxor.Commands {
    class Slides : HaxCommand {
        private int shiftCycle;
        int state;
        int lastSize;

        protected override void Execute() {
            shiftCycle = PInt(0, 1);
        }
        protected override void Cycle() {
            state++;
            int index = 1 + state / shiftCycle;
            if (index >= NumParams) {
                Completed = true;
            } else {
                Completed = false;
                if (lastSize > 0) cpu.Blit(cmdX, cmdY, new string(' ', lastSize));
                cpu.Blit(cmdX, cmdY, PStr(index));
                lastSize = PStr(index).Length;
            }
        }

        protected override void OnComplete() {
            if (Blocking) cpu.AdvanceCursor(lastSize);
        }
    }
}