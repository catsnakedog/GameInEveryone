using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RarityDropRate
{
    public ItemRarity rarity;
    [Min(0)] public float weight;
}

[CreateAssetMenu(menuName = "Game In Everyone/Drop/Box Table")]
public class BoxDropTable : ScriptableObject
{
    public string boxName = "Delivery box";
    [Min(0)] public int price = 4;
    [Min(1)] public int choiceCount = 3;
    public ItemDropTable itemTable;
    public List<RarityDropRate> rarityRates = new List<RarityDropRate>();
}
