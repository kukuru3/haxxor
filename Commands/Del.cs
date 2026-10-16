namespace Haxxor.Commands {
    // takes the cursor back N spaces (and fills those spaces with ' ')
    class Del : HaxCommand {
        int n = 0;
        protected override void Execute() {
            n = PInt(0, 1);
            for (var i = 0; i < n; i++) {
                if (cmdX - i >= 0) { 
                    bios.cursorX = (byte)(cmdX - i);
                    cpu.PrintAt(cmdX - i, cmdY, ' ');
                }
            }
        }
    }
}