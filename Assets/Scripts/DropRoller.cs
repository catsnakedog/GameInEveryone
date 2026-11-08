using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DropRoller
{
    public static bool TryRoll(BoxDropTable box, out List<ItemDropRow> results, out string error)
    {
        results = new List<ItemDropRow>();
        error = null;
        if (box == null || box.itemTable == null || box.itemTable.items == null || box.rarityRates == null ||
            box.choiceCount < 1 || box.price < 0)
        {
            error = "상자 데이터가 올바르지 않습니다.";
            return false;
        }

        var rates = box.rarityRates.Where(r => r != null && ValidWeight(r.weight) && r.weight > 0).ToList();
        if (rates.Count == 0 || rates.GroupBy(r => r.rarity).Any(g => g.Count() > 1))
        {
            error = "등급별 확률 설정을 확인하세요.";
            return false;
        }

        foreach (var rate in rates)
        {
            if (!box.itemTable.items.Any(i =>
                    i != null && i.HasReward && i.rarity == rate.rarity && ValidWeight(i.weight) && i.weight > 0))
            {
                error = $"{PieceDescriptions.Rarity(rate.rarity)} 등급에 등장 가능한 기물이 없습니다.";
                return false;
            }
        }

        float total = rates.Sum(r => r.weight);
        if (!ValidWeight(total) || total <= 0)
        {
            error = "등급별 확률 설정을 확인하세요.";
            return false;
        }

        for (int n = 0; n < box.choiceCount; n++)
        {
            var rarity = Pick(rates, r => r.weight);
            var pool = box.itemTable.items.Where(i =>
                    i != null && i.HasReward && i.rarity == rarity.rarity && ValidWeight(i.weight) && i.weight > 0)
                .ToList();
            float itemTotal = pool.Sum(i => i.weight);
            if (!ValidWeight(itemTotal) || itemTotal <= 0)
            {
                results.Clear();
                error = $"{PieceDescriptions.Rarity(rarity.rarity)} 등급의 기물 가중치를 확인하세요.";
                return false;
            }

            results.Add(Pick(pool, i => i.weight));
        }

        return true;
    }

    static bool ValidWeight(float value) => !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0;

    static T Pick<T>(List<T> rows, System.Func<T, float> weight)
    {
        float remaining = Random.value * rows.Sum(weight);
        foreach (var row in rows)
        {
            remaining -= weight(row);
            if (remaining < 0) return row;
        }

        return rows[rows.Count - 1];
    }
}