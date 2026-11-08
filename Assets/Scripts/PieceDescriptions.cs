using UnityEngine;

// Effect text follows the implemented rules; tuning values are read from the same asset.
public static class PieceDescriptions
{
    static IdeaTuning tuning;

    static IdeaTuning Tuning
    {
        get
        {
            if (tuning == null) tuning = Resources.Load<IdeaTuning>("IdeaTuning");
            return tuning;
        }
    }

    public static string Rarity(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return "일반";
            case ItemRarity.Rare: return "레어";
            case ItemRarity.Epic: return "희귀";
            default: return "미정";
        }
    }

    public static string For(BlockDefinition block)
    {
        if (block == null) return "설명이 없습니다.";
        if (block is IIdeaBlock idea) return ForIdea(idea.IdeaType);

        switch (block.Kind)
        {
            case BlockKind.BadGame:
                return "주변의 똥겜충을 만족시킵니다. 갓겜충은 이 게임을 만나면 즉시 사라집니다.";
            case BlockKind.GoodGame:
                return "주변 인간에게 게임으로 취급됩니다. 갓겜충은 이 게임과 상호작용하면 +2점을 얻습니다.";
            case BlockKind.NormalGame:
                return "주변 인간에게 게임으로 취급됩니다. 일반인이 이 게임과 상호작용하면 +4점을 얻습니다.";
            case BlockKind.GoodFan:
                return "주변에 똥겜이 있으면 즉시 사라집니다. A급 이상 게임이나 갓겜이 있으면 +2점과 인내심을 얻습니다. 등급 게임의 점수는 별도로 더해집니다.";
            case BlockKind.BadFan:
                return "주변에 똥겜이 있으면 +2점과 인내심을 얻습니다. 없으면 인내심이 1 줄어듭니다. 인내심이 0이 되면 사라집니다.";
            case BlockKind.Streamer:
                return "주변에 게임과 그 턴에 활성화된 다른 인간이 있으면 +3점과 인내심을 얻습니다. 조건을 만족하지 못하면 인내심이 1 줄어듭니다.";
            case BlockKind.Television:
                return "가장 먼 게임 1개를 연결합니다. 주변 인간은 그 게임이 곁에 있는 것처럼 상호작용합니다.";
            case BlockKind.OrdinaryPerson:
                return "주변에 평범겜이 있으면 +4점. 평범겜 없이 다른 게임이 있으면 +2점을 얻습니다. 게임이 없어도 사라지지 않습니다.";
            default:
                return "설명이 없습니다.";
        }
    }

    public static string For(RelicDefinition relic)
    {
        if (relic is BaronRelicDefinition)
        {
            int bonus = Tuning != null ? Tuning.baronScoreBonus : 10;
            return $"LOL 기물이 점수를 얻거나 잃을 때마다 점수를 {bonus:+#;-#;0}만큼 바꿉니다. 여러 개를 얻으면 효과가 중첩됩니다.";
        }

        return "유물의 효과가 아직 없습니다.";
    }

    static string ForIdea(IdeaPieceType type)
    {
        int growth = Tuning != null ? Tuning.playsPerGrowth : 5;
        int matt = Tuning != null ? Tuning.mattContactBonus : 40;
        int mushroom = Mathf.RoundToInt((Tuning != null ? Tuning.mushroomDeathChance : .2f) * 100);
        int developer = Mathf.RoundToInt((Tuning != null ? Tuning.developerSpawnChance : .3f) * 100);
        int ceremony = Mathf.RoundToInt((Tuning != null ? Tuning.ceremonyGrowthChance : .1f) * 100);

        switch (type)
        {
            case IdeaPieceType.Lol:
                return "주변의 렁키즈와 캐리머신이 점수를 얻을 수 있게 합니다.";
            case IdeaPieceType.Rung:
                return "주변에 LOL이 있으면 +20점. 맞닿은 메튜렁 하나당 추가 점수를 얻습니다.";
            case IdeaPieceType.Carry:
                return "주변에 LOL이 있으면 30 + 주변 렁키즈 수² × 100점. 렁키즈가 수용 한계를 넘으면 즉시 사라집니다. 기본 한계는 1명입니다.";
            case IdeaPieceType.Matt:
                return $"맞닿은 렁키즈마다 그 렁키즈가 +{matt}점을 더 얻습니다.";
            case IdeaPieceType.Yumi:
                return "맵에 캐리머신이 있으면 +20점. 모든 캐리머신의 렁키즈 수용 한계를 1 늘립니다. 캐리머신이 없으면 -10점, 이때 렁키즈와 맞닿으면 -30점 후 사라집니다.";
            case IdeaPieceType.Faker:
                return "맵의 다른 LOL 인간을 모두 숭배자로 바꿉니다. 훼이커 자신은 바뀌지 않습니다.";
            case IdeaPieceType.Worshipper:
                return "+50점. 훼이커가 없으면 인내심이 1 줄어듭니다. 훼이커와 맞닿은 트로피 수만큼 점수가 배가됩니다.";
            case IdeaPieceType.Hack:
                return "맞닿은 렁키즈를 캐리머신으로도 취급합니다. 렁키즈의 효과와 판정은 그대로 유지됩니다.";
            case IdeaPieceType.Trophy:
                return "훼이커와 맞닿으면 모든 숭배자의 점수를 연결된 트로피 수만큼 곱합니다.";
            case IdeaPieceType.Mushroom:
                return $"주변 LOL 기물의 점수를 2배로 만듭니다. 대상마다 {mushroom}% 확률로 그 기물을 사라지게 합니다.";
            case IdeaPieceType.GradeC:
            case IdeaPieceType.GradeB:
            case IdeaPieceType.GradeA:
            case IdeaPieceType.GradeAA:
            case IdeaPieceType.GradeAAA:
                return
                    $"활성화된 인간이 이 게임을 이용할 때마다 +{IdeaPieceInfo.GradePoints(type)}점과 플레이 횟수 1을 얻습니다. 플레이 횟수가 {growth}에 도달하면 다음 등급으로 성장합니다.";
            case IdeaPieceType.Goty:
                return "활성화된 인간이 이용할 때마다 +1000점. 맞닿은 갓겜충을 다음 턴 전까지 택시보고놀란아저씨로 바꿉니다.";
            case IdeaPieceType.AiSlop:
                return "활성화된 인간이 이 게임을 이용할 때마다 -50점. 성장하지 않습니다.";
            case IdeaPieceType.Critic:
                return "맞닿은 등급 게임과 AI 슬롭의 평가 횟수를 각각 5 늘립니다. 각 게임 기본 점수의 절반을 합산해 얻습니다.";
            case IdeaPieceType.Developer:
                return $"매 턴 {developer}% 확률로 빈 칸에 C~A급 게임을 만듭니다. ChatGPT와 맞닿으면 A급 게임 또는 AI 슬롭을 같은 확률로 만듭니다.";
            case IdeaPieceType.Influencer:
                return "주변 등급 게임과 AI 슬롭의 평가 횟수를 각각 10 늘립니다.";
            case IdeaPieceType.Ceremony:
                return $"맞닿은 성장 가능 게임마다 {ceremony}% 확률로 한 단계 성장시킵니다.";
            case IdeaPieceType.ChatGpt:
                return "맞닿은 게임개발자가 만드는 게임을 A급 게임 또는 AI 슬롭으로 바꿉니다. 각각 50% 확률입니다.";
            case IdeaPieceType.Refund:
                return "맞닿은 게임을 제거하고 그 게임의 기본 점수만큼 누적합니다. 매 턴 누적 점수를 얻습니다. 음수 점수는 누적하지 않습니다.";
            case IdeaPieceType.Taxi:
                return "GOTY 게임에 맞닿은 갓겜충의 임시 모습입니다. 다음 턴 시작 시 갓겜충으로 돌아옵니다.";
            default:
                return "설명이 없습니다.";
        }
    }
}