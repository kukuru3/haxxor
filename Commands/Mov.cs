namespace Haxxor.Commands {
    class Mov : HaxCommand {
        protected override void Execute() {
            bios.cursorX = (byte)(bios.cursorX + PInt(0));
            bios.cursorY = (byte)(bios.cursorY + PInt(1));
        }
    }
}