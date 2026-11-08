using UnityEngine;

public enum BlockKind
{
    BadGame,
    GoodGame,
    NormalGame,
    GoodFan,
    BadFan,
    Streamer,
    Television,
    OrdinaryPerson,
    IdeaGame,
    IdeaHuman,
    IdeaSpecial
}

public abstract class BlockDefinition : MonoBehaviour
{
    public abstract BlockKind Kind { get; }
    public virtual int Range => 2;
    public virtual int Patience => 3;
    public virtual int Price => 4;
    public abstract Vector2Int[] Shape { get; }

    public virtual Color Color => (int)Kind <= (int)BlockKind.NormalGame ? new Color(.3f, .65f, .95f) :
        Kind == BlockKind.Television ? new Color(.8f, .55f, .95f) : new Color(1f, .7f, .35f);

    public virtual string Label => new[]
    {
        "똥겜", "갓겜", "평범겜", "갓겜충", "똥겜충", "인방러", "텔레비전", "일반인"
    }[(int)Kind];
}

public abstract class HumanBlock : BlockDefinition
{
}

public abstract class GameBlock : BlockDefinition
{
}

public abstract class SpecialBlock : BlockDefinition
{
}

public abstract class Relic
{
    public virtual int ModifyScore(BoardPiece source, int score) => score;

    public virtual void OnTurn(GameSession session)
    {
    }
}

public abstract class BossDebuff
{
    public virtual void OnStageStart(GameSession session)
    {
    }
}