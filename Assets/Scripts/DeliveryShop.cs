using System.Collections.Generic;

public class DeliveryShop
{
    public IReadOnlyList<ItemDropRow> Choices => choices;
    public string Error { get; private set; }
    readonly List<ItemDropRow> choices = new List<ItemDropRow>();

    public bool Open(BoxDropTable box, StageProgress stage)
    {
        Error = null;
        if (choices.Count != 0)
        {
            Error = "먼저 기물 하나를 선택하세요.";
            return false;
        }

        if (box == null)
        {
            Error = "상자 데이터가 없습니다.";
            return false;
        }

        if (stage.Money < box.price)
        {
            Error = "돈이 부족합니다.";
            return false;
        }

        if (!DropRoller.TryRoll(box, out var rolled, out var error))
        {
            Error = error;
            return false;
        }

        if (!stage.Buy(box.price))
        {
            Error = "돈이 부족합니다.";
            return false;
        }

        choices.AddRange(rolled);
        return true;
    }

    public BlockDefinition Choose(int index)
    {
        var row = ChooseItem(index);
        return row != null ? row.block : null;
    }

    public ItemDropRow ChooseItem(int index)
    {
        if (index < 0 || index >= choices.Count) return null;
        var selected = choices[index];
        choices.Clear();
        return selected;
    }
}