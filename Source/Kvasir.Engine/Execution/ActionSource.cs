namespace nGratis.AI.Kvasir.Engine;

public class ActionSource : IActionSource
{
    public ActionSource()
    {
        this.Card = Engine.Card.Unknown;
    }

    public static IActionSource Unknown => UnknownActionSource.Instance;

    public static IActionSource None => NoneActionSource.Instance;

    public ICard Card { get; init; }
}