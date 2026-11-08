using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine;

public class BoardView : MonoBehaviour
{
    readonly Dictionary<BoardPiece, BlockDefinition> placed = new Dictionary<BoardPiece, BlockDefinition>();
    readonly Dictionary<BoardPiece, BlockDefinition> sources = new Dictionary<BoardPiece, BlockDefinition>();
    BlockDefinition falling;
    BlockDefinition fallingSource;
    Camera boardCamera;

    void Awake()
    {
        boardCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (boardCamera == null) return;
        float size = Mathf.Min((Screen.height - 40f) / 10f, (Screen.width - 300f) / 10f);
        if (size <= 0) return;
        // Clear the whole frame; a partial viewport leaves old pixels outside it.
        boardCamera.rect = new Rect(0, 0, 1, 1);
        boardCamera.orthographicSize = Screen.height / (2f * size);
        float boardCenterX = 20f + 5f * size;
        float boardCenterY = Screen.height - 20f - 5f * size;
        boardCamera.transform.position = new Vector3(
            4.5f + (Screen.width * 0.5f - boardCenterX) / size,
            4.5f + (Screen.height * 0.5f - boardCenterY) / size,
            boardCamera.transform.position.z);
    }

    public void ShowFalling(BlockDefinition definition, Vector2Int[] cells)
    {
        if (falling == null || fallingSource != definition)
        {
            if (falling != null) Destroy(falling.gameObject);
            falling = Instantiate(definition, transform);
            fallingSource = definition;
            falling.name = "Falling - " + definition.Label;
        }

        Position(falling, cells);
    }

    public void ClearFalling()
    {
        if (falling != null) Destroy(falling.gameObject);
        falling = null;
        fallingSource = null;
    }

    public void Sync(IReadOnlyList<BoardPiece> pieces)
    {
        var removed = new List<BoardPiece>();
        foreach (var entry in placed)
            if (!pieces.Contains(entry.Key))
            {
                Destroy(entry.Value.gameObject);
                removed.Add(entry.Key);
            }

        foreach (var piece in removed)
        {
            placed.Remove(piece);
            sources.Remove(piece);
        }

        foreach (var piece in pieces)
        {
            if (placed.TryGetValue(piece, out var existing) && sources[piece] != piece.Definition)
            {
                Destroy(existing.gameObject);
                placed.Remove(piece);
                sources.Remove(piece);
            }

            if (!placed.TryGetValue(piece, out var view))
            {
                view = Instantiate(piece.Definition, transform);
                view.name = "Placed - " + piece.Definition.Label;
                placed.Add(piece, view);
                sources.Add(piece, piece.Definition);
            }

            Position(view, piece.Cells);
            var hearts = view.GetComponent<HeartDisplay>();
            if (hearts != null)
            {
                hearts.SetCells(piece.Cells);
                hearts.SetHealth(piece.Patience, piece.Definition.Patience);
            }
        }
    }

    public IEnumerator Pulse(IEnumerable<BoardPiece> pieces, Color tint)
    {
        var renderers = new List<SpriteRenderer>();
        var colors = new List<Color>();
        foreach (var piece in pieces)
        {
            if (!placed.TryGetValue(piece, out var view)) continue;
            foreach (Transform child in view.transform)
            {
                if (!child.name.StartsWith("Block Cell ") || !child.gameObject.activeInHierarchy) continue;
                var renderer = child.GetComponent<SpriteRenderer>();
                if (renderer == null) continue;
                renderers.Add(renderer);
                colors.Add(renderer.color);
                renderer.color = Color.Lerp(renderer.color, tint, .75f);
            }
        }

        if (renderers.Count == 0) yield break;
        yield return new WaitForSeconds(.18f);
        for (int i = 0; i < renderers.Count; i++)
            if (renderers[i] != null)
                renderers[i].color = colors[i];
    }

    static void Position(BlockDefinition view, Vector2Int[] cells)
    {
        for (int i = 0; i < view.transform.childCount && i < 4; i++)
        {
            var child = view.transform.GetChild(i);
            if (!child.name.StartsWith("Block Cell ")) continue;
            child.gameObject.SetActive(i < cells.Length);
            if (i < cells.Length) child.localPosition = new Vector3(cells[i].x, cells[i].y, -0.1f);
        }

        var art = view.GetComponent<BlockArt>();
        if (art != null) art.SetCells(cells);
        var outline = view.GetComponent<BlockOutline>();
        if (outline == null) outline = view.gameObject.AddComponent<BlockOutline>();
        outline.SetCells(cells);
    }
}