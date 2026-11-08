using System.Collections.Generic;
using UnityEngine;

// Draws only the outer boundary of the cells belonging to one block.
public class BlockOutline : MonoBehaviour
{
    static readonly Vector2Int[] Neighbors = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
    readonly List<SpriteRenderer> segments = new List<SpriteRenderer>();
    Sprite square;

    public void SetCells(Vector2Int[] cells)
    {
        if (cells == null || cells.Length == 0) return;
        if (square == null) square = Resources.Load<Sprite>("BoardSquare");
        if (square == null) return;
        var occupied = new HashSet<Vector2Int>(cells);
        int used = 0;
        foreach (var cell in cells)
        {
            for (int side = 0; side < Neighbors.Length; side++)
            {
                if (occupied.Contains(cell + Neighbors[side])) continue;
                if (used == segments.Count)
                {
                    var edge = new GameObject("Outline " + used);
                    edge.transform.SetParent(transform, false);
                    var renderer = edge.AddComponent<SpriteRenderer>();
                    renderer.sprite = square;
                    renderer.color = new Color(.025f,.025f,.035f);
                    renderer.sortingOrder = 5;
                    segments.Add(renderer);
                }
                var segment = segments[used++];
                segment.gameObject.SetActive(true);
                var direction = Neighbors[side];
                segment.transform.localPosition = new Vector3(cell.x + direction.x * .435f, cell.y + direction.y * .435f, -.18f);
                segment.transform.localScale = direction.x == 0 ? new Vector3(.93f, .065f, 1f) : new Vector3(.065f, .93f, 1f);
            }
        }
        for (int i=used;i<segments.Count;i++) segments[i].gameObject.SetActive(false);
    }
}
