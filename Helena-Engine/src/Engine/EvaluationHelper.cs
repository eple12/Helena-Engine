namespace H.Engine;

using H.Core;

public static class EvaluationHelper
{
    // Evaluation precomputed data
    public static readonly int[,] DistanceFromSquare;
    public static readonly int[] DistanceFromCenter;

    // Bitboards
    // [Color] [Rank] Does not contain the rank itself
    public static readonly Bitboard[][] PawnForwardMask;
    public static readonly Bitboard[][] PassedPawnMask;
    public static readonly Bitboard[][] ForwardPawnAttackers;
    public static readonly Bitboard[] KingArea;

    static EvaluationHelper()
    {
        DistanceFromSquare = new int[64, 64];
        DistanceFromCenter = new int[64];

        for (Square sq1 = 0; sq1 < 64; sq1++)
        {
            for (Square sq2 = 0; sq2 < 64; sq2++)
            {
                DistanceFromSquare[sq1, sq2] = Math.Abs(SquareHelper.GetRank(sq1) - SquareHelper.GetRank(sq2)) + Math.Abs(SquareHelper.GetFile(sq1) - SquareHelper.GetFile(sq2));
            }
        }
        for (Square sq = 0; sq < 64; sq++)
        {
            Coord c = new (sq);

            if (c.X <= 3)
            {
                if (c.Y <= 3)
                {
                    DistanceFromCenter[sq] = DistanceFromSquare[sq, SquareHelper.D4];
                    continue;
                }

                DistanceFromCenter[sq] = DistanceFromSquare[sq, SquareHelper.D5];
                continue;
            }

            if (c.Y <= 3)
            {
                DistanceFromCenter[sq] = DistanceFromSquare[sq, SquareHelper.E4];
                continue;
            }

            DistanceFromCenter[sq] = DistanceFromSquare[sq, SquareHelper.E5];
            continue;
        }

        PawnForwardMask = new Bitboard[2][];
        PawnForwardMask[0] = new Bitboard[8];
        PawnForwardMask[1] = new Bitboard[8];
        for (int rank = 0; rank < 8; rank++)
        {
             PawnForwardMask[0][rank] = BitboardHelper.Shift(ulong.MaxValue, 8 * (rank + 1));
             PawnForwardMask[1][rank] = BitboardHelper.Shift(ulong.MaxValue, -8 * (8 - rank));
        }

        PassedPawnMask = new Bitboard[2][];
        for (int color = 0; color < 2; color++)
        {
            PassedPawnMask[color] = new Bitboard[64];

            for (Square sq = 0; sq < 64; sq++)
            {
                int rank = SquareHelper.GetRank(sq);
                int file = SquareHelper.GetFile(sq);

                PassedPawnMask[color][sq] = PawnForwardMask[color][rank] & Bits.TripleFiles[file];
            }
        }

        ForwardPawnAttackers = new Bitboard[2][];
        for (int color = 0; color < 2; color++)
        {
            ForwardPawnAttackers[color] = new Bitboard[64];

            for (Square sq = 0; sq < 64; sq++)
            {
                int file = SquareHelper.GetFile(sq);

                ForwardPawnAttackers[color][sq] = PassedPawnMask[color][sq] & Bits.AdjacentFiles[file];
            }
        }

        KingArea = new Bitboard[64];
        for (Square sq = 0; sq < 64; sq++)
        {
            KingArea[sq] = Bits.KingMovement[sq] | (1UL << (int)sq);
        }
    }
}