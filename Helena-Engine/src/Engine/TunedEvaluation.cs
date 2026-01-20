namespace H.Engine;

using S = TaperedScore;
using H.Core;

public static partial class TunedEvaluation
{
    public static readonly S[] MaterialValues = {new S(133, 113), new S(354, 333), new S(379, 357), new S(562, 566), new S(1019, 986)};
    public static readonly S[] PawnPsqt = {
        new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0),
        new S(48, 91), new S(85, 54), new S(78, 107), new S(42, 69), new S(61, 102), new S(9, 78), new S(68, 122), new S(10, 62),
        new S(43, 56), new S(24, 39), new S(50, 51), new S(44, 32), new S(39, 48), new S(34, 33), new S(24, 40), new S(29, 44),
        new S(18, 26), new S(31, 26), new S(9, 34), new S(54, 13), new S(22, 21), new S(32, 2), new S(1, 33), new S(28, 5),
        new S(14, 25), new S(19, 12), new S(9, 32), new S(35, 9), new S(26, 11), new S(14, 7), new S(8, 1), new S(10, 13),
        new S(13, 5), new S(19, 18), new S(13, 14), new S(7, 19), new S(9, 16), new S(0, -8), new S(9, 8), new S(8, -7),
        new S(11, 9), new S(21, -2), new S(16, 13), new S(-9, 15), new S(-10, 5), new S(12, 2), new S(18, -3), new S(5, 7),
        new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0)
    };
    public static readonly S[] KnightPsqt = {
        new S(-51, -18), new S(-126, -58), new S(10, -2), new S(-71, -26), new S(1, -35), new S(-88, -33), new S(-36, -57), new S(-88, -86),
        new S(21, -18), new S(43, -38), new S(71, -3), new S(11, -36), new S(28, 7), new S(-21, -31), new S(7, -2), new S(-21, -58),
        new S(8, -20), new S(13, -13), new S(24, 13), new S(41, -4), new S(63, -4), new S(42, -11), new S(12, 1), new S(-18, -28),
        new S(-34, -9), new S(28, -5), new S(16, 19), new S(43, 2), new S(11, 31), new S(41, -2), new S(12, 9), new S(-8, -37),
        new S(-28, -10), new S(12, 2), new S(18, 15), new S(31, 13), new S(23, 7), new S(27, 14), new S(10, -6), new S(-18, -21),
        new S(-20, -30), new S(6, 5), new S(9, 4), new S(23, 2), new S(22, 19), new S(8, -15), new S(8, 6), new S(-31, -35),
        new S(-37, -27), new S(-14, -6), new S(5, -3), new S(2, -6), new S(5, -7), new S(3, -5), new S(-8, -24), new S(-38, -44),
        new S(-50, -13), new S(-45, -41), new S(-20, -16), new S(-20, -24), new S(-21, -13), new S(-15, -30), new S(-41, -26), new S(-43, 4)
    };
    public static readonly S[] BishopPsqt = {
        new S(-50, -6), new S(-33, -10), new S(31, -6), new S(-25, -4), new S(-44, 7), new S(-102, -14), new S(-8, -9), new S(-21, -51),
        new S(42, -5), new S(43, -22), new S(63, 18), new S(-5, -34), new S(58, 7), new S(12, -4), new S(49, 8), new S(-13, -13),
        new S(12, -18), new S(9, -11), new S(28, 6), new S(32, 4), new S(13, 12), new S(33, 17), new S(-8, 19), new S(21, -22),
        new S(-3, -12), new S(23, -12), new S(5, 22), new S(43, -3), new S(25, 29), new S(27, 4), new S(6, 22), new S(5, -10),
        new S(-3, -23), new S(12, -10), new S(5, -8), new S(23, 12), new S(15, 18), new S(20, 22), new S(28, 2), new S(-7, -18),
        new S(-10, -13), new S(17, -16), new S(16, 14), new S(6, -6), new S(8, 15), new S(15, 11), new S(15, 14), new S(-1, -7),
        new S(1, -35), new S(2, -13), new S(3, -19), new S(-3, -3), new S(8, -1), new S(3, 12), new S(7, 9), new S(-2, -13),
        new S(-12, -4), new S(0, -1), new S(-9, -0), new S(4, -5), new S(-4, 10), new S(-2, -0), new S(-9, 3), new S(-15, -8)
    };
    public static readonly S[] RookPsqt = {
        new S(40, 29), new S(12, 9), new S(33, 4), new S(37, -16), new S(33, 12), new S(-2, -11), new S(27, 9), new S(-62, 2),
        new S(60, 42), new S(27, -19), new S(68, 26), new S(50, 9), new S(51, 23), new S(31, 11), new S(35, 19), new S(30, 7),
        new S(11, -5), new S(25, -4), new S(22, 11), new S(21, 9), new S(14, 12), new S(36, -2), new S(5, 7), new S(12, -7),
        new S(-9, 18), new S(24, -9), new S(1, 29), new S(18, 11), new S(15, 27), new S(3, 11), new S(-13, 8), new S(-1, 7),
        new S(-9, -13), new S(7, 9), new S(3, 1), new S(14, 20), new S(20, 15), new S(-2, 6), new S(-9, 10), new S(-4, -15),
        new S(-6, 0), new S(-4, -13), new S(-0, 10), new S(5, -5), new S(-0, 10), new S(0, 1), new S(7, -9), new S(-17, -11),
        new S(-8, -28), new S(-3, -3), new S(10, 2), new S(6, 7), new S(3, 2), new S(5, 2), new S(7, -3), new S(-4, -6),
        new S(-20, -33), new S(-14, -20), new S(3, 11), new S(18, 12), new S(8, 10), new S(-7, -15), new S(-18, -26), new S(-23, -25)
    };
    public static readonly S[] QueenPsqt = {
        new S(-26, -17), new S(21, -62), new S(16, -10), new S(20, -63), new S(20, -27), new S(-24, -61), new S(22, -14), new S(-60, -76),
        new S(46, 11), new S(88, -24), new S(63, -4), new S(51, -31), new S(27, -9), new S(25, 9), new S(44, -9), new S(26, -26),
        new S(8, 30), new S(35, 34), new S(30, 31), new S(53, 7), new S(5, 7), new S(37, -3), new S(-7, 27), new S(-1, -4),
        new S(17, 44), new S(12, 26), new S(22, 67), new S(16, 40), new S(21, 11), new S(19, 6), new S(2, -18), new S(-1, -10),
        new S(-1, 8), new S(13, 31), new S(16, 35), new S(4, 41), new S(4, 33), new S(6, -1), new S(5, -7), new S(-9, -23),
        new S(-7, -5), new S(2, -1), new S(1, 26), new S(5, 14), new S(6, 12), new S(-5, 2), new S(3, -24), new S(-11, -24),
        new S(-1, -15), new S(1, 8), new S(2, 6), new S(-0, 9), new S(-1, 7), new S(1, 9), new S(7, 1), new S(4, -3),
        new S(-11, -13), new S(-7, 4), new S(-20, 7), new S(-15, -10), new S(-5, 3), new S(5, -12), new S(8, -24), new S(-27, 23)
    };
    public static readonly S[] KingPsqt = {
        new S(-66, 38), new S(-94, 13), new S(-50, 19), new S(-99, -34), new S(-38, 20), new S(-95, -45), new S(-75, -3), new S(-74, 17),
        new S(-47, 32), new S(-59, 8), new S(-37, 10), new S(-51, -14), new S(-50, -16), new S(-66, 19), new S(-75, 20), new S(-65, 21),
        new S(-52, 20), new S(-64, 8), new S(-80, 27), new S(-48, -6), new S(-90, 32), new S(-56, 12), new S(-99, 34), new S(-49, 3),
        new S(-71, 12), new S(-80, -4), new S(-111, 37), new S(-99, 26), new S(-53, 13), new S(-74, 40), new S(-80, 1), new S(-29, 1),
        new S(9, -19), new S(-24, -14), new S(-48, 26), new S(-68, 15), new S(-34, 26), new S(-29, 9), new S(-16, -15), new S(-5, -34),
        new S(-4, -5), new S(22, -24), new S(16, -1), new S(-17, 1), new S(-4, -1), new S(8, 4), new S(14, -30), new S(28, -32),
        new S(34, -25), new S(39, -19), new S(30, -6), new S(26, -17), new S(25, -12), new S(32, -11), new S(46, -29), new S(49, -39),
        new S(43, -32), new S(45, -38), new S(35, -40), new S(37, -33), new S(27, -50), new S(38, -23), new S(38, -43), new S(23, -1)
    };

    // Piece Features
    public static readonly S BishopPairBonus = new S(21, 14);
    public static readonly S[] KnightMobilityBonus = {new S(13, -4), new S(33, 30), new S(43, 39), new S(49, 43), new S(49, 42), new S(49, 47), new S(50, 45), new S(47, 47), new S(51, 37)};
    public static readonly S[] BishopMobilityBonus = {new S(10, 1), new S(31, 34), new S(41, 43), new S(48, 44), new S(53, 53), new S(53, 54), new S(55, 54), new S(55, 52), new S(52, 55), new S(53, 49), new S(51, 46), new S(51, 36), new S(39, 43), new S(42, 14)};
    public static readonly S[] RookMobilityBonus = {new S(7, -14), new S(27, 24), new S(37, 38), new S(43, 46), new S(48, 53), new S(47, 59), new S(46, 64), new S(46, 64), new S(46, 63), new S(45, 64), new S(47, 60), new S(45, 60), new S(43, 57), new S(40, 50), new S(57, 25)};
    public static readonly S[] QueenMobilityBonus = {new S(-6, 0), new S(23, 59), new S(34, 43), new S(41, 60), new S(47, 60), new S(46, 67), new S(47, 69), new S(46, 74), new S(46, 78), new S(46, 76), new S(47, 79), new S(46, 82), new S(48, 81), new S(48, 83), new S(45, 82), new S(52, 77), new S(56, 69), new S(64, 56), new S(64, 52), new S(72, 35), new S(76, 26), new S(71, 17), new S(124, -22), new S(93, -15), new S(52, -4), new S(47, -3), new S(16, -20), new S(27, -5)};
    public static readonly S OutpostBonus = new S(21, 31);
    public static readonly S OpenFileBonus = new S(43, 1);
    public static readonly S SemiFileBonus = new S(13, 23);

    // Pawn Features
    public static readonly S[] PassedPawnBonus = {new S(0, 0), new S(0, 11), new S(10, 18), new S(21, 62), new S(28, 104), new S(69, 194), new S(80, 314), new S(0, 0)};
    public static readonly S PassedPawnProtectedBonus = new S(50, 74);
    public static readonly S[] PassedPawnBlockedPenalty = {new S(5, 17), new S(-2, 23), new S(-6, 21), new S(-4, 4), new S(-2, 16)};
    public static readonly S DoubledPawnPenalty = new S(8, 9);
    public static readonly S[] IsolatedPawnPenaltyByCount = {new S(-3, -2), new S(17, 30), new S(25, 42), new S(37, 54), new S(51, 66), new S(75, 67), new S(73, 71), new S(86, 83), new S(90, 100)};

    // King Safety
    public static readonly S PawnShelterMissingPenalty = new S(17, -3);
    public static readonly S PawnShelterWeakPenalty = new S(10, -2);
    public static readonly S KingFileOpenPenalty = new S(21, 1);
    public static readonly S KingFileSemiOpenPenalty = new S(11, -5);
    public static readonly S[] PawnStormPenaltyByDistance = {new S(53, -46), new S(36, -4), new S(23, -1), new S(15, -2)};



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