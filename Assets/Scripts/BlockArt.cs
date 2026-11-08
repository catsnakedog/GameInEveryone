using UnityEngine;

// Places a replaceable illustration on the topmost cell of a block.
public class BlockArt : MonoBehaviour
{
    [SerializeField] SpriteRenderer illustration;
    public Sprite Icon => illustration != null ? illustration.sprite : null;
    public void SetCells(Vector2Int[] cells)
    {
        if (illustration == null || cells == null || cells.Length == 0) return;
        var anchor = cells[0];
        foreach (var cell in cells)
            if (cell.y > anchor.y || (cell.y == anchor.y && cell.x < anchor.x)) anchor = cell;
        illustration.transform.localPosition = new Vector3(anchor.x, anchor.y, -.15f);
    }
}
