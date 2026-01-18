namespace H.Engine;

using S = TaperedScore;
using H.Core;

public static partial class TunedEvaluation
{
    public static readonly S[] MaterialValues = {new S(118, 159), new S(520, 604), new S(487, 563), new S(568, 980), new S(1532, 1907)};
    public static readonly S[] PawnPsqt = {
        new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0),
        new S(42, 101), new S(91, 58), new S(83, 100), new S(40, 70), new S(82, 89), new S(17, 74), new S(67, 106), new S(30, 46),
        new S(50, 55), new S(29, 41), new S(59, 51), new S(51, 31), new S(47, 46), new S(46, 35), new S(44, 35), new S(42, 45),
        new S(18, 32), new S(29, 33), new S(9, 40), new S(51, 23), new S(20, 27), new S(40, 8), new S(11, 36), new S(39, 6),
        new S(11, 32), new S(17, 17), new S(5, 39), new S(29, 16), new S(23, 16), new S(16, 13), new S(16, 4), new S(14, 19),
        new S(9, 9), new S(19, 25), new S(12, 21), new S(2, 24), new S(5, 20), new S(2, -3), new S(15, 15), new S(11, -3),
        new S(7, 15), new S(19, 5), new S(13, 20), new S(-13, 15), new S(-15, 5), new S(14, 2), new S(25, 1), new S(9, 8),
        new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0)
    };
    public static readonly S[] KnightPsqt = {
        new S(104, -11), new S(-103, -38), new S(37, 11), new S(-53, -6), new S(-11, -3), new S(-74, -29), new S(-41, -36), new S(-43, -94),
        new S(6, -0), new S(26, -15), new S(63, 18), new S(16, -18), new S(12, 26), new S(-20, -22), new S(-5, 4), new S(-16, -38),
        new S(3, -7), new S(27, -5), new S(32, 25), new S(45, 8), new S(54, 15), new S(21, 3), new S(16, 7), new S(-15, -20),
        new S(-30, 6), new S(37, 9), new S(23, 26), new S(52, 9), new S(18, 36), new S(56, 1), new S(25, 20), new S(-11, -17),
        new S(-22, 1), new S(17, 19), new S(26, 19), new S(34, 21), new S(30, 11), new S(33, 19), new S(26, -1), new S(-8, -10),
        new S(-8, -25), new S(15, 14), new S(19, 8), new S(32, 11), new S(36, 25), new S(16, -8), new S(14, 15), new S(-16, -41),
        new S(-35, -14), new S(-3, -1), new S(17, 8), new S(9, 1), new S(13, -5), new S(10, 17), new S(-1, -9), new S(-24, -41),
        new S(-42, -5), new S(-37, -51), new S(-13, -8), new S(-15, -5), new S(-16, 4), new S(-6, -23), new S(-31, -42), new S(-24, -24)
    };
    public static readonly S[] BishopPsqt = {
        new S(45, 22), new S(-8, 13), new S(30, 34), new S(5, 12), new S(-16, 16), new S(-68, 5), new S(-17, 40), new S(-5, -15),
        new S(58, 24), new S(40, -1), new S(66, 41), new S(-7, -17), new S(79, 17), new S(21, 5), new S(45, 29), new S(-19, 14),
        new S(-6, 13), new S(15, 5), new S(34, 12), new S(40, 12), new S(9, 28), new S(35, 23), new S(-11, 37), new S(24, -10),
        new S(-4, 9), new S(32, -5), new S(16, 31), new S(54, 1), new S(41, 29), new S(39, 12), new S(23, 26), new S(13, -1),
        new S(8, -12), new S(28, -2), new S(17, -2), new S(38, 17), new S(27, 27), new S(35, 28), new S(38, 13), new S(3, -1),
        new S(2, 4), new S(32, 5), new S(33, 28), new S(20, 1), new S(21, 27), new S(25, 21), new S(29, 26), new S(13, 3),
        new S(12, -7), new S(15, 1), new S(20, 2), new S(10, 8), new S(18, 8), new S(18, 20), new S(18, 13), new S(10, 6),
        new S(3, 13), new S(15, 13), new S(4, 2), new S(17, 9), new S(10, 30), new S(4, 8), new S(4, 13), new S(-8, 17)
    };
    public static readonly S[] RookPsqt = {
        new S(34, 59), new S(-3, 40), new S(19, 27), new S(-3, 9), new S(-3, 33), new S(-48, 24), new S(-10, 55), new S(-45, 30),
        new S(72, 73), new S(29, 18), new S(69, 58), new S(64, 31), new S(66, 45), new S(35, 40), new S(43, 46), new S(31, 42),
        new S(21, 27), new S(25, 32), new S(40, 29), new S(32, 30), new S(25, 26), new S(36, 24), new S(-5, 36), new S(7, 23),
        new S(-5, 46), new S(35, 22), new S(17, 51), new S(38, 29), new S(31, 47), new S(26, 26), new S(15, 32), new S(6, 33),
        new S(-3, 23), new S(9, 39), new S(16, 31), new S(29, 47), new S(40, 42), new S(14, 31), new S(-8, 47), new S(7, 17),
        new S(6, 41), new S(9, 18), new S(14, 41), new S(20, 27), new S(21, 34), new S(19, 24), new S(26, 10), new S(0, 15),
        new S(-4, 12), new S(9, 35), new S(23, 36), new S(16, 40), new S(22, 27), new S(15, 34), new S(18, 21), new S(1, 22),
        new S(-9, -19), new S(-5, 8), new S(13, 38), new S(29, 33), new S(19, 37), new S(5, 1), new S(-6, -5), new S(-9, -21)
    };
    public static readonly S[] QueenPsqt = {
        new S(25, 16), new S(69, -33), new S(45, 15), new S(17, -41), new S(56, -20), new S(16, -58), new S(60, 8), new S(-2, -46),
        new S(54, 83), new S(73, 65), new S(91, 36), new S(70, 6), new S(73, 11), new S(60, 27), new S(57, 36), new S(33, 43),
        new S(22, 91), new S(66, 69), new S(53, 67), new S(80, 38), new S(37, 20), new S(78, 20), new S(20, 63), new S(31, 42),
        new S(26, 97), new S(49, 50), new S(42, 97), new S(44, 53), new S(58, 15), new S(52, 30), new S(41, 8), new S(31, 28),
        new S(26, 44), new S(50, 49), new S(55, 43), new S(41, 52), new S(42, 52), new S(46, 20), new S(45, 1), new S(31, 3),
        new S(27, 35), new S(35, 23), new S(41, 45), new S(41, 33), new S(42, 36), new S(29, 24), new S(35, 7), new S(27, 1),
        new S(26, 31), new S(29, 49), new S(37, 23), new S(32, 30), new S(34, 23), new S(38, 37), new S(39, 36), new S(34, 26),
        new S(10, 37), new S(21, 47), new S(13, 35), new S(21, -29), new S(25, 35), new S(30, 22), new S(31, 40), new S(-10, 75)
    };
    public static readonly S[] KingPsqt = {
        new S(-132, 31), new S(-159, 17), new S(-115, 35), new S(-151, -4), new S(-22, 13), new S(-129, -40), new S(-97, -15), new S(-73, -2),
        new S(-90, 33), new S(-59, 2), new S(-12, 4), new S(-77, -1), new S(-67, -1), new S(-70, 14), new S(-76, 14), new S(-117, 27),
        new S(-52, 19), new S(-87, 5), new S(-48, 28), new S(-67, 12), new S(-73, 48), new S(-35, 13), new S(-92, 16), new S(-74, 3),
        new S(-63, 16), new S(-39, -11), new S(-81, 37), new S(-52, 36), new S(-42, 19), new S(-29, 32), new S(-52, -12), new S(-42, 7),
        new S(3, -10), new S(-10, -17), new S(-29, 30), new S(-26, 15), new S(-20, 30), new S(-3, 4), new S(-33, -10), new S(-32, -18),
        new S(24, -8), new S(19, -21), new S(21, 6), new S(4, 7), new S(5, 4), new S(14, 6), new S(14, -29), new S(11, -22),
        new S(32, -29), new S(38, -18), new S(22, 4), new S(21, -3), new S(21, 1), new S(21, -3), new S(31, -21), new S(36, -35),
        new S(37, -29), new S(30, -24), new S(24, -33), new S(18, -11), new S(18, -38), new S(20, -6), new S(26, -39), new S(11, -2)
    };
    public static readonly S OutpostBonus = new S(23, 24);
    public static readonly S OpenFileBonus = new S(44, 0);
    public static readonly S SemiFileBonus = new S(16, 22);
    public static readonly S[] PassedPawnBonus = {new S(0, 0), new S(-17, 2), new S(-23, 14), new S(-26, 58), new S(21, 100), new S(72, 197), new S(112, 279), new S(0, 0)};
    public static readonly S IsolatedPawnPenaltyPerPawn = new S(17, 21);

    // King Safety
    public static readonly S PawnShelterMissingPenalty = new S(17, -2);
    public static readonly S PawnShelterWeakPenalty = new S(11, 0);
    public static readonly S KingFileOpenPenalty = new S(24, 1);
    public static readonly S KingFileSemiOpenPenalty = new S(10, -5);



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