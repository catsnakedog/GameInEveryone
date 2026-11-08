using UnityEngine;

public class GoodGameBlock : GameBlock
{
    public override BlockKind Kind => BlockKind.GoodGame;

    public override Vector2Int[] Shape => new[]
    {
        new Vector2Int(0, 0),
        new Vector2Int(1, 0),
    };
}