namespace Haxxor.Commands {
    [Alias("R")]
    class RandomInt : HaxCommand {
        protected override void Execute() => AsPrintedText($"{UnityEngine.Random.Range(PInt(0), PInt(1))}");
    }

}