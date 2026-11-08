using System;
using System.Collections.Generic;
using UnityEngine;

public enum ItemRarity
{
    Common,
    Rare,
    Epic
}

[Serializable]
public class ItemDropRow
{
    public BlockDefinition block;
    public RelicDefinition relic;
    public ItemRarity rarity;
    [Min(0)] public float weight = 1f;
    public List<string> tags = new List<string>();
    public bool HasReward => (block != null) != (relic != null);
    public string DisplayName => block != null ? block.Label : relic != null ? relic.Label : "기물 정보 없음";
}

[CreateAssetMenu(menuName = "Game In Everyone/Drop/Item Table")]
public class ItemDropTable : ScriptableObject
{
    public List<ItemDropRow> items = new List<ItemDropRow>();
}