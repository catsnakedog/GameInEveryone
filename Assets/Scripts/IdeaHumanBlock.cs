using UnityEngine;

public class IdeaHumanBlock : HumanBlock, IIdeaBlock
{
    public IdeaPieceType type;
    public IdeaPieceType IdeaType => type;
    public override BlockKind Kind => BlockKind.IdeaHuman;
    public override string Label => IdeaPieceInfo.Name(type);
    public override Vector2Int[] Shape => IdeaPieceInfo.Shape(type);
}