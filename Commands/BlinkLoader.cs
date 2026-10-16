namespace Haxxor.Commands {
    [Alias("BLINK")]
    class BlinkLoader : HaxCommand {
        string blinkingText, finalText;
        int numBlinks, blinkPhase;
        int maxLen;
        protected override void Execute() { 
            blinkingText = PStr(0);
            blinkPhase = PInt(1);
            numBlinks = PInt(2);
            finalText = PStr(3);
            maxLen = UnityEngine.Mathf.Max(finalText.Length, blinkingText.Length);
        }

        int cycle;

        protected override void Cycle() {
            cycle++;
            var q = cycle % (blinkPhase * 2);
            if (q < blinkPhase) {
                cpu.Blit(cmdX,cmdY, blinkingText);
            } else {
                cpu.Blit(cmdX,cmdY, new string(' ', maxLen));
            }
            Completed = cycle >= numBlinks * blinkPhase * 2;
        }
        protected override void OnComplete() {
            cpu.Blit(cmdX,cmdY, new string(' ', maxLen));
            cpu.Blit(cmdX,cmdY, finalText);
            if (Blocking) cpu.AdvanceCursor(maxLen);
        }
    }

}