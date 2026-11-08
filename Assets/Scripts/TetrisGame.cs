using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(GameSession))]
public class TetrisGame : MonoBehaviour
{
    public BlockDefinition[] catalog;
    [SerializeField] BoxDropTable boxTable;
    readonly DeliveryShop delivery = new DeliveryShop();
    readonly List<BlockDefinition> deck = new List<BlockDefinition>();
    readonly Queue<BlockDefinition> next = new Queue<BlockDefinition>();
    GameSession session;
    BoardView boardView;
    StageProgress stage = new StageProgress();
    BlockDefinition current;
    Vector2Int origin;
    int rotation;
    float timer;
    bool shop, finished, lost, choosingStage = true, resolving;
    GUIStyle label;
    Font koreanFont;
    PieceTooltip pieceTooltip;

    void Start()
    {
        session = GetComponent<GameSession>();
        boardView = GetComponent<BoardView>();
        koreanFont = Resources.Load<Font>("NotoSansCJKkr-Regular");
        if (koreanFont == null)
        {
            koreanFont =
                Font.CreateDynamicFontFromOSFont(new[] { "Malgun Gothic", "Apple SD Gothic Neo", "Noto Sans CJK KR" },
                    18);
        }

        if (boxTable == null) boxTable = Resources.Load<BoxDropTable>("BoxDropTable");
        if (catalog == null || catalog.Length == 0)
        {
            Debug.LogError("Block prefab catalog is empty.");
            enabled = false;
            return;
        }

        foreach (var block in catalog)
        {
            // New idea-route items enter through deliveries; keep the starter deck stable.
            if (block is IIdeaBlock) continue;
            deck.Add(block);
            if (!(block is TelevisionBlock)) deck.Add(block);
        }

        for (int i = 0; i < 2; i++) Enqueue();
        Spawn();
    }

    void Enqueue() => next.Enqueue(deck[Random.Range(0, deck.Count)]);

    Vector2Int[] Cells(Vector2Int pos, int rot)
    {
        var shape = current.Shape.Select(v =>
        {
            for (int i = 0; i < rot; i++) v = new Vector2Int(v.y, -v.x);
            return v;
        }).ToArray();
        int minX = shape.Min(v => v.x), minY = shape.Min(v => v.y);
        return shape.Select(v => v + pos - new Vector2Int(minX, minY)).ToArray();
    }

    void Spawn()
    {
        current = next.Dequeue();
        Enqueue();
        rotation = 0;
        origin = new Vector2Int(3, 8);
        if (!session.Free(Cells(origin, rotation))) lost = true;
    }

    void Move(Vector2Int offset)
    {
        if (session.Free(Cells(origin + offset, rotation))) origin += offset;
    }

    void DropStep()
    {
        if (session.Free(Cells(origin + Vector2Int.down, rotation))) origin += Vector2Int.down;
        else Lock();
    }

    void Lock()
    {
        if (!resolving) StartCoroutine(ResolvePlacement());
    }

    IEnumerator ResolvePlacement()
    {
        resolving = true;
        session.Place(current, Cells(origin, rotation));
        if (boardView != null)
        {
            boardView.ClearFalling();
            boardView.Sync(session.Pieces);
        }

        yield return session.ResolveTurn(boardView);
        if (stage.Cleared(session))
        {
            stage.Reward(session);
            if (stage.IsFinalStage) finished = true;
            else shop = true;
        }
        else Spawn();

        timer = 0f;
        resolving = false;
    }

    void Update()
    {
        if (boardView != null)
        {
            boardView.Sync(session.Pieces);
            if (!resolving && !choosingStage && !shop && !finished && !lost)
                boardView.ShowFalling(current, Cells(origin, rotation));
            else boardView.ClearFalling();
        }

        if (resolving || choosingStage || shop || finished || lost || Keyboard.current == null) return;
        var k = Keyboard.current;
        if (k.leftArrowKey.wasPressedThisFrame) Move(Vector2Int.left);
        if (k.rightArrowKey.wasPressedThisFrame) Move(Vector2Int.right);
        if (k.upArrowKey.wasPressedThisFrame)
        {
            int r = (rotation + 1) % 4;
            if (session.Free(Cells(origin, r))) rotation = r;
        }

        if (k.downArrowKey.wasPressedThisFrame)
        {
            DropStep();
            return;
        }

        if (k.spaceKey.wasPressedThisFrame)
        {
            while (session.Free(Cells(origin + Vector2Int.down, rotation))) origin += Vector2Int.down;
            Lock();
            return;
        }

        timer += Time.deltaTime;
        if (timer >= 1f)
        {
            timer = 0;
            DropStep();
        }
    }

