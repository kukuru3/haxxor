namespace Haxxor.Commands {
    [Alias("W")]
    class Wait : HaxCommand {
        int time;
        bool doCursor;
        const char cursor = '_';
        protected override void Execute() {
            time = PInt(0);
            doCursor = (PStr(1) != "NOCURSOR" && PStr(1) != "N");
            // if (HasParam(1)) cursor = PStr(1)[0];
            //if (HasParam(2)) {
            //    time = UnityEngine.Random.Range(PInt(0), PInt(1));
            //} else {
            //    time = PInt(0);
            //}
        }
        protected override void Cycle() {
            Completed = time-- <= 0;
            if (doCursor)
                cpu.PrintAt(cmdX, cmdY, (time % 20 > 10) ? cursor : ' ');
        }
    }

}