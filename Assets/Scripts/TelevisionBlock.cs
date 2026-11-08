using UnityEngine;

public class TelevisionBlock : SpecialBlock
{
    public override BlockKind Kind => BlockKind.Television;

    public override Vector2Int[] Shape => new[]
    {
        new Vector2Int(0, 0),
        new Vector2Int(1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(1, 1),
    };
}