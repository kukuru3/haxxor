namespace Haxxor.Commands {
    class CLS : HaxCommand {
        protected override void Execute() {
            for (var y = 0; y < cpu.H; y++) cpu.ClearLine(y);
            bios.cursorX = 0;
            bios.cursorY = 0;
        }
    }
}