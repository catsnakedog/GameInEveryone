using UnityEngine;

public class StreamerBlock : HumanBlock
{
    public override BlockKind Kind => BlockKind.Streamer;

    public override Vector2Int[] Shape => new[]
    {
        new Vector2Int(0, 0),
        new Vector2Int(1, 0),
        new Vector2Int(1, 1),
    };
}