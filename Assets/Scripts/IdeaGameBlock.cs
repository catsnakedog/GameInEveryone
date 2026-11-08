using UnityEngine;

public class IdeaGameBlock : GameBlock, IIdeaBlock
{
    public IdeaPieceType type;
    public IdeaPieceType IdeaType => type;
    public override BlockKind Kind => BlockKind.IdeaGame;
    public override string Label => IdeaPieceInfo.Name(type);
    public override Vector2Int[] Shape => IdeaPieceInfo.Shape(type);
    public override Color Color => new Color(.3f, .65f, .95f);
}