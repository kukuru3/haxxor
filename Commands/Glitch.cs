using System.Text;

namespace Haxxor.Commands {
    class Glitch : HaxCommand {
        protected override void Execute() {
            var sb = new StringBuilder();
            var n = PInt(0, 16);
            for (var i = 0; i < n; i++) {
                sb.Append(GetRandomGlitchChar());
                if (i % this.cpu.W - 2 == 0) sb.Append(' ');
            }
            AsPrintedText(sb.ToString());
        }

        private char GetRandomGlitchChar() => (char)(UnityEngine.Random.Range(32, 220));
    }

    class DiskGlitch : HaxCommand {
        protected override void Execute() {
            var file = disk.ReadDisk(PStr(0));
            var lastLine = file.lines.Length - cpu.H;
            var startingLine = 0;
            if (lastLine > 1) startingLine = UnityEngine.Random.Range(0, lastLine);

            for (var y = 0; y < cpu.H; y++) {
                cpu.ClearLine(y);
                cpu.Blit(0, y, file.lines[startingLine + y]);
            }

        }
    }
    [Alias("WORD")]
    class RandomWord : HaxCommand {
        protected override void Execute() {
            var file = disk.ReadDisk(PStr(0));
            var word = file.lines[UnityEngine.Random.Range(0, file.lines.Length)];
            AsPrintedText(word);
        }
    }

}