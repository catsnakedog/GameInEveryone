using UnityEngine;

public class IdeaSpecialBlock : SpecialBlock, IIdeaBlock
{
    public IdeaPieceType type;
    public IdeaPieceType IdeaType => type;
    public override BlockKind Kind => BlockKind.IdeaSpecial;
    public override string Label => IdeaPieceInfo.Name(type);
    public override Vector2Int[] Shape => IdeaPieceInfo.Shape(type);
    public override Color Color => new Color(.8f, .55f, .95f);
}