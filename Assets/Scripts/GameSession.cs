using System.Collections.Generic;
using System.Linq;
using System.Collections;
using UnityEngine;

public class BoardPiece
{
    public BlockDefinition Definition;
    public Vector2Int[] Cells;
    public int Patience;

    public bool Active;

    // State belongs to the placed piece, not its shared prefab.
    public int PlayCount;
    public int AccumulatedScore;
    public BlockDefinition RestoreAfterTurn;
}

public class GameSession : MonoBehaviour
{
    public readonly List<BoardPiece> Pieces = new List<BoardPiece>();
    public readonly List<Relic> Relics = new List<Relic>();
    public int Score { get; private set; }
    public int Humans => Pieces.Count(p => p.Definition is HumanBlock);

    public void AddScore(int points)
    {
        Score += points;
    }

    public void AddScoreFor(BoardPiece source, int points)
    {
        foreach (var relic in Relics) points = relic.ModifyScore(source, points);
        Score += points;
    }

    public void ResetBoard()
    {
        Pieces.Clear();
        Score = 0;
    }

    public bool Free(Vector2Int[] cells) => cells.All(c =>
        c.x >= 0 && c.x < 10 && c.y >= 0 && c.y < 10 && !Pieces.Any(p => p.Cells.Contains(c)));

    public void Place(BlockDefinition definition, Vector2Int[] cells)
    {
        Pieces.Add(new BoardPiece { Definition = definition, Cells = cells, Patience = definition.Patience });
    }

    bool Near(BoardPiece a, BoardPiece b, int range) => a.Cells.Any(x =>
        b.Cells.Any(y => Mathf.Max(Mathf.Abs(x.x - y.x), Mathf.Abs(x.y - y.y)) <= range));

    internal IEnumerable<BoardPiece> GamesFor(BoardPiece human)
    {
        var games = Pieces.Where(p => p.Definition is GameBlock).ToList();
        var result = new HashSet<BoardPiece>(games.Where(g => Near(human, g, human.Definition.Range)));
        foreach (var tv in Pieces.Where(p => p.Definition is TelevisionBlock && Near(human, p, p.Definition.Range)))
        {
            var farthest = games
                .OrderByDescending(g => tv.Cells.SelectMany(t => g.Cells.Select(c => (t - c).sqrMagnitude)).Min())
                .FirstOrDefault();
            if (farthest != null) result.Add(farthest);
        }

        return result;
    }

    public IEnumerator ResolveTurn(BoardView view)
    {
        foreach (var piece in Pieces)
        {
            if (piece.RestoreAfterTurn == null) continue;
            piece.Definition = piece.RestoreAfterTurn;
            piece.RestoreAfterTurn = null;
        }

        if (view != null)
        {
            view.Sync(Pieces);
            var order = new[]
            {
                BlockKind.GoodGame, BlockKind.BadGame, BlockKind.NormalGame, BlockKind.IdeaGame, BlockKind.Television,
                BlockKind.IdeaSpecial
            };
            foreach (var kind in order)
            {
                var group = Pieces.Where(p => p.Definition.Kind == kind).ToList();
                yield return view.Pulse(group, kind == BlockKind.Television ? new Color(.9f, .5f, 1f) : Color.white);
            }
        }

        var humans = Pieces.Where(p => p.Definition is HumanBlock).ToList();
        var dead = new List<BoardPiece>();
        foreach (var h in humans.Where(h => !(h.Definition is StreamerBlock) && !(h.Definition is IdeaHumanBlock)))
        {
            var games = GamesFor(h).ToList();
            bool bad = games.Any(g => g.Definition is BadGameBlock);
            if (h.Definition is GoodFanBlock && bad)
            {
                h.Patience = 0;
                dead.Add(h);
                continue;
            }

            h.Active = h.Definition is BadFanBlock ? bad :
                h.Definition is GoodFanBlock ? games.Any(IdeaPieceInfo.IsHighQualityGame) : games.Count > 0;
            if (h.Active)
            {
                h.Patience = h.Definition.Patience;
                Score += h.Definition is OrdinaryPersonBlock && games.Any(g => g.Definition is NormalGameBlock) ? 4 : 2;
            }
            else if (h.Definition is BadFanBlock && --h.Patience <= 0) dead.Add(h);
        }

        foreach (var h in humans.Where(h => h.Definition is StreamerBlock))
        {
            h.Active = GamesFor(h).Any() && humans.Any(other =>
                other != h && !(other.Definition is StreamerBlock) && !dead.Contains(other) && other.Active &&
                Near(h, other, h.Definition.Range));
            if (h.Active)
            {
                h.Patience = h.Definition.Patience;
                Score += 3;
            }
            else if (--h.Patience <= 0) dead.Add(h);
        }

        IdeaTurnRules.ResolveLol(this, dead);
        GradeRouteRules.Resolve(this, dead, Resources.Load<IdeaTuning>("IdeaTuning"));
        if (view != null)
        {
            view.Sync(Pieces);
            yield return view.Pulse(humans.Where(h => h.Active || dead.Contains(h)), Color.yellow);
        }

        foreach (var h in dead) Pieces.Remove(h);
        if (view != null) view.Sync(Pieces);
        foreach (var relic in Relics) relic.OnTurn(this);
    }
}