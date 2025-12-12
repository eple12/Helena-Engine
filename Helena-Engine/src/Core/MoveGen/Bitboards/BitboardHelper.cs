using System.Numerics;

namespace H.Core;


public static class BitboardHelper
{
    public const Bitboard FileA = 0x01010101_01010101;
    public const Bitboard Rank1 = 0b11111111;
    
    public static readonly Bitboard[] Files;
    public static readonly Bitboard[] Ranks;

    // bb.PopLSB
    public static int PopLSB(ref this Bitboard b)
    {
        int i = BitOperations.TrailingZeroCount(b);
        b &= b - 1;
        return i;
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

    public static bool Contains(ref this Bitboard b, Square square)
    {
        return (b & (1ul << square)) != 0;
    }
    // NOTICE: "this" bitboard DOES NOT change, it only returns the shifted copy
    public static Bitboard Shift(ref this Bitboard b, int numSquares)
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

    static BitboardHelper()
    {
        Files = new Bitboard[8];
        for (int file = 0; file < 8; file++)
        {
            Files[file] = FileA << file;
        }
        Ranks = new Bitboard[8];
        for (int rank = 0; rank < 8; rank++)
        {
            Ranks[rank] = Rank1 << (rank * 8);
        }


    }
}