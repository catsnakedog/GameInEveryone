using System.Linq;
using UnityEngine;
using System.Text.RegularExpressions;

// Draws a compact inspection card for both placed pieces and delivery rewards.
public class PieceTooltip
{
    readonly ItemDropTable table;
    readonly GUIStyle title;
    readonly GUIStyle details;
    readonly GUIStyle description;
    Vector2 scroll;
    string lastSelection;

    public PieceTooltip(ItemDropTable table, GUIStyle label)
    {
        this.table = table;
        title = new GUIStyle(label) { fontSize = 23, fontStyle = FontStyle.Bold, wordWrap = true };
        details = new GUIStyle(label) { fontSize = 15, wordWrap = true };
        description = new GUIStyle(label) { fontSize = 18, wordWrap = true, richText = true };
        title.normal.textColor = new Color(1f, .87f, .45f);
        details.normal.textColor = new Color(.72f, .82f, .85f);
        description.normal.textColor = Color.white;
    }

    public ItemDropRow Find(BlockDefinition block)
    {
        if (table == null || table.items == null || block == null) return null;
        return table.items.FirstOrDefault(row => row != null && row.block == block);
    }

    public void Draw(Rect rect, ItemDropRow row, BlockDefinition block, BoardPiece piece, string source)
    {
        if (row != null && block == null) block = row.block;
        string name = row != null ? row.DisplayName : block != null ? block.Label : "기물";
        string effect = row != null && row.relic != null
            ? PieceDescriptions.For(row.relic)
            : PieceDescriptions.For(block);
        if (lastSelection != name)
        {
            scroll = Vector2.zero;
            lastSelection = name;
        }

        string rarity = row != null ? PieceDescriptions.Rarity(row.rarity) : "변신";
        string category = row != null && row.relic != null ? "유물" :
            block is HumanBlock ? "인간" :
            block is GameBlock ? "게임" : "특수";
        string health = piece != null && block is HumanBlock ? $"   인내심 {piece.Patience}/{block.Patience}" : "";
        string tags = row != null && row.tags != null && row.tags.Count > 0 ? "\n" + string.Join(" · ", row.tags) : "";
        string state = "";
        if (piece != null && IdeaBoardQueries.IsGameGrade(piece))
        {
            var tuning = Resources.Load<IdeaTuning>("IdeaTuning");
            int threshold = tuning != null ? tuning.playsPerGrowth : 5;
            state = IdeaBoardQueries.Is(piece, IdeaPieceType.Goty)
                ? $"\n플레이 횟수: {piece.PlayCount} (최종 등급)"
                : $"\n플레이 횟수: {piece.PlayCount}/{threshold}";
        }
        else if (piece != null && IdeaBoardQueries.Is(piece, IdeaPieceType.Refund))
        {
            state = $"\n누적 점수: {piece.AccumulatedScore}";
        }

        GUI.color = new Color(.11f, .16f, .19f, .98f);
        GUI.DrawTexture(rect, Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(rect.x + 16, rect.y + 12, rect.width - 32, 38), name, title);
        GUI.Label(new Rect(rect.x + 16, rect.y + 50, rect.width - 32, 52),
            $"{source}  ·  {category}  ·  {rarity}{health}{tags}", details);
        string content = Highlight(effect) + state;
        float viewportHeight = Mathf.Max(1f, rect.height - 155f);
        float contentWidth = Mathf.Max(1f, rect.width - 52f);
        float contentHeight = Mathf.Max(viewportHeight, description.CalcHeight(new GUIContent(content), contentWidth));
        var viewport = new Rect(rect.x + 16, rect.y + 109, rect.width - 32, viewportHeight);
        scroll = GUI.BeginScrollView(viewport, scroll, new Rect(0, 0, contentWidth, contentHeight), false,
            contentHeight > viewportHeight);
        GUI.Label(new Rect(0, 0, contentWidth, contentHeight), content, description);
        GUI.EndScrollView();
        if (block != null)
        {
            GUI.Label(new Rect(rect.x + 16, rect.yMax - 35, rect.width - 32, 24),
                $"주변: {block.Range}칸 · 맞닿음: 1칸 (대각선 포함)", details);
        }
    }

    static string Highlight(string text)
    {
        return Regex.Replace(text, @"[+-]?\d+(?:%|점|칸|명|회)?|주변|맞닿은|인내심|즉시 사라집니다",
            match => $"<color=#FFD66B>{match.Value}</color>");
    }
}