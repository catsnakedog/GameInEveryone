using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class IdeaTurnRules
{
    static bool IsLol(BoardPiece p)
    {
        if (!(p.Definition is IIdeaBlock b)) return false;
        return (int)b.IdeaType <= (int)IdeaPieceType.Mushroom;
    }

    static int Multiplier(BoardPiece piece, IEnumerable<BoardPiece> pieces)
    {
        int count = pieces.Count(p =>
            IdeaBoardQueries.Is(p, IdeaPieceType.Mushroom) && IdeaBoardQueries.Nearby(p, piece, p.Definition.Range));
        return 1 << Mathf.Min(count, 10);
    }

    static BlockDefinition Find(GameSession session, IdeaPieceType type)
    {
        var game = session.GetComponent<TetrisGame>();
        return game == null ? null : game.catalog.FirstOrDefault(p => p is IIdeaBlock b && b.IdeaType == type);
    }

    public static void ResolveLol(GameSession session, List<BoardPiece> dead)
    {
        var pieces = session.Pieces;
        var fakers = pieces.Where(p => IdeaBoardQueries.Is(p, IdeaPieceType.Faker) && !dead.Contains(p)).ToList();
        if (fakers.Count > 0)
        {
            var worshipPrefab = Find(session, IdeaPieceType.Worshipper);
            if (worshipPrefab != null)
                foreach (var p in pieces.Where(p =>
                                 p.Definition is HumanBlock && IsLol(p) && !IdeaBoardQueries.Is(p, IdeaPieceType.Faker))
                             .ToList())
                    p.Definition = worshipPrefab;
        }

        var rung = pieces.Where(LolEffectRules.IsRung).ToList();
        var carries = pieces
            .Where(p => IdeaBoardQueries.Is(p, IdeaPieceType.Carry) || LolEffectRules.HasHack(p, pieces)).ToList();
        var helpers = pieces.Where(p => IdeaBoardQueries.Is(p, IdeaPieceType.Yumi)).ToList();
        var tuning = Resources.Load<IdeaTuning>("IdeaTuning");
        int mattBonus = tuning != null ? tuning.mattContactBonus : 40;
        foreach (var unit in rung)
        {
            bool lol = session.GamesFor(unit).Any(g => IdeaBoardQueries.Is(g, IdeaPieceType.Lol));
            unit.Active = lol;
            int extra = pieces.Count(p =>
                IdeaBoardQueries.Is(p, IdeaPieceType.Matt) && IdeaBoardQueries.Touches(unit, p)) * mattBonus;
            session.AddScoreFor(unit, (LolEffectRules.RungScore(lol) + extra) * Multiplier(unit, pieces));
        }

        foreach (var carry in carries)
        {
            int count = LolEffectRules.CountNearbyRung(carry, pieces);
            int capacity = LolEffectRules.CarryCapacity(helpers.Count);
            bool lol = session.GamesFor(carry).Any(g => IdeaBoardQueries.Is(g, IdeaPieceType.Lol));
            if (count > capacity)
            {
                carry.Patience = 0;
                if (!dead.Contains(carry)) dead.Add(carry);
                continue;
            }

            carry.Active |= lol;
            session.AddScoreFor(carry, LolEffectRules.CarryScore(count, lol, capacity) * Multiplier(carry, pieces));
        }

        foreach (var helper in helpers)
        {
            int score;
            if (carries.Count > 0)
            {
                helper.Active = true;
                score = 20;
            }
            else if (rung.Any(p => IdeaBoardQueries.Touches(helper, p)))
            {
                score = -30;
                helper.Patience = 0;
                dead.Add(helper);
            }
            else score = -10;

            session.AddScoreFor(helper, score * Multiplier(helper, pieces));
        }

        int trophies = pieces.Count(t =>
            IdeaBoardQueries.Is(t, IdeaPieceType.Trophy) && fakers.Any(f => IdeaBoardQueries.Touches(f, t)));
        foreach (var worship in pieces.Where(p => IdeaBoardQueries.Is(p, IdeaPieceType.Worshipper)).ToList())
        {
            int factor = trophies > 0 ? trophies : 1;
            session.AddScoreFor(worship, 50 * factor * Multiplier(worship, pieces));
            worship.Active = true;
            if (fakers.Count == 0 && --worship.Patience <= 0) dead.Add(worship);
        }

        foreach (var mushroom in pieces.Where(p => IdeaBoardQueries.Is(p, IdeaPieceType.Mushroom)).ToList())
        {
            foreach (var target in pieces.Where(p =>
                         IsLol(p) && p != mushroom && !dead.Contains(p) &&
                         IdeaBoardQueries.Nearby(mushroom, p, mushroom.Definition.Range)).ToList())
                if (Random.value < (tuning != null ? tuning.mushroomDeathChance : .2f))
                {
                    target.Patience = 0;
                    dead.Add(target);
                }
        }
    }
}