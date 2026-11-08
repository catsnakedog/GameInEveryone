using UnityEngine;

public class GoodFanBlock : HumanBlock
{
    public override BlockKind Kind => BlockKind.GoodFan;

    public override Vector2Int[] Shape => new[]
    {
        new Vector2Int(0, 0),
        new Vector2Int(1, 0),
    };
}