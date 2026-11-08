using UnityEngine;

public class BadFanBlock : HumanBlock
{
    public override BlockKind Kind => BlockKind.BadFan;

    public override Vector2Int[] Shape => new[]
    {
        new Vector2Int(0, 0),
        new Vector2Int(1, 0),
        new Vector2Int(0, 1),
    };
}