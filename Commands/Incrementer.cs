namespace Haxxor.Commands {
    [Alias("INC")]
    class Incrementer : HaxCommand {
        int state, to, increment;
        
        protected override void Execute() {
            state = PInt(0);
            to = PInt(1);
            increment = PInt(2, 1);
        }

        protected override void Cycle() {
            state += increment;
            if (state > to) state = to;
            cpu.Blit(cmdX,cmdY, $"{state}");
            Completed = state >= to;
        }

        protected override void OnComplete() {
            if (Blocking) cpu.AdvanceCursor(to.ToString().Length);
        }
    }

}

