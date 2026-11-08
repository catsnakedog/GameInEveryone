using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Shared spatial predicates for new effects. Proximity includes diagonals;
// contact requires adjacent cells and excludes the piece itself.
public static class IdeaBoardQueries
{
    public static bool Is(BoardPiece piece, IdeaPieceType type) =>
        piece.Definition is IIdeaBlock idea && idea.IdeaType == type;

    public static bool Nearby(BoardPiece a, BoardPiece b, int range)
    {
        return a != b && a.Cells.Any(x =>
            b.Cells.Any(y => Mathf.Max(Mathf.Abs(x.x - y.x), Mathf.Abs(x.y - y.y)) <= range));
    }

    public static bool Touches(BoardPiece a, BoardPiece b) => Nearby(a, b, 1);

    public static IEnumerable<BoardPiece> OfType(IEnumerable<BoardPiece> pieces, IdeaPieceType type) =>
        pieces.Where(p => Is(p, type));

    public static bool IsGameGrade(BoardPiece piece)
    {
        return piece.Definition is IIdeaBlock idea && (int)idea.IdeaType >= (int)IdeaPieceType.GradeC &&
               (int)idea.IdeaType <= (int)IdeaPieceType.Goty;
    }
}