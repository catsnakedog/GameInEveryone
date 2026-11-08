using UnityEngine;

public enum IdeaPieceType
{
    Lol,
    Rung,
    Carry,
    Matt,
    Yumi,
    Faker,
    Worshipper,
    Hack,
    Trophy,
    Mushroom,
    GradeC,
    GradeB,
    GradeA,
    GradeAA,
    GradeAAA,
    Goty,
    AiSlop,
    Critic,
    Developer,
    Influencer,
    Ceremony,
    ChatGpt,
    Refund,
    Taxi
}

public static class IdeaPieceInfo
{
    static readonly string[] names =
    {
        "LOL", "렁키즈", "캐리머신", "메튜렁", "유미장인", "훼이커", "숭배자", "핵", "트로피", "버섯",
        "C급 게임", "B급 게임", "A급 게임", "AA급 게임", "AAA급 게임", "GOTY 게임", "AI 슬롭", "평론가", "게임개발자",
        "인플루언서", "시상식", "ChatGPT", "환불런", "택시보고놀란아저씨"
    };

    public static string Name(IdeaPieceType type) => names[(int)type];

    public static Vector2Int[] Shape(IdeaPieceType type)
    {
        switch (type)
        {
            case IdeaPieceType.Rung: return new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0) };
            case IdeaPieceType.Lol:
            case IdeaPieceType.GradeC:
            case IdeaPieceType.GradeB:
            case IdeaPieceType.GradeA:
            case IdeaPieceType.GradeAA:
            case IdeaPieceType.GradeAAA:
            case IdeaPieceType.Goty:
            case IdeaPieceType.AiSlop:
            case IdeaPieceType.Hack:
            case IdeaPieceType.Trophy:
            case IdeaPieceType.Mushroom:
            case IdeaPieceType.Ceremony:
            case IdeaPieceType.ChatGpt:
            case IdeaPieceType.Refund:
                return new[] { new Vector2Int(0, 0) };
            default: return new[] { new Vector2Int(0, 0), new Vector2Int(1, 0) };
        }
    }

    public static int GradePoints(IdeaPieceType type)
    {
        switch (type)
        {
            case IdeaPieceType.GradeC: return 10;
            case IdeaPieceType.GradeB: return 30;
            case IdeaPieceType.GradeA: return 100;
            case IdeaPieceType.GradeAA: return 300;
            case IdeaPieceType.GradeAAA: return 500;
            case IdeaPieceType.Goty: return 1000;
            case IdeaPieceType.AiSlop: return -50;
            default: return 0;
        }
    }

    public static bool IsHighQualityGame(BoardPiece piece)
    {
        if (piece.Definition is GoodGameBlock) return true;
        if (!(piece.Definition is IIdeaBlock idea)) return false;
        return (int)idea.IdeaType >= (int)IdeaPieceType.GradeA && (int)idea.IdeaType <= (int)IdeaPieceType.Goty;
    }
}

public interface IIdeaBlock
{
    IdeaPieceType IdeaType { get; }
}