namespace Haxxor.Commands {
    [Alias("CPUFREQ")]
    class CycleFrequency : HaxCommand {
        int freq;
        int letters;
        protected override void Execute() {
            freq = PInt(0, -1);
            letters = PInt(1,-1);
            if (bios.originalCycleN == 0) bios.originalCycleN = bios.cyclesPerSecond;
            if (bios.origCharsPerCycle  == 0) bios.origCharsPerCycle= bios.charsPerCycle;
            if (NumParams == 0) {
                bios.cyclesPerSecond = bios.originalCycleN;
                bios.charsPerCycle = bios.origCharsPerCycle;
            } else { 
                if (freq >-1) bios.cyclesPerSecond = (byte)freq;
                if (letters > -1) bios.charsPerCycle = (byte)letters;
            }
        }
    }

}