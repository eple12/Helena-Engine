namespace H.Core;

using System.Numerics;
using H.Program;

public static class BitboardHelper
{
    public static int PopLSB(ref this Bitboard b)
    {
        int i = BitOperations.TrailingZeroCount(b);
        b &= b - 1;
        return i;
    }
    public static int PopMSB(ref this Bitboard b)
    {
        int i = BitOperations.LeadingZeroCount(b);
        b ^= 1ul << (63 - i);
        return 63 - i;
    }

    public static void SetSquare(ref this Bitboard b, Square square)
    {
        b |= 1ul << square;
    }
    public static void ClearSquare(ref this Bitboard b, Square square)
    {
        b &= ~(1ul << square);
    }
    public static void ToggleSquare(ref this Bitboard b, Square square)
    {
        b ^= 1ul << square;
    }
    public static void ToggleSquare(ref this Bitboard b, Square s1, Square s2)
    {
        b ^= 1ul << s1 | 1ul << s2;
    }

    public static Bitboard PawnAttacks(Bitboard pawns, Color color)
    {
        if (color == PieceHelper.WHITE)
        {
            return ((pawns << 9) & Bits.NotFileA) | ((pawns << 7) & Bits.NotFileH);
        }

        return ((pawns >> 9) & Bits.NotFileH) | ((pawns >> 7) & Bits.NotFileA);
    }

    public static int Count(this Bitboard bb)
    {
        return BitOperations.PopCount(bb);
    }

    public static bool Contains(ref readonly this Bitboard b, Square square)
    {
        return (b & (1ul << square)) != 0;
    }
    // NOTICE: "this" bitboard DOES NOT change, it only returns the shifted copy
    public static Bitboard Shift(Bitboard b, int numSquares)
    {
        if (numSquares > 0)
        {
            return b << numSquares;
        }
        else
        {
            return b >> -numSquares;
        }
    }

    // Debugging
    public static string Display(ref readonly this Bitboard b)
    {
        string s = "";

        for (int rank = 7; rank >= 0; rank--)
        {
            for (int file = 0; file < 8; file++)
            {
                Square square = SquareHelper.GetSquare(file, rank);
                s += b.Contains(square) ? "O " : ". ";
            }

            if (rank != 0)
            {
                s += "\n";
            }
        }

        return s;
    }
    public static void Print(ref readonly this Bitboard b)
    {
        Logger.LogLine(b.Display());
    }
}