using System.Collections.Generic;
using UnityEngine;

public class StageProgress
{
    public const int StageCount = 12;
    public const int StagesPerRound = 3;

    public int Stage { get; private set; } = 1;
    public int Money { get; private set; }
    public int HumanTarget => GetHumanTarget(Stage);
    public int ScoreTarget => GetScoreTarget(Stage);
    public int Round => (Stage - 1) / StagesPerRound + 1;
    public bool IsBoss => IsBossStage(Stage);
    public bool IsFinalStage => Stage == StageCount;
    public readonly List<BossDebuff> BossDebuffs = new List<BossDebuff>();

    public static int GetHumanTarget(int stage) => stage + 1;
    public static int GetScoreTarget(int stage) => stage * 10;
    public static bool IsBossStage(int stage) => stage > 0 && stage % StagesPerRound == 0;

    public bool Cleared(GameSession session) => session.Humans >= HumanTarget && session.Score >= ScoreTarget;

    public void Reward(GameSession session)
    {
        Money += session.Humans * 2 + session.Score / 5;
    }

    public bool Buy(int price)
    {
        if (Money < price) return false;
        Money -= price;
        return true;
    }

    public void Next(GameSession session)
    {
        if (IsFinalStage) return;

        Stage++;
        session.ResetBoard();
        if (!IsBoss) return;

        foreach (var debuff in BossDebuffs)
        {
            debuff.OnStageStart(session);
        }
    }
}
