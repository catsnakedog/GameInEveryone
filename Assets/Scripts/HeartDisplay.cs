using System.Collections.Generic;
using UnityEngine;

// Attach to a block root and call SetHealth whenever that block's health changes.
public class HeartDisplay : MonoBehaviour
{
    [SerializeField, Min(1)] int maximumHearts = 3;
    [SerializeField] Sprite fullHeart;
    [SerializeField] Sprite emptyHeart;
    readonly List<SpriteRenderer> hearts = new List<SpriteRenderer>();
    int current;
    Vector2Int[] positions;

    void Awake()
    {
        if (fullHeart == null) fullHeart = Resources.Load<Sprite>("HeartFull");
        if (emptyHeart == null) emptyHeart = Resources.Load<Sprite>("HeartEmpty");
        current = maximumHearts;
    }
    public void SetHealth(int value, int maximum)
    {
        maximumHearts = Mathf.Max(1, maximum);
        current = Mathf.Clamp(value, 0, maximumHearts);
        Refresh();
    }
    public void SetCells(Vector2Int[] cells) { positions = cells; Refresh(); }
    void Refresh()
    {
        if (positions == null || positions.Length == 0 || fullHeart == null || emptyHeart == null) return;
        while (hearts.Count < maximumHearts)
        {
            var go = new GameObject("Heart " + (hearts.Count + 1));
            go.transform.SetParent(transform, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 10;
            hearts.Add(renderer);
        }
        Vector2Int anchor = positions[0];
        foreach (var cell in positions)
            if (cell.y < anchor.y || (cell.y == anchor.y && cell.x > anchor.x)) anchor = cell;
        float step = Mathf.Min(.2f, .75f / maximumHearts);
        float scale = step * .95f;
        for (int i = 0; i < hearts.Count; i++)
        {
            var h = hearts[i];
            h.gameObject.SetActive(i < maximumHearts);
            if (i >= maximumHearts) continue;
            h.sprite = i < current ? fullHeart : emptyHeart;
            h.color = i < current ? new Color(1f, .25f, .32f) : new Color(.8f, .82f, .85f);
            h.transform.localScale = Vector3.one * scale;
            h.transform.localPosition = new Vector3(anchor.x + .39f - step * (maximumHearts - 1 - i), anchor.y - .32f, -.2f);
        }
    }
}
