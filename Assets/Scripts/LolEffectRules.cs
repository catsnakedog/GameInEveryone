using System.Collections.Generic;
using System.Linq;

// Pure calculations for the explicitly specified LOL route rules.
public static class LolEffectRules
{
    public static bool IsRung(BoardPiece piece) => IdeaBoardQueries.Is(piece, IdeaPieceType.Rung);

    public static int CountNearbyRung(BoardPiece carry, IEnumerable<BoardPiece> pieces)
        => pieces.Count(p => IsRung(p) && IdeaBoardQueries.Nearby(carry, p, carry.Definition.Range));

    public static bool HasNearbyLol(BoardPiece piece, IEnumerable<BoardPiece> pieces)
        => pieces.Any(p =>
            IdeaBoardQueries.Is(p, IdeaPieceType.Lol) && IdeaBoardQueries.Nearby(piece, p, piece.Definition.Range));

    public static bool HasHack(BoardPiece human, IEnumerable<BoardPiece> pieces)
        => IsRung(human) &&
           pieces.Any(p => IdeaBoardQueries.Is(p, IdeaPieceType.Hack) && IdeaBoardQueries.Touches(human, p));

    public static int CarryCapacity(int connectedYumiCount) => 1 + System.Math.Max(0, connectedYumiCount);

    public static int CarryScore(int nearbyRungCount, bool hasLol, int capacity)
        => !hasLol || nearbyRungCount > capacity ? 0 : 30 + nearbyRungCount * nearbyRungCount * 100;

    public static int RungScore(bool hasLol) => hasLol ? 20 : 0;
}