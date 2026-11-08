using UnityEngine;

public class OrdinaryPersonBlock : HumanBlock
{
    public override BlockKind Kind => BlockKind.OrdinaryPerson;

    public override Vector2Int[] Shape => new[]
    {
        new Vector2Int(0, 0),
        new Vector2Int(1, 0),
    };
}