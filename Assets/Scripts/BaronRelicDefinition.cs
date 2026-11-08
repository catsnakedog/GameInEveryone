using UnityEngine;

[CreateAssetMenu(menuName = "Game In Everyone/Relics/Baron")]
public class BaronRelicDefinition : RelicDefinition
{
    [SerializeField] Sprite icon;

    public override string Label => "바론 버프";
    public override Sprite Icon => icon;

    public override Relic CreateRelic()
    {
        var tuning = Resources.Load<IdeaTuning>("IdeaTuning");
        return new BaronRelic(tuning != null ? tuning.baronScoreBonus : 10);
    }
}

public class BaronRelic : Relic
{
    readonly int bonus;

    public BaronRelic(int bonus)
    {
        this.bonus = bonus;
    }

    public override int ModifyScore(BoardPiece source, int score)
    {
        if (source.Definition is IIdeaBlock idea && (int)idea.IdeaType <= (int)IdeaPieceType.Mushroom)
        {
            return score + bonus;
        }

        return score;
    }
}