    void OnGUI()
    {
        if (session == null || current == null) return;
        if (koreanFont != null)
        {
            GUI.skin.font = koreanFont;
            GUI.skin.label.font = koreanFont;
            GUI.skin.button.font = koreanFont;
            GUI.skin.box.font = koreanFont;
        }

        if (label == null)
        {
            label = new GUIStyle(GUI.skin.label) { fontSize = 16, wordWrap = true };
            label.normal.textColor = Color.white;
            pieceTooltip = new PieceTooltip(boxTable != null ? boxTable.itemTable : null, label);
        }

        if (choosingStage)
        {
            DrawStageSelection();
            return;
        }

        if (shop)
        {
            DrawShop();
            return;
        }

        float size = Mathf.Min((Screen.height - 40f) / 10f, (Screen.width - 300f) / 10f);
        if (size < 8) return;
        float left = 20, top = 20;


        GUI.color = Color.white;
        float side = left + size * 10 + 20;
        GUI.Label(new Rect(side, top, 300, 90),
            $"스테이지 {stage.Stage}{(stage.IsBoss ? " · 보스" : "")}\n인원 {session.Humans} / {stage.HumanTarget}    재미 {session.Score} / {stage.ScoreTarget}\n보유금 {stage.Money}원",
            label);

        var hoveredBlock = current;
        BoardPiece hoveredPiece = null;
        string source = "현재 기물";
        Vector2 mouse = Event.current.mousePosition;
        var currentRect = new Rect(side, top + 92, 300, 26);
        GUI.Label(currentRect, $"현재: {current.Label}", label);
        int nextIndex = 0;
        foreach (var upcoming in next)
        {
            var nextRect = new Rect(side, top + 120 + nextIndex * 26, 300, 26);
            GUI.Label(nextRect, $"다음 {nextIndex + 1}: {upcoming.Label}", label);
            if (nextRect.Contains(mouse))
            {
                hoveredBlock = upcoming;
                source = $"다음 기물 {nextIndex + 1}";
            }

            nextIndex++;
        }

        var boardRect = new Rect(left, top, size * 10, size * 10);
        if (boardRect.Contains(mouse))
        {
            var cell = new Vector2Int(Mathf.FloorToInt((mouse.x - left) / size),
                9 - Mathf.FloorToInt((mouse.y - top) / size));
            hoveredPiece = session.Pieces.LastOrDefault(p => p.Cells.Contains(cell));
            if (hoveredPiece != null)
            {
                hoveredBlock = hoveredPiece.Definition;
                source = "배치된 기물";
            }
            else if (!resolving && !shop && !finished && !lost && Cells(origin, rotation).Contains(cell))
            {
                hoveredBlock = current;
                source = "현재 기물";
            }
        }

        float detailWidth = Mathf.Max(180f, Mathf.Min(500f, Screen.width - side - 20f));
        float detailHeight = Mathf.Max(190f, Mathf.Min(410f, Screen.height - top - 255f));
        GUI.Label(new Rect(side, top + 181, detailWidth, 52), "기물에 마우스를 올려 자세히 보기\n← → 이동  ↑ 회전  ↓ 내리기  Space 즉시 낙하",
            label);
        pieceTooltip.Draw(new Rect(side, top + 240, detailWidth, detailHeight), pieceTooltip.Find(hoveredBlock),
            hoveredBlock, hoveredPiece, source);

        if (lost || finished)
        {
            GUI.Box(new Rect(60, 80, 350, 130), finished ? "모든 스테이지 클리어!" : "보드가 가득 찼습니다!");
            if (GUI.Button(new Rect(100, 140, 270, 40), "다시 시작"))
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager
                    .GetActiveScene().buildIndex);
        }
    }

    void DrawBackdrop(string title, string subtitle)
    {
        GUI.color = new Color(.04f, .09f, .12f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;
        var heading = new GUIStyle(label)
            { fontSize = 32, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
        var subheading = new GUIStyle(label) { fontSize = 16, alignment = TextAnchor.MiddleCenter };
        GUI.Label(new Rect(0, 24, Screen.width, 50), title, heading);
        GUI.Label(new Rect(0, 74, Screen.width, 32), subtitle, subheading);
    }

    void DrawStageSelection()
    {
        DrawBackdrop("스테이지 선택",
            $"라운드 {stage.Round} / {StageProgress.StageCount / StageProgress.StagesPerRound}  ·  스테이지 {stage.Stage} / {StageProgress.StageCount}");
        float width = Mathf.Min(230f, (Screen.width - 80f) / StageProgress.StagesPerRound);
        float gap = 16f, total = width * StageProgress.StagesPerRound + gap * (StageProgress.StagesPerRound - 1);
        float x0 = (Screen.width - total) * .5f;
        float y = Mathf.Max(120f, (Screen.height - 340f) * .5f);
        var cardText = new GUIStyle(label) { alignment = TextAnchor.UpperCenter, fontSize = 18 };
        int firstStage = (stage.Round - 1) * StageProgress.StagesPerRound + 1;
        for (int index = 0; index < StageProgress.StagesPerRound; index++)
        {
            int i = firstStage + index;
            float x = x0 + index * (width + gap);
            bool active = stage.Stage == i;
            GUI.color = i < stage.Stage ? new Color(.12f, .27f, .20f) :
                active ? new Color(.24f, .32f, .38f) : new Color(.12f, .15f, .18f);
            GUI.DrawTexture(new Rect(x, y, width, 300), Texture2D.whiteTexture);
            GUI.color = Color.white;
            string state = i < stage.Stage ? "클리어" : active ? "도전 가능" : "잠김";
            GUI.Label(new Rect(x + 10, y + 25, width - 20, 180),
                $"{(StageProgress.IsBossStage(i) ? "보스 " : "스테이지 ") + i}\n\n인원: {StageProgress.GetHumanTarget(i)}\n재미: {StageProgress.GetScoreTarget(i)}\n\n{state}",
                cardText);
            if (active && GUI.Button(new Rect(x + 20, y + 225, width - 40, 48), "시작")) choosingStage = false;
        }

        GUI.Label(new Rect(0, y + 320, Screen.width, 30), $"보유금: {stage.Money}원",
            new GUIStyle(label) { alignment = TextAnchor.MiddleCenter });
    }

    void DrawShop()
    {
        if (delivery.Choices.Count > 0)
        {
            DrawDelivery();
            return;
        }

        DrawBackdrop("상점", $"스테이지 {stage.Stage} 클리어  ·  보유금 {stage.Money}원");
        float x = (Screen.width - 300f) * .5f;
        GUI.color = new Color(.14f, .20f, .24f);
        GUI.DrawTexture(new Rect(x, 135, 300, 220), Texture2D.whiteTexture);
        GUI.color = Color.white;
        string boxName = boxTable != null && boxTable.boxName != "Delivery box" ? boxTable.boxName : "택배 상자";
        string description = boxTable == null
            ? "상자 데이터가 없습니다."
            : $"{boxName}\n\n무작위 기물 {boxTable.choiceCount}개 등장\n하나를 골라 덱에 추가합니다";
        GUI.Label(new Rect(x + 20, 155, 260, 120), description,
            new GUIStyle(label) { alignment = TextAnchor.UpperCenter });
        if (boxTable != null && GUI.Button(new Rect(x + 35, 285, 230, 45), $"택배 열기  {boxTable.price}원"))
            delivery.Open(boxTable, stage);
        if (!string.IsNullOrEmpty(delivery.Error))
            GUI.Label(new Rect(x - 70, 365, 440, 50), delivery.Error,
                new GUIStyle(label) { alignment = TextAnchor.UpperCenter });
        if (GUI.Button(new Rect((Screen.width - 220) * .5f, Mathf.Min(Screen.height - 65, 435), 220, 45), "다음 스테이지"))
        {
            stage.Next(session);
            shop = false;
            choosingStage = true;
            Spawn();
        }
    }

    void DrawDelivery()
    {
        DrawBackdrop("택배 도착!", "기물 하나를 골라 덱에 추가하세요");
        int count = delivery.Choices.Count;
        float width = Mathf.Min(210f, (Screen.width - 60f) / count - 12f);
        float gap = 12f, x0 = (Screen.width - (width * count + gap * (count - 1))) * .5f;
        ItemDropRow inspected = delivery.Choices[0];
        for (int i = 0; i < count; i++)
        {
            var item = delivery.Choices[i];
            float x = x0 + i * (width + gap), y = 150;
            if (new Rect(x, y, width, 260).Contains(Event.current.mousePosition)) inspected = item;
            GUI.color = new Color(.14f, .20f, .24f);
            GUI.DrawTexture(new Rect(x, y, width, 260), Texture2D.whiteTexture);
            GUI.color = Color.white;
            var art = item.block != null ? item.block.GetComponent<BlockArt>() : null;
            var icon = art != null ? art.Icon : item.relic != null ? item.relic.Icon : null;
            if (icon != null)
                GUI.DrawTexture(new Rect(x + (width - 90) * .5f, y + 12, 90, 90), icon.texture, ScaleMode.ScaleToFit,
                    true);
            string size = item.block != null ? item.block.Shape.Length + "칸" : "유물";
            GUI.Label(new Rect(x + 10, y + 108, width - 20, 90),
                $"{item.DisplayName}\n{PieceDescriptions.Rarity(item.rarity)}\n{size}",
                new GUIStyle(label) { alignment = TextAnchor.UpperCenter });
            if (GUI.Button(new Rect(x + 15, y + 205, width - 30, 40), "선택"))
            {
                var selected = delivery.ChooseItem(i);
                if (selected != null && selected.block != null) deck.Add(selected.block);
                if (selected != null && selected.relic != null) session.Relics.Add(selected.relic.CreateRelic());
                return;
            }
        }

        float tooltipWidth = Mathf.Min(720f, Screen.width - 60f);
        float tooltipHeight = Mathf.Max(180f, Mathf.Min(350f, Screen.height - 440f));
        pieceTooltip.Draw(new Rect((Screen.width - tooltipWidth) * .5f, 425, tooltipWidth, tooltipHeight), inspected,
            inspected.block, null, "택배 후보");
    }
}