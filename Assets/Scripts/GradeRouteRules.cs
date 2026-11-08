using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class GradeRouteRules
{
    static BlockDefinition Find(GameSession session, IdeaPieceType type)
    {
        var game = session.GetComponent<TetrisGame>();
        return game == null ? null : game.catalog.FirstOrDefault(p => p is IIdeaBlock b && b.IdeaType == type);
    }

    static bool IsGradeGame(BoardPiece p) => IdeaBoardQueries.IsGameGrade(p);
    static bool IsGradeOrSlop(BoardPiece p) => IsGradeGame(p) || IdeaBoardQueries.Is(p, IdeaPieceType.AiSlop);
    static int Points(BoardPiece p) => p.Definition is IIdeaBlock b ? IdeaPieceInfo.GradePoints(b.IdeaType) : 0;

    static void Grow(GameSession session, BoardPiece game, int threshold, bool instant = false)
    {
        if (!(game.Definition is IIdeaBlock idea) || !IsGradeGame(game)) return;
        if (instant) game.PlayCount += threshold;
        while (game.PlayCount >= threshold && idea.IdeaType != IdeaPieceType.Goty)
        {
            var next = Find(session, (IdeaPieceType)((int)idea.IdeaType + 1));
            if (next == null) return;
            game.PlayCount -= threshold;
            game.Definition = next;
            idea = (IIdeaBlock)next;
        }
    }

    public static void Resolve(GameSession session, List<BoardPiece> dead, IdeaTuning tuning)
    {
        var pieces = session.Pieces;
        int threshold = Mathf.Max(1, tuning != null ? tuning.playsPerGrowth : 5);
        var games = pieces.Where(p => IsGradeOrSlop(p) && !dead.Contains(p)).ToList();
        foreach (var human in pieces.Where(p => p.Definition is HumanBlock && p.Active && !dead.Contains(p)).ToList())
        {
            foreach (var game in session.GamesFor(human).Where(g => IsGradeOrSlop(g) && !dead.Contains(g)))
            {
                session.AddScore(Points(game));
                game.PlayCount++;
            }
        }

        foreach (var critic in pieces.Where(p => IdeaBoardQueries.Is(p, IdeaPieceType.Critic) && !dead.Contains(p)))
        {
            var targets = games.Where(g => IdeaBoardQueries.Touches(critic, g)).ToList();
            critic.Active = targets.Count > 0;
            foreach (var g in targets)
            {
                g.PlayCount += 5;
                session.AddScore(Points(g) / 2);
            }
        }

        foreach (var influencer in pieces.Where(p =>
                     IdeaBoardQueries.Is(p, IdeaPieceType.Influencer) && !dead.Contains(p)))
        {
            var targets = games.Where(g => IdeaBoardQueries.Nearby(influencer, g, influencer.Definition.Range))
                .ToList();
            influencer.Active = targets.Count > 0;
            foreach (var g in targets) g.PlayCount += 10;
        }

        foreach (var ceremony in pieces.Where(p => IdeaBoardQueries.Is(p, IdeaPieceType.Ceremony) && !dead.Contains(p)))
        foreach (var g in games.Where(g => IsGradeGame(g) && IdeaBoardQueries.Touches(ceremony, g)))
            if (Random.value < (tuning != null ? tuning.ceremonyGrowthChance : .1f))
                Grow(session, g, threshold, true);
        foreach (var g in games.Where(IsGradeGame)) Grow(session, g, threshold);
        var taxi = Find(session, IdeaPieceType.Taxi);
        if (taxi != null)
        {
            foreach (var fan in pieces.Where(p => p.Definition is GoodFanBlock && !dead.Contains(p)).ToList())
            {
                if (!pieces.Any(p =>
                        IdeaBoardQueries.Is(p, IdeaPieceType.Goty) && IdeaBoardQueries.Touches(fan, p))) continue;
                fan.RestoreAfterTurn = fan.Definition;
                fan.Definition = taxi;
            }
        }

        foreach (var refund in pieces.Where(p => IdeaBoardQueries.Is(p, IdeaPieceType.Refund) && !dead.Contains(p)))
        {
            foreach (var game in pieces.Where(g =>
                             g.Definition is GameBlock && !dead.Contains(g) && IdeaBoardQueries.Touches(refund, g))
                         .ToList())
            {
                refund.AccumulatedScore += Mathf.Max(0, Points(game));
                dead.Add(game);
            }

            session.AddScore(refund.AccumulatedScore);
        }

        foreach (var dev in pieces.Where(p => IdeaBoardQueries.Is(p, IdeaPieceType.Developer) && !dead.Contains(p))
                     .ToList())
        {
            if (Random.value >= (tuning != null ? tuning.developerSpawnChance : .3f)) continue;
            bool chat = pieces.Any(p =>
                IdeaBoardQueries.Is(p, IdeaPieceType.ChatGpt) && IdeaBoardQueries.Touches(dev, p) && !dead.Contains(p));
            var type = chat
                ? (Random.value < .5f ? IdeaPieceType.GradeA : IdeaPieceType.AiSlop)
                : (IdeaPieceType)Random.Range((int)IdeaPieceType.GradeC, (int)IdeaPieceType.GradeA + 1);
            var prefab = Find(session, type);
            if (prefab == null) continue;
            var free = new List<Vector2Int>();
            for (int y = 0; y < 10; y++)
            for (int x = 0; x < 10; x++)
            {
                var c = new Vector2Int(x, y);
                if (session.Free(new[] { c })) free.Add(c);
            }

            if (free.Count > 0) session.Place(prefab, new[] { free[Random.Range(0, free.Count)] });
        }
    }
}