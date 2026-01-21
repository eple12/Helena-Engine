namespace H.Engine;

using S = TaperedScore;
using H.Core;

public static partial class TunedEvaluation
{
    public static readonly S[] MaterialValues = {new S(80, 138), new S(305, 360), new S(318, 373), new S(440, 650), new S(961, 1238)};
    public static readonly S[] PawnPsqt = {
        new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0),
        new S(83, 87), new S(71, 90), new S(53, 88), new S(52, 69), new S(43, 73), new S(40, 87), new S(-21, 112), new S(-17, 92),
        new S(-9, 43), new S(2, 38), new S(13, 19), new S(9, -7), new S(17, -5), new S(51, 23), new S(11, 34), new S(-25, 32),
        new S(-9, 12), new S(-5, 2), new S(-5, -9), new S(7, -31), new S(18, -26), new S(18, -18), new S(0, -4), new S(-15, -7),
        new S(-14, -6), new S(-14, -9), new S(-6, -24), new S(-5, -27), new S(4, -27), new S(-1, -20), new S(-4, -20), new S(-18, -22),
        new S(-15, -13), new S(-18, -17), new S(-15, -17), new S(-15, -17), new S(-4, -11), new S(-10, -8), new S(-2, -19), new S(-17, -23),
        new S(-16, -8), new S(-20, -13), new S(-22, -9), new S(-22, -8), new S(-19, 1), new S(-0, 1), new S(9, -16), new S(-19, -28),
        new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0)
    };
    public static readonly S[] KnightPsqt = {
        new S(-178, -27), new S(-105, 9), new S(-85, 25), new S(-0, 5), new S(47, 2), new S(-60, 26), new S(-9, -3), new S(-123, -50),
        new S(-16, -13), new S(-19, 4), new S(25, 3), new S(54, 19), new S(46, 16), new S(86, -19), new S(-12, 1), new S(36, -20),
        new S(-30, -1), new S(-4, 13), new S(24, 37), new S(39, 36), new S(75, 25), new S(91, 27), new S(43, 9), new S(29, -6),
        new S(-8, 8), new S(0, 14), new S(17, 37), new S(28, 51), new S(19, 54), new S(45, 42), new S(22, 33), new S(33, 14),
        new S(-17, -0), new S(-5, 16), new S(7, 38), new S(9, 41), new S(19, 46), new S(14, 43), new S(35, 22), new S(4, 15),
        new S(-25, -29), new S(-9, 0), new S(-5, 11), new S(8, 31), new S(14, 27), new S(6, 10), new S(5, 5), new S(-9, -10),
        new S(-40, -17), new S(-29, 5), new S(-14, -5), new S(1, 7), new S(-2, 12), new S(-1, -8), new S(-12, 5), new S(-8, -1),
        new S(-81, -33), new S(-19, -35), new S(-42, -9), new S(-20, 6), new S(-12, 4), new S(-9, -5), new S(-13, -23), new S(-64, -25)
    };
    public static readonly S[] BishopPsqt = {
        new S(-52, 41), new S(-64, 37), new S(-89, 37), new S(-78, 36), new S(-54, 27), new S(-91, 32), new S(-1, 22), new S(-44, 35),
        new S(-45, 21), new S(-13, 22), new S(-14, 23), new S(-24, 27), new S(-8, 21), new S(10, 19), new S(-16, 22), new S(-3, 12),
        new S(-10, 18), new S(-6, 22), new S(15, 20), new S(15, 15), new S(34, 18), new S(52, 24), new S(43, 18), new S(16, 21),
        new S(-18, 16), new S(7, 17), new S(4, 19), new S(35, 25), new S(19, 29), new S(29, 19), new S(9, 26), new S(-2, 25),
        new S(-10, 4), new S(-1, 12), new S(8, 25), new S(18, 25), new S(23, 25), new S(-2, 23), new S(5, 16), new S(4, 5),
        new S(-4, 5), new S(10, 14), new S(5, 19), new S(10, 21), new S(6, 25), new S(10, 17), new S(7, 11), new S(12, 10),
        new S(9, 4), new S(8, -1), new S(14, 4), new S(-2, 13), new S(5, 12), new S(8, 3), new S(23, 4), new S(11, -18),
        new S(7, -4), new S(13, 5), new S(-8, 1), new S(-18, 9), new S(-17, 11), new S(-6, 11), new S(-3, 4), new S(5, -6)
    };
    public static readonly S[] RookPsqt = {
        new S(13, 48), new S(17, 51), new S(2, 58), new S(16, 52), new S(29, 48), new S(71, 40), new S(92, 31), new S(90, 33),
        new S(-13, 54), new S(-18, 58), new S(11, 53), new S(32, 50), new S(30, 50), new S(80, 22), new S(49, 31), new S(63, 26),
        new S(-26, 48), new S(9, 37), new S(3, 45), new S(25, 34), new S(51, 24), new S(83, 22), new S(104, 6), new S(41, 27),
        new S(-31, 41), new S(-18, 41), new S(-10, 43), new S(12, 37), new S(5, 36), new S(22, 30), new S(33, 24), new S(13, 29),
        new S(-42, 29), new S(-43, 41), new S(-39, 42), new S(-30, 35), new S(-30, 34), new S(-20, 35), new S(7, 25), new S(-17, 22),
        new S(-46, 13), new S(-40, 23), new S(-44, 23), new S(-34, 19), new S(-31, 18), new S(-22, 17), new S(7, 7), new S(-20, 5),
        new S(-59, 10), new S(-39, 7), new S(-33, 12), new S(-26, 8), new S(-25, 6), new S(-10, -1), new S(0, -6), new S(-48, 5),
        new S(-24, 8), new S(-23, 12), new S(-17, 16), new S(-10, 9), new S(-9, 7), new S(-5, 15), new S(2, 3), new S(-12, -9)
    };
    public static readonly S[] QueenPsqt = {
        new S(-32, 50), new S(-23, 65), new S(-13, 75), new S(5, 74), new S(24, 81), new S(89, 53), new S(91, 40), new S(76, 47),
        new S(-37, 38), new S(-67, 75), new S(-33, 76), new S(-48, 111), new S(-27, 129), new S(41, 87), new S(-20, 111), new S(55, 60),
        new S(-31, 18), new S(-27, 29), new S(-30, 62), new S(-13, 74), new S(11, 97), new S(63, 96), new S(72, 78), new S(36, 95),
        new S(-22, 2), new S(-20, 35), new S(-24, 46), new S(-19, 76), new S(-12, 98), new S(2, 106), new S(13, 105), new S(16, 82),
        new S(-12, -12), new S(-12, 22), new S(-12, 28), new S(-13, 60), new S(-14, 59), new S(-3, 60), new S(4, 45), new S(11, 58),
        new S(-12, -23), new S(-3, -7), new S(-4, 13), new S(-7, 3), new S(-6, 8), new S(-1, 21), new S(11, 3), new S(11, -10),
        new S(-9, -37), new S(-3, -28), new S(3, -39), new S(3, -23), new S(4, -30), new S(15, -68), new S(20, -82), new S(8, -57),
        new S(-4, -40), new S(-4, -46), new S(2, -50), new S(6, -29), new S(7, -53), new S(-13, -49), new S(-5, -73), new S(-10, -41)
    };
    public static readonly S[] KingPsqt = {
        new S(36, -144), new S(109, -46), new S(93, -15), new S(81, -8), new S(75, -12), new S(88, -13), new S(82, -8), new S(29, -119),
        new S(27, -34), new S(102, 13), new S(130, 9), new S(110, 12), new S(110, 15), new S(117, 16), new S(92, 30), new S(25, -29),
        new S(5, 9), new S(99, 28), new S(97, 35), new S(69, 36), new S(100, 26), new S(123, 38), new S(101, 34), new S(-10, 10),
        new S(-33, 14), new S(58, 27), new S(46, 48), new S(16, 53), new S(15, 49), new S(44, 41), new S(32, 31), new S(-73, 17),
        new S(-59, -3), new S(8, 20), new S(12, 40), new S(-32, 56), new S(-23, 50), new S(-28, 39), new S(-9, 18), new S(-99, 7),
        new S(-67, -2), new S(-27, 11), new S(-38, 29), new S(-53, 42), new S(-47, 37), new S(-52, 28), new S(-27, 4), new S(-72, -2),
        new S(-14, -14), new S(-25, -1), new S(-33, 13), new S(-70, 20), new S(-57, 17), new S(-59, 16), new S(-10, -9), new S(-9, -30),
        new S(-27, -61), new S(18, -38), new S(2, -21), new S(-72, -21), new S(-12, -53), new S(-69, -12), new S(12, -41), new S(3, -85)
    };

    // Piece Features
    public static readonly S BishopPairBonus = new S(15, 74);
    public static readonly S[] KnightMobilityBonus = {new S(-18, -73), new S(-6, -16), new S(-2, 13), new S(1, 29), new S(5, 38), new S(8, 46), new S(13, 45), new S(20, 38), new S(30, 20)};
    public static readonly S[] BishopMobilityBonus = {new S(-14, -63), new S(-11, -28), new S(-3, -9), new S(0, 9), new S(6, 25), new S(9, 38), new S(11, 46), new S(12, 50), new S(15, 54), new S(20, 55), new S(29, 50), new S(44, 47), new S(55, 56), new S(75, 33)};
    public static readonly S[] RookMobilityBonus = {new S(-23, -18), new S(-19, 11), new S(-16, 18), new S(-14, 26), new S(-15, 38), new S(-11, 42), new S(-9, 49), new S(-6, 51), new S(-2, 54), new S(1, 58), new S(4, 60), new S(7, 62), new S(14, 61), new S(28, 49), new S(75, 24)};
    public static readonly S[] QueenMobilityBonus = {new S(-7, -69), new S(-10, -109), new S(-11, -56), new S(-9, -24), new S(-8, 1), new S(-7, 17), new S(-5, 33), new S(-4, 47), new S(-1, 56), new S(1, 63), new S(3, 70), new S(5, 75), new S(7, 79), new S(8, 84), new S(8, 88), new S(7, 93), new S(7, 97), new S(9, 96), new S(12, 95), new S(18, 92), new S(23, 90), new S(34, 81), new S(37, 77), new S(55, 69), new S(43, 74), new S(66, 65), new S(51, 73), new S(48, 72)};
    public static readonly S OutpostBonus = new S(26, 13);
    public static readonly S OpenFileBonus = new S(33, 7);
    public static readonly S SemiFileBonus = new S(15, 12);

    // Pawn Features
    public static readonly S[] PassedPawnBonus = {new S(0, 0), new S(-7, 16), new S(-18, 20), new S(-12, 44), new S(15, 64), new S(45, 111), new S(74, 107), new S(0, 0)};
    public static readonly S PassedPawnProtectedBonus = new S(19, 8);
    public static readonly S[] PassedPawnBlockedPenalty = {new S(16, 33), new S(11, 40), new S(-6, 21), new S(-1, 24), new S(59, 24)};
    public static readonly S DoubledPawnPenalty = new S(5, 10);
    public static readonly S[] IsolatedPawnPenaltyByCount = {new S(-15, -30), new S(-9, -18), new S(-0, -1), new S(12, 11), new S(19, 26), new S(19, 47), new S(30, 66), new S(32, 54), new S(35, 60)};

    // King Safety
    public static readonly S PawnShelterMissingPenalty = new S(18, -4);
    public static readonly S PawnShelterWeakPenalty = new S(11, -10);
    public static readonly S KingFileOpenPenalty = new S(27, 5);
    public static readonly S KingFileSemiOpenPenalty = new S(11, -12);
    public static readonly S[] PawnStormPenaltyByDistance = {new S(6, -136), new S(60, -85), new S(12, -17), new S(1, -0)};


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