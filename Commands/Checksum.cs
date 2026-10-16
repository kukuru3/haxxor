namespace Haxxor.Commands {
    class Checksum : HaxCommand {
        protected override void Execute() {
            var n = PInt(0, 4);
            var sb = new System.Text.StringBuilder(n);
            for (var i = 0; i < n; i++) sb.Append($"{UnityEngine.Random.Range(0, 15):x}");
            AsPrintedText(sb.ToString());
        }
    }

}