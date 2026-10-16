namespace Haxxor.Commands {
    [Alias("%")]
    class PctLoader : HaxCommand {
        int increment;
        int state;
        protected override void Execute() {
            increment = PInt(0, 1);
        }
        protected override void Cycle() {
            state += increment; if (state > 100) state = 100;
            cpu.Blit(cmdX, cmdY, $"{state}%");
            Completed = state >= 100;
        }

        protected override void OnComplete() {
            cpu.AdvanceCursor(4);
        }
    }

}