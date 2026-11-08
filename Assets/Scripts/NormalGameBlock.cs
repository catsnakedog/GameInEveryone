using UnityEngine;

public class NormalGameBlock : GameBlock
{
    public override BlockKind Kind => BlockKind.NormalGame;

    public override Vector2Int[] Shape => new[]
    {
        new Vector2Int(0, 0),
    };
}