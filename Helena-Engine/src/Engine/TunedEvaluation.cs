namespace H.Engine;

using S = TaperedScore;
using H.Core;

public static partial class TunedEvaluation
{
    public static readonly S[] MaterialValues = {new S(114, 161), new S(516, 593), new S(487, 556), new S(576, 962), new S(1542, 1906)};
    public static readonly S[] PawnPsqt = {
        new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0),
        new S(49, 88), new S(72, 59), new S(77, 101), new S(41, 66), new S(75, 87), new S(24, 66), new S(76, 109), new S(21, 49),
        new S(33, 56), new S(22, 37), new S(42, 50), new S(34, 32), new S(31, 45), new S(30, 31), new S(20, 38), new S(22, 42),
        new S(11, 28), new S(22, 29), new S(4, 35), new S(47, 17), new S(16, 25), new S(30, 4), new S(-3, 34), new S(26, 6),
        new S(6, 27), new S(13, 14), new S(6, 34), new S(29, 12), new S(23, 11), new S(11, 8), new S(3, 1), new S(9, 13),
        new S(7, 6), new S(11, 21), new S(10, 16), new S(3, 18), new S(6, 14), new S(-1, -7), new S(6, 9), new S(7, -6),
        new S(6, 14), new S(16, 5), new S(15, 19), new S(-10, 15), new S(-11, 8), new S(13, 2), new S(18, -1), new S(6, 8),
        new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0)
    };
    public static readonly S[] KnightPsqt = {
        new S(-53, -16), new S(-111, -57), new S(22, -2), new S(-48, -27), new S(3, -21), new S(-44, -38), new S(-39, -40), new S(-61, -83),
        new S(-8, -18), new S(-4, -34), new S(43, -1), new S(-3, -31), new S(21, 14), new S(-24, -38), new S(3, -8), new S(-34, -56),
        new S(-10, -24), new S(4, -15), new S(16, 11), new S(32, -5), new S(43, 2), new S(32, -6), new S(8, 0), new S(-15, -29),
        new S(-35, -17), new S(29, -4), new S(10, 17), new S(42, -1), new S(13, 27), new S(41, -6), new S(15, 10), new S(-13, -36),
        new S(-26, -11), new S(10, -2), new S(19, 12), new S(31, 12), new S(24, 5), new S(27, 12), new S(9, -7), new S(-15, -18),
        new S(-14, -27), new S(9, 8), new S(15, 8), new S(25, 1), new S(25, 19), new S(14, -9), new S(11, 10), new S(-24, -33),
        new S(-34, -24), new S(-10, -8), new S(9, -1), new S(7, -2), new S(10, -3), new S(5, -0), new S(-5, -18), new S(-34, -32),
        new S(-45, -29), new S(-35, -35), new S(-20, -15), new S(-17, -22), new S(-17, -9), new S(-14, -24), new S(-29, -26), new S(-37, -20)
    };
    public static readonly S[] BishopPsqt = {
        new S(-33, 2), new S(-29, -2), new S(31, 7), new S(-8, -7), new S(-12, -2), new S(-22, -22), new S(6, 2), new S(-27, -34),
        new S(5, -3), new S(-6, -19), new S(38, 20), new S(-14, -36), new S(35, 6), new S(12, -10), new S(31, 6), new S(-14, -18),
        new S(-9, -12), new S(4, -17), new S(18, -1), new S(25, 1), new S(13, 11), new S(31, 10), new S(-3, 11), new S(14, -19),
        new S(-19, -10), new S(24, -16), new S(-2, 15), new S(37, -9), new S(22, 22), new S(22, 1), new S(11, 20), new S(-3, -12),
        new S(-8, -19), new S(11, -10), new S(10, -8), new S(22, 8), new S(16, 10), new S(25, 21), new S(25, 3), new S(-6, -13),
        new S(-6, -10), new S(19, -10), new S(20, 11), new S(12, -6), new S(14, 16), new S(17, 11), new S(17, 19), new S(2, -6),
        new S(0, -19), new S(7, -7), new S(5, -12), new S(2, -0), new S(13, 3), new S(6, 15), new S(12, 11), new S(-0, -4),
        new S(-9, -3), new S(4, -2), new S(-1, 2), new S(5, -1), new S(-1, 14), new S(2, 2), new S(-10, 8), new S(-16, -3)
    };
    public static readonly S[] RookPsqt = {
        new S(37, 27), new S(6, 7), new S(18, 7), new S(27, -15), new S(28, 16), new S(-3, -11), new S(24, 6), new S(-42, -9),
        new S(49, 41), new S(10, -17), new S(53, 29), new S(41, 9), new S(43, 25), new S(30, 6), new S(28, 17), new S(29, 2),
        new S(3, -8), new S(17, -6), new S(13, 4), new S(20, 5), new S(8, 7), new S(33, -8), new S(6, -0), new S(15, -18),
        new S(-14, 11), new S(22, -16), new S(5, 17), new S(16, 3), new S(13, 21), new S(7, 2), new S(-1, -1), new S(2, -4),
        new S(-8, -15), new S(8, 4), new S(2, 1), new S(13, 19), new S(21, 12), new S(4, 6), new S(-0, 8), new S(3, -18),
        new S(-1, 3), new S(-0, -8), new S(4, 13), new S(6, 3), new S(5, 16), new S(10, 7), new S(10, -0), new S(-6, -9),
        new S(-8, -19), new S(0, 6), new S(14, 10), new S(8, 17), new S(6, 10), new S(8, 14), new S(9, 9), new S(-3, -1),
        new S(-11, -17), new S(-9, -11), new S(9, 21), new S(25, 25), new S(14, 25), new S(3, 1), new S(-11, -13), new S(-12, -11)
    };
    public static readonly S[] QueenPsqt = {
        new S(-13, -12), new S(10, -38), new S(22, -1), new S(4, -37), new S(18, -12), new S(-36, -44), new S(18, -1), new S(-49, -64),
        new S(22, 18), new S(23, 19), new S(35, 12), new S(24, -12), new S(15, 2), new S(25, -6), new S(27, -2), new S(16, -19),
        new S(-2, 23), new S(23, 27), new S(11, 36), new S(39, 12), new S(4, 9), new S(34, -7), new S(1, 16), new S(14, -19),
        new S(7, 31), new S(16, 17), new S(14, 53), new S(15, 29), new S(18, 9), new S(25, -2), new S(9, -17), new S(11, -12),
        new S(9, -0), new S(20, 23), new S(26, 26), new S(13, 27), new S(13, 25), new S(19, -6), new S(16, -6), new S(4, -25),
        new S(-1, 3), new S(16, -2), new S(15, 23), new S(20, 13), new S(19, 9), new S(11, -0), new S(14, -18), new S(-1, -15),
        new S(9, -7), new S(14, 12), new S(17, 9), new S(14, 11), new S(15, 9), new S(16, 11), new S(20, 4), new S(13, -2),
        new S(-4, -2), new S(6, 3), new S(-6, 8), new S(4, -8), new S(9, 8), new S(12, 3), new S(13, -5), new S(-10, 17)
    };
    public static readonly S[] KingPsqt = {
        new S(-55, 6), new S(-72, -1), new S(-46, 6), new S(-93, -29), new S(-45, 12), new S(-91, -47), new S(-53, 1), new S(-68, -21),
        new S(-44, 9), new S(-58, -11), new S(-56, 2), new S(-51, -13), new S(-53, -9), new S(-51, -0), new S(-55, 5), new S(-46, 1),
        new S(-43, 3), new S(-31, -11), new S(-60, 18), new S(-43, -0), new S(-73, 33), new S(-29, -2), new S(-49, 8), new S(-34, -11),
        new S(-32, 0), new S(-36, -16), new S(-56, 27), new S(-57, 30), new S(-48, 20), new S(-38, 29), new S(-48, -13), new S(-23, -5),
        new S(-2, -18), new S(-30, -13), new S(-34, 29), new S(-45, 20), new S(-32, 34), new S(-33, 11), new S(-26, -11), new S(-21, -24),
        new S(-8, -10), new S(1, -19), new S(-5, 11), new S(-22, 12), new S(-14, 11), new S(-11, 14), new S(-5, -19), new S(1, -18),
        new S(16, -23), new S(22, -13), new S(7, 7), new S(9, -2), new S(8, 5), new S(12, 1), new S(24, -14), new S(25, -25),
        new S(16, -33), new S(24, -27), new S(20, -27), new S(11, -16), new S(15, -31), new S(7, -5), new S(17, -28), new S(-4, 8)
    };

    // Piece Features
    public static readonly S BishopPairBonus = new S(19, 16);
    public static readonly S[] KnightMobilityBonus = {new S(13, 1), new S(33, 31), new S(43, 41), new S(48, 47), new S(48, 47), new S(48, 49), new S(49, 47), new S(48, 48), new S(50, 41)};
    public static readonly S[] BishopMobilityBonus = {new S(9, 4), new S(30, 31), new S(40, 43), new S(46, 46), new S(51, 53), new S(51, 54), new S(52, 53), new S(51, 52), new S(50, 53), new S(50, 50), new S(50, 48), new S(49, 44), new S(41, 47), new S(33, 37)};
    public static readonly S[] RookMobilityBonus = {new S(11, -12), new S(30, 28), new S(40, 41), new S(45, 49), new S(50, 55), new S(50, 59), new S(49, 62), new S(49, 62), new S(49, 62), new S(49, 62), new S(50, 60), new S(49, 61), new S(48, 59), new S(44, 57), new S(46, 49)};
    public static readonly S[] QueenMobilityBonus = {new S(18, -0), new S(38, 25), new S(46, 36), new S(52, 50), new S(57, 56), new S(57, 63), new S(57, 62), new S(56, 67), new S(56, 71), new S(56, 71), new S(56, 72), new S(55, 76), new S(56, 76), new S(55, 78), new S(53, 77), new S(57, 74), new S(60, 68), new S(65, 58), new S(68, 53), new S(75, 42), new S(75, 35), new S(71, 30), new S(66, 28), new S(55, 25), new S(39, 26), new S(38, 24), new S(34, 27), new S(38, 32)};
    public static readonly S OutpostBonus = new S(23, 29);
    public static readonly S OpenFileBonus = new S(42, 4);
    public static readonly S SemiFileBonus = new S(15, 14);

    // Pawn Features
    public static readonly S[] PassedPawnBonus = {new S(0, 0), new S(13, 10), new S(17, 20), new S(27, 64), new S(36, 103), new S(81, 197), new S(103, 284), new S(0, 0)};
    public static readonly S PassedPawnProtectedBonus = new S(52, 72);
    public static readonly S[] PassedPawnBlockedPenalty = {new S(12, 15), new S(9, 15), new S(4, 11), new S(3, 1), new S(3, 7)};
    public static readonly S DoubledPawnPenalty = new S(9, 9);
    public static readonly S[] IsolatedPawnPenaltyByCount = {new S(-4, -2), new S(17, 29), new S(28, 40), new S(39, 51), new S(52, 62), new S(71, 68), new S(73, 82), new S(80, 88), new S(90, 100)};

    // King Safety
    public static readonly S PawnShelterMissingPenalty = new S(16, -1);
    public static readonly S PawnShelterWeakPenalty = new S(10, -1);
    public static readonly S KingFileOpenPenalty = new S(21, 1);
    public static readonly S KingFileSemiOpenPenalty = new S(11, -2);
    public static readonly S[] PawnStormPenaltyByDistance = {new S(34, -7), new S(31, 4), new S(22, 2), new S(15, -1)};


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