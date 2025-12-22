namespace H.Engine;

using S = TaperedScore;
using H.Core;

public static class EvaluationConstants
{
    
    public static readonly S[] MaterialValues = {
        new S(100, 100), new S(300, 300), new S(320, 320), new S(500, 500), new S(900, 900)
    };

    public static readonly S CloserToEnemyKing = new S(0, 50);
    public static readonly S EnemyKingCorner = new S(0, 50);
    public static readonly S OutpostBonus = new S(25, 20);
    public static readonly S OpenFileBonus = new S(20, 5);
    public static readonly S SemiFileBonus = new S(15, 5);
    // [Rank]
    public static readonly S[] PassedPawnBonus = {
        new S(0, 0), new S(5, 5), new S(10, 10), new S(20, 20), new S(30, 30), new S(40, 40), new S(50, 50), new S(0, 0)
    };
    public static readonly S IsolatedPawnPenaltyPerPawn = new S(5, 5);   
    public static readonly S KingOpenFilePenalty = new S(50, 0);
    public static readonly S KingSemiOpenFilePenalty = new S(35, 0);
    public static readonly S KingProtector = new S(20, 0);

    // Evaluation precomputed data
    public static readonly int[,] DistanceFromSquare;
    public static readonly int[] DistanceFromCenter;

    // Bitboards
    // [Color] [Rank] Does not contain the rank itself
    public static readonly Bitboard[][] PawnForwardMask;
    public static readonly Bitboard[][] PassedPawnMask;
    public static readonly Bitboard[][] ForwardPawnAttackers;

    static EvaluationConstants()
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
    }
}