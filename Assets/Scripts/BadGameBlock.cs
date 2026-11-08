using UnityEngine;

public class BadGameBlock : GameBlock
{
    public override BlockKind Kind => BlockKind.BadGame;

    public override Vector2Int[] Shape => new[]
    {
        new Vector2Int(0, 0),
    };
}