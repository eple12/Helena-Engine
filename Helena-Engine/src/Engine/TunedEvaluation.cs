namespace H.Engine;

using S = TaperedScore;
using H.Core;

public static partial class TunedEvaluation
{
    public static readonly S[] MaterialValues = {new S(130, 114), new S(353, 331), new S(378, 357), new S(564, 564), new S(1015, 990)};
    public static readonly S[] PawnPsqt = {
        new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0),
        new S(50, 92), new S(79, 57), new S(77, 107), new S(39, 71), new S(66, 101), new S(15, 77), new S(75, 119), new S(12, 61),
        new S(40, 56), new S(27, 38), new S(50, 51), new S(41, 32), new S(37, 48), new S(35, 33), new S(25, 42), new S(27, 45),
        new S(17, 26), new S(30, 26), new S(8, 33), new S(53, 13), new S(21, 21), new S(32, 2), new S(0, 33), new S(28, 4),
        new S(12, 24), new S(18, 12), new S(8, 32), new S(33, 9), new S(25, 10), new S(13, 7), new S(6, 1), new S(10, 12),
        new S(12, 4), new S(17, 19), new S(12, 14), new S(5, 18), new S(8, 15), new S(-0, -8), new S(8, 8), new S(9, -8),
        new S(9, 9), new S(19, -0), new S(14, 14), new S(-10, 13), new S(-12, 5), new S(12, 1), new S(18, -3), new S(5, 6),
        new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0)
    };
    public static readonly S[] KnightPsqt = {
        new S(-54, -16), new S(-118, -57), new S(16, -1), new S(-55, -29), new S(2, -32), new S(-68, -39), new S(-39, -47), new S(-75, -92),
        new S(16, -20), new S(32, -39), new S(66, -5), new S(10, -36), new S(21, 7), new S(-22, -37), new S(7, -4), new S(-29, -58),
        new S(3, -22), new S(10, -15), new S(23, 13), new S(39, -5), new S(61, -4), new S(36, -10), new S(8, 2), new S(-21, -28),
        new S(-32, -12), new S(27, -4), new S(14, 19), new S(42, 1), new S(10, 29), new S(40, -3), new S(12, 9), new S(-11, -37),
        new S(-28, -11), new S(11, 1), new S(17, 15), new S(30, 14), new S(22, 8), new S(25, 15), new S(9, -7), new S(-19, -19),
        new S(-20, -28), new S(7, 6), new S(9, 6), new S(23, 2), new S(22, 20), new S(8, -12), new S(8, 7), new S(-31, -34),
        new S(-38, -25), new S(-13, -6), new S(6, -2), new S(2, -5), new S(5, -6), new S(3, -2), new S(-7, -23), new S(-38, -41),
        new S(-48, -20), new S(-44, -38), new S(-22, -15), new S(-19, -24), new S(-19, -11), new S(-15, -29), new S(-39, -26), new S(-37, -9)
    };
    public static readonly S[] BishopPsqt = {
        new S(-52, -2), new S(-31, -7), new S(35, -0), new S(-18, -5), new S(-23, 2), new S(-69, -17), new S(1, -5), new S(-27, -40),
        new S(34, -6), new S(37, -26), new S(57, 17), new S(-5, -34), new S(50, 7), new S(12, -5), new S(50, 7), new S(-19, -15),
        new S(8, -17), new S(8, -13), new S(24, 6), new S(31, 2), new S(11, 13), new S(33, 15), new S(-3, 16), new S(20, -22),
        new S(-9, -12), new S(22, -13), new S(4, 18), new S(42, -4), new S(24, 27), new S(25, 2), new S(6, 21), new S(3, -11),
        new S(-5, -22), new S(12, -11), new S(5, -7), new S(22, 10), new S(15, 15), new S(20, 22), new S(28, 1), new S(-6, -18),
        new S(-9, -13), new S(17, -15), new S(16, 12), new S(6, -6), new S(8, 16), new S(14, 12), new S(14, 15), new S(-1, -8),
        new S(-2, -29), new S(2, -12), new S(2, -17), new S(-3, -3), new S(7, 0), new S(3, 13), new S(7, 9), new S(-2, -11),
        new S(-10, -5), new S(0, 1), new S(-9, -0), new S(2, -3), new S(-3, 11), new S(-4, 1), new S(-11, 4), new S(-17, -5)
    };
    public static readonly S[] RookPsqt = {
        new S(40, 31), new S(7, 11), new S(31, 4), new S(38, -17), new S(34, 12), new S(4, -15), new S(26, 5), new S(-64, 4),
        new S(59, 42), new S(26, -20), new S(67, 27), new S(50, 9), new S(53, 23), new S(36, 6), new S(30, 18), new S(29, 6),
        new S(10, -6), new S(27, -6), new S(22, 8), new S(23, 7), new S(17, 10), new S(36, -5), new S(9, 3), new S(13, -10),
        new S(-10, 17), new S(22, -9), new S(3, 26), new S(18, 9), new S(15, 25), new S(3, 9), new S(-11, 5), new S(-2, 5),
        new S(-10, -13), new S(5, 10), new S(1, 4), new S(13, 21), new S(21, 13), new S(-1, 4), new S(-7, 9), new S(-1, -16),
        new S(-7, 3), new S(-4, -10), new S(-0, 11), new S(5, -3), new S(1, 11), new S(4, 1), new S(8, -9), new S(-13, -12),
        new S(-8, -25), new S(-3, -0), new S(11, 4), new S(6, 9), new S(3, 3), new S(4, 6), new S(7, -1), new S(-5, -5),
        new S(-20, -28), new S(-14, -17), new S(2, 14), new S(18, 16), new S(8, 13), new S(-7, -12), new S(-18, -23), new S(-22, -22)
    };
    public static readonly S[] QueenPsqt = {
        new S(-27, -13), new S(10, -54), new S(16, -11), new S(8, -54), new S(8, -20), new S(-38, -54), new S(14, -9), new S(-66, -69),
        new S(43, 7), new S(84, -24), new S(60, -3), new S(49, -28), new S(29, -9), new S(28, -1), new S(41, -9), new S(22, -30),
        new S(4, 32), new S(35, 33), new S(27, 35), new S(53, 7), new S(7, 6), new S(32, -4), new S(-9, 24), new S(0, -12),
        new S(13, 43), new S(12, 30), new S(20, 66), new S(16, 41), new S(21, 12), new S(22, -1), new S(2, -19), new S(2, -14),
        new S(-1, 10), new S(14, 32), new S(16, 38), new S(5, 42), new S(6, 33), new S(8, 0), new S(6, -6), new S(-4, -30),
        new S(-9, 4), new S(4, -0), new S(3, 27), new S(6, 17), new S(8, 15), new S(-3, 4), new S(5, -21), new S(-9, -22),
        new S(1, -14), new S(4, 8), new S(5, 7), new S(1, 10), new S(2, 8), new S(4, 11), new S(12, 1), new S(9, -9),
        new S(-10, -10), new S(-3, -1), new S(-17, 6), new S(-12, -10), new S(-3, 5), new S(8, -9), new S(9, -19), new S(-18, 11)
    };
    public static readonly S[] KingPsqt = {
        new S(-57, 24), new S(-74, 19), new S(-45, 18), new S(-95, -29), new S(-46, 17), new S(-92, -36), new S(-54, 14), new S(-69, -0),
        new S(-44, 26), new S(-59, 7), new S(-51, 10), new S(-49, -12), new S(-52, -9), new S(-54, 12), new S(-57, 21), new S(-49, 14),
        new S(-46, 14), new S(-41, 3), new S(-68, 24), new S(-48, -6), new S(-74, 29), new S(-39, 8), new S(-66, 28), new S(-38, -2),
        new S(-45, 7), new S(-52, -10), new S(-78, 30), new S(-79, 22), new S(-57, 13), new S(-62, 35), new S(-71, -3), new S(-28, -2),
        new S(1, -20), new S(-35, -14), new S(-46, 24), new S(-67, 13), new S(-41, 26), new S(-36, 8), new S(-20, -15), new S(-17, -32),
        new S(-12, -5), new S(10, -23), new S(1, 3), new S(-25, 2), new S(-11, 1), new S(-1, 6), new S(6, -28), new S(23, -31),
        new S(22, -23), new S(29, -17), new S(14, -1), new S(15, -14), new S(14, -8), new S(22, -8), new S(37, -26), new S(39, -36),
        new S(34, -33), new S(35, -35), new S(25, -37), new S(24, -29), new S(17, -46), new S(23, -18), new S(28, -40), new S(13, 2)
    };

    // Piece Features
    public static readonly S BishopPairBonus = new S(20, 15);
    public static readonly S[] KnightMobilityBonus = {new S(13, -5), new S(33, 29), new S(43, 38), new S(48, 43), new S(49, 43), new S(49, 47), new S(49, 44), new S(47, 46), new S(50, 35)};
    public static readonly S[] BishopMobilityBonus = {new S(10, 5), new S(32, 33), new S(42, 43), new S(48, 44), new S(53, 52), new S(53, 54), new S(54, 53), new S(54, 51), new S(52, 53), new S(52, 48), new S(50, 46), new S(50, 35), new S(36, 45), new S(36, 17)};
    public static readonly S[] RookMobilityBonus = {new S(7, -13), new S(28, 28), new S(38, 40), new S(44, 47), new S(49, 54), new S(48, 59), new S(47, 64), new S(47, 64), new S(47, 63), new S(47, 63), new S(48, 59), new S(45, 59), new S(45, 55), new S(42, 47), new S(53, 26)};
    public static readonly S[] QueenMobilityBonus = {new S(11, -1), new S(32, 32), new S(39, 45), new S(45, 59), new S(50, 61), new S(50, 69), new S(51, 67), new S(50, 73), new S(50, 76), new S(51, 75), new S(50, 78), new S(51, 81), new S(52, 79), new S(52, 81), new S(48, 79), new S(55, 74), new S(57, 66), new S(65, 52), new S(65, 48), new S(74, 33), new S(77, 21), new S(67, 18), new S(79, 1), new S(55, 2), new S(31, 6), new S(29, 2), new S(25, 7), new S(32, 19)};
    public static readonly S OutpostBonus = new S(22, 31);
    public static readonly S OpenFileBonus = new S(43, 2);
    public static readonly S SemiFileBonus = new S(14, 23);

    // Pawn Features
    public static readonly S[] PassedPawnBonus = {new S(0, 0), new S(1, 11), new S(9, 19), new S(20, 64), new S(28, 106), new S(70, 198), new S(88, 311), new S(0, 0)};
    public static readonly S PassedPawnProtectedBonus = new S(51, 74);
    public static readonly S[] PassedPawnBlockedPenalty = {new S(5, 18), new S(0, 22), new S(-3, 20), new S(-3, 5), new S(-2, 17)};
    public static readonly S DoubledPawnPenalty = new S(8, 9);
    public static readonly S[] IsolatedPawnPenaltyByCount = {new S(-3, -2), new S(17, 29), new S(26, 42), new S(37, 54), new S(51, 65), new S(77, 67), new S(72, 74), new S(81, 87), new S(90, 100)};

    // King Safety
    public static readonly S PawnShelterMissingPenalty = new S(16, -3);
    public static readonly S PawnShelterWeakPenalty = new S(10, -2);
    public static readonly S KingFileOpenPenalty = new S(21, 1);
    public static readonly S KingFileSemiOpenPenalty = new S(11, -5);
    public static readonly S[] PawnStormPenaltyByDistance = {new S(49, -40), new S(36, -6), new S(23, -2), new S(15, -2)};


    // Mopup
    public static readonly S CloserToEnemyKing = new S(0, 50);
    public static readonly S EnemyKingCorner = new S(0, 50);

    public static readonly S[][] Tables = [
        [],
        PawnPsqt,
        KnightPsqt,
        BishopPsqt,
        RookPsqt,
        QueenPsqt,
        KingPsqt
    ];

    public static int ReadTableFromPiece(Piece pieceType, Square square, bool white)
    {
        return Tables[pieceType][white ? SquareHelper.FlipRank(square) : square].Mid;
    }
}