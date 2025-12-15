namespace H.Core;

/*
    Credit to Sebastian Lague: https://github.com/SebLague/Chess-Coding-Adventure
*/

using static PrecomputedMagics;

public static class Magic
{
    // [Square]
    public static readonly Bitboard[] RookMask;
    public static readonly Bitboard[] BishopMask;

    // [Square][Key]
    static readonly Bitboard[][] RookAttacks;
    static readonly Bitboard[][] BishopAttacks;

    public static Bitboard GetSliderAttacks(Square square, Bitboard blockers, bool ortho)
    {
        return ortho ? GetRookAttacks(square, blockers) : GetBishopAttacks(square, blockers);
    }

    public static Bitboard GetRookAttacks(Square square, Bitboard blockers)
    {
        ulong key = ((blockers & RookMask[square]) * RookMagics[square]) >> RookShifts[square];
        return RookAttacks[square][key];
    }

    public static Bitboard GetBishopAttacks(Square square, Bitboard blockers)
    {
        ulong key = ((blockers & BishopMask[square]) * BishopMagics[square]) >> BishopShifts[square];
        return BishopAttacks[square][key];
    }

    static Magic()
    {
        RookMask = new Bitboard[64];
        BishopMask = new Bitboard[64];

        for (Square square = 0; square < 64; square ++)
        {
            RookMask[square] = MagicHelper.CreateMovementMask(square, ortho: true);
            BishopMask[square] = MagicHelper.CreateMovementMask(square, ortho: false);
        }

        RookAttacks = new Bitboard[64][];
        BishopAttacks = new Bitboard[64][];

        for (Square square = 0; square < 64; square++)
        {
            RookAttacks[square] = CreateTable(square, true, RookMagics[square], RookShifts[square]);
            BishopAttacks[square] = CreateTable(square, false, BishopMagics[square], BishopShifts[square]);
        }

        Bitboard[] CreateTable(Square square, bool rook, ulong magic, int shift)
        {
            int numBits = 64 - shift;
            int lookupSize = 1 << numBits; // 2^n
            Bitboard[] table = new Bitboard[lookupSize];

            // Consider all path possible on an empty board
            Bitboard movementMask = MagicHelper.CreateMovementMask(square, ortho: rook);
            Bitboard[] blockerPatterns = MagicHelper.CreateAllBlockers(movementMask);

            foreach (Bitboard pattern in blockerPatterns)
            {
                // For each possible blocker pattern
                ulong idx = (pattern * magic) >> shift;
                ulong moves = MagicHelper.LegalMoveBitboardFromBlockers(square, pattern, rook);
                table[idx] = moves;
            }

            return table;
        }
    }
}