using UnityEngine;

[CreateAssetMenu(menuName = "Game In Everyone/Idea Tuning")]
public class IdeaTuning : ScriptableObject
{
    [Min(1)] public int playsPerGrowth = 5;
    public int mattContactBonus = 40;
    public int baronScoreBonus = 10;
    [Range(0f, 1f)] public float mushroomDeathChance = .2f;
    [Range(0f, 1f)] public float developerSpawnChance = .3f;
    [Range(0f, 1f)] public float ceremonyGrowthChance = .1f;
}