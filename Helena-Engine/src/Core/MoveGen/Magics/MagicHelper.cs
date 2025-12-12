namespace H.Core;

/*
    Credit to Sebastian Lague: https://github.com/SebLague/Chess-Coding-Adventure
*/

/* 
    .  .  .  .  .  .  .  .
    .  .  .  .  .  .  .  .
    .  .  .  .  .  .  .  .
    .  .  .  .  .  .  .  .
    .  .  .  .  .  .  .  .
    .  .  .  .  .  .  .  .
    .  .  .  .  .  .  .  .
    .  .  .  .  .  .  .  .
*/
// Copy this for cool visual bitboard comments

public static class MagicHelper
{
// region Comment
    /* BLOCKERS
        .  .  .  .  .  .  .  .
        .  .  .  .  .  .  .  .
        .  .  B  .  .  .  .  .
        .  .  .  .  .  .  .  .
        .  .  .  .  .  .  .  .
        .  .  .  .  .  .  .  .
        .  .  .  B  .  .  .  .
        .  B  .  .  .  .  .  .
    */
    /* 
        .  .  .  .  .  .  .  .
        .  .  .  .  .  .  .  .
        .  .  x  .  .  .  .  .
        .  .  .  .  .  .  .  .
        .  .  .  .  .  .  .  .
        .  .  .  .  .  .  .  .
        .  .  .  x  .  .  .  .
        .  x  .  .  .  .  .  .
    */
    /* 
        .  .  .  .  .  .  .  .
        .  .  .  .  .  .  .  .
        .  .  x  .  .  .  .  .
        .  .  .  .  .  .  .  .
        .  .  .  .  .  .  .  .
        .  .  .  .  .  .  .  .
        .  .  .  x  .  .  .  .
        .  o  .  .  .  .  .  .
    */
    /* 
        .  .  .  .  .  .  .  .
        .  .  .  .  .  .  .  .
        .  .  x  .  .  .  .  .
        .  .  .  .  .  .  .  .
        .  .  .  .  .  .  .  .
        .  .  .  .  .  .  .  .
        .  .  .  o  .  .  .  .
        .  x  .  .  .  .  .  .
    */
    // ...
    /* 
        .  .  .  .  .  .  .  .
        .  .  .  .  .  .  .  .
        .  .  o  .  .  .  .  .
        .  .  .  .  .  .  .  .
        .  .  .  .  .  .  .  .
        .  .  .  .  .  .  .  .
        .  .  .  o  .  .  .  .
        .  o  .  .  .  .  .  .
    */
// endregion
    // Given a movement mask(with legal squares of a sliding piece on a specific square), calculate all possible blocker placement
    public static Bitboard[] CreateAllBlockers(Bitboard movementMask)
    {
        List<Square> moveSquareIndices = new();
        Bitboard movementCopy = movementMask;
        while (movementCopy != 0)
        {
            int idx = movementCopy.PopLSB();
            moveSquareIndices.Add((Square) idx);
        }

        int numPatterns = 1 << moveSquareIndices.Count; // 2^n. Number of possible patterns of blockers
        Bitboard[] blockerBitboards = new Bitboard[numPatterns];

        // All possible patterns
        for (int patternIdx = 0; patternIdx < numPatterns; patternIdx++)
        {
            // patternIdx -> 000...000 - 111...111 (0 - 2^n-1)
            for (int bitIdx = 0; bitIdx < moveSquareIndices.Count; bitIdx++)
            {
                int bit = (patternIdx >> bitIdx) & 1;
                blockerBitboards[patternIdx] |= ((ulong)bit) << moveSquareIndices[bitIdx];
            }
        }

        return blockerBitboards;
    }

    /* 
        .  .  .  .  .  .  .  .
        .  .  X  .  .  .  .  .
        .  .  X  .  .  .  .  .
        .  .  X  .  .  .  .  .
        .  .  X  .  .  .  .  .
        .  X  R  X  X  X  X  .
        .  .  X  .  .  .  .  .
        .  .  .  .  .  .  .  .
    */
    // Exclude the outer squares for.. what?
    // Even if there are blockers on the edge of the board, we don't need to calculate them
    // since the magic always assumes that legal moves contain "capturing" blockers.
    // No matter if we encounter a blocker on the edge, that square is legal.
    // And there are no more squares in that direction so we don't really need to consider "being blocked" by a blocker there.
    public static Bitboard CreateMovementMask(Square square, bool ortho)
    {
        Bitboard mask = 0;
        Coord[] directions = ortho ? Coord.RookDirections : Coord.BishopDirections;
        Coord start = new(square);

        foreach (Coord dir in directions)
        {
            for (int dst = 1; dst < 8; dst++)
            {
                Coord current = start + dir * dst;
                Coord next = start + dir * (dst + 1);

                if (next.IsValid)
                {
                    mask.SetSquare(current.GetSquare);
                }
                else
                { break; }
            }
        }

        return mask;
    }

    /* 
        .  .  .  .  .  .  .  .
        .  .  .  .  .  .  O  .
        .  .  .  .  .  .  .  .
        .  .  .  O  B  .  .  .
        .  .  .  .  .  O  .  .
        .  .  .  .  .  .  .  .
        .  .  .  .  .  .  .  .
        .  .  .  .  .  .  .  .
    */
    /* 
        .  x  .  .  .  .  .  .
        .  .  x  .  .  .  x  .
        .  .  .  x  .  x  .  .
        .  .  .  .  .  .  .  .
        .  .  .  .  .  x  .  .
        .  .  .  x  .  .  .  .
        .  .  x  .  .  .  .  .
        .  x  .  .  .  .  .  .
    */
    // Includes capturing the blocker
    public static Bitboard LegalMoveBitboardFromBlockers(Square square, Bitboard blockers, bool ortho)
    {
        Bitboard bb = 0;

        Coord[] directions = ortho ? Coord.RookDirections : Coord.BishopDirections;
        Coord start = new(square);

        foreach (Coord dir in directions)
        {
            for (int dst = 1; dst < 8; dst++)
            {
                Coord current = start + dir * dst;
                
                if (current.IsValid)
                {
                    bb.SetSquare(current.GetSquare);

                    if (blockers.Contains(current.GetSquare))
                    {
                        break;
                    }
                }
                else { break; }
            }
        }

        return bb;
    }
}