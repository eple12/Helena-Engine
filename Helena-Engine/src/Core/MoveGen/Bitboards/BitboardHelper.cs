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

    public static int GetLSB(ref readonly this Bitboard b)
    {
        return BitOperations.TrailingZeroCount(b);
    }
    public static int GetMSB(ref readonly this Bitboard b)
    {
        return 63 - BitOperations.LeadingZeroCount(b);
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
    public static bool MoreThanOne(this Bitboard bb)
    {
        return (bb & (bb - 1)) != 0;
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

    /// <summary>
    /// Find the first blocker square going in the direction.
    /// </summary>
    /// <param name="ray">The ray bitboard must be a direction ray(bits in a straight line) since we use LSB/MSB. </param>
    /// <param name="dirIndex">Direction index must be 0-7.</param>
    /// <returns>Square of the first blocker. If none, returns INVALID_SQUARE</returns>
    public static Square FirstBlocker(Bitboard ray, int dirIndex)
    {
        if (ray == 0)
        {
            // No need for pop counts
            return SquareHelper.INVALID_SQUARE;
        }

        // LSB: 0, 1, 4, 5
        // MSB: 2, 3, 6, 7
        if ((dirIndex % 4) <= 1)
        {
            // LSB
            return (Square) ray.GetLSB();
        }
        else
        {
            // MSB
            return (Square) ray.GetMSB();
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