using UnityEngine;

public abstract class RelicDefinition : ScriptableObject
{
    public abstract string Label { get; }
    public abstract Sprite Icon { get; }
    public abstract Relic CreateRelic();
}