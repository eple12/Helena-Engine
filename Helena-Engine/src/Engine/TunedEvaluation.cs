namespace H.Engine;

using S = TaperedScore;
using H.Core;

public static partial class TunedEvaluation
{
    public static readonly S[] MaterialValues = {new S(144, 111), new S(427, 309), new S(458, 334), new S(653, 538), new S(1199, 924)};
    public static readonly S[] PawnPsqt = {
        new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0),
        new S(41, 95), new S(71, 74), new S(61, 90), new S(70, 69), new S(80, 82), new S(45, 80), new S(38, 101), new S(7, 113),
        new S(19, 55), new S(15, 54), new S(38, 49), new S(48, 50), new S(52, 50), new S(42, 47), new S(23, 49), new S(19, 52),
        new S(11, 30), new S(14, 28), new S(16, 29), new S(35, 26), new S(36, 26), new S(16, 28), new S(10, 29), new S(3, 34),
        new S(4, 22), new S(5, 20), new S(5, 19), new S(30, 15), new S(25, 20), new S(5, 19), new S(2, 22), new S(-4, 22),
        new S(8, 12), new S(2, 10), new S(-6, 10), new S(4, 10), new S(4, 9), new S(-6, 9), new S(-4, 10), new S(1, 13),
        new S(5, 10), new S(13, 7), new S(10, 7), new S(-21, 5), new S(-21, 6), new S(9, 11), new S(8, 10), new S(-2, 13),
        new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0)
    };
    public static readonly S[] KnightPsqt = {
        new S(-74, -62), new S(-54, -42), new S(-42, -25), new S(-34, -40), new S(-24, -64), new S(-61, -41), new S(-41, -63), new S(-59, -32),
        new S(-5, -33), new S(46, -27), new S(53, -0), new S(45, -15), new S(-11, -6), new S(-1, 11), new S(4, -10), new S(-42, -53),
        new S(-7, -22), new S(9, 6), new S(29, 16), new S(36, 18), new S(66, 5), new S(9, 5), new S(9, 4), new S(-42, -34),
        new S(-26, -19), new S(7, 7), new S(29, 22), new S(26, 30), new S(22, 29), new S(20, 24), new S(4, 4), new S(-16, -36),
        new S(-32, -26), new S(5, 8), new S(18, 26), new S(25, 26), new S(23, 31), new S(18, 24), new S(5, 2), new S(-33, -32),
        new S(-39, -34), new S(4, 4), new S(4, 10), new S(17, 20), new S(13, 22), new S(6, 5), new S(6, 1), new S(-38, -26),
        new S(-36, -44), new S(-28, -9), new S(-6, 1), new S(-0, 3), new S(-1, 6), new S(0, -13), new S(-23, -31), new S(-47, -51),
        new S(-54, -39), new S(-54, -43), new S(-29, -28), new S(-34, -41), new S(-35, -33), new S(-32, -38), new S(-55, -46), new S(-53, -10)
    };
    public static readonly S[] BishopPsqt = {
        new S(-50, -30), new S(-19, -18), new S(-1, -27), new S(-34, 2), new S(-29, 5), new S(-68, -6), new S(-7, -36), new S(-20, -39),
        new S(32, -10), new S(68, 1), new S(39, 0), new S(22, 12), new S(40, 6), new S(-2, 15), new S(43, 7), new S(-20, 6),
        new S(25, -18), new S(3, 12), new S(21, 18), new S(25, 16), new S(14, 12), new S(28, 15), new S(11, 14), new S(9, -4),
        new S(11, -13), new S(6, 14), new S(16, 17), new S(23, 16), new S(20, 19), new S(13, 12), new S(0, 13), new S(0, -1),
        new S(-4, -10), new S(2, 7), new S(6, 14), new S(15, 15), new S(13, 17), new S(7, 14), new S(10, -2), new S(-7, -19),
        new S(-14, -16), new S(7, 5), new S(8, 13), new S(4, 15), new S(5, 9), new S(11, 10), new S(10, 4), new S(-11, -13),
        new S(-6, -28), new S(1, -8), new S(-5, -8), new S(-6, -4), new S(-6, -6), new S(-5, -1), new S(1, -1), new S(-8, -29),
        new S(-22, -16), new S(-12, -10), new S(-24, -17), new S(-11, -12), new S(-18, -13), new S(-15, -20), new S(-12, -8), new S(-22, -31)
    };
    public static readonly S[] RookPsqt = {
        new S(21, -0), new S(15, -1), new S(25, -8), new S(25, -5), new S(-6, 2), new S(-5, 5), new S(2, 8), new S(-47, 24),
        new S(33, 3), new S(46, 1), new S(50, 1), new S(39, 8), new S(30, 8), new S(27, 4), new S(11, 8), new S(18, 6),
        new S(9, 3), new S(24, 0), new S(17, 13), new S(12, 11), new S(26, 10), new S(31, 4), new S(23, -1), new S(4, 9),
        new S(6, 13), new S(7, 15), new S(-2, 24), new S(5, 23), new S(11, 16), new S(-1, 17), new S(-19, 12), new S(-5, 17),
        new S(-8, 6), new S(3, 16), new S(2, 9), new S(-1, 10), new S(0, 9), new S(-2, 3), new S(-5, 1), new S(-7, 1),
        new S(-13, -3), new S(-5, 6), new S(-4, 2), new S(-4, -3), new S(-11, 2), new S(-11, -2), new S(-5, -0), new S(-20, -2),
        new S(-6, -14), new S(-4, -9), new S(-2, -7), new S(1, -9), new S(-8, -7), new S(-2, -16), new S(-3, -12), new S(-6, -7),
        new S(-9, -15), new S(-4, -11), new S(-7, -10), new S(-2, -9), new S(-0, -14), new S(-10, -22), new S(-4, -21), new S(-13, -15)
    };
    public static readonly S[] QueenPsqt = {
        new S(-37, -34), new S(-22, -27), new S(-3, -41), new S(19, -59), new S(-14, -41), new S(-28, -16), new S(-20, -42), new S(-76, -37),
        new S(27, 4), new S(86, -21), new S(54, -19), new S(72, -35), new S(41, -30), new S(-3, 20), new S(41, -18), new S(12, -30),
        new S(3, 42), new S(18, 48), new S(41, 30), new S(41, 21), new S(6, 1), new S(15, 9), new S(2, -9), new S(-17, 4),
        new S(14, 48), new S(-2, 47), new S(32, 41), new S(23, 30), new S(34, 3), new S(26, -18), new S(-1, -12), new S(-4, -13),
        new S(1, 18), new S(3, 26), new S(1, 33), new S(9, 27), new S(8, 20), new S(3, 21), new S(-4, 9), new S(-10, -4),
        new S(-7, -11), new S(2, 3), new S(0, 16), new S(-2, 12), new S(-2, 17), new S(-2, 17), new S(3, -12), new S(-13, -15),
        new S(-11, -17), new S(1, -8), new S(1, -1), new S(-4, -1), new S(-7, 2), new S(-10, 6), new S(-3, -6), new S(-6, -6),
        new S(-13, -34), new S(-15, -17), new S(-17, -8), new S(-16, -6), new S(-12, -10), new S(-4, -26), new S(-12, -31), new S(-24, -22)
    };
    public static readonly S[] KingPsqt = {
        new S(-75, 10), new S(-70, 21), new S(-65, -3), new S(-67, -26), new S(-66, 2), new S(-64, 6), new S(-73, 12), new S(-81, 16),
        new S(-56, 11), new S(-58, 22), new S(-50, 15), new S(-57, -1), new S(-56, -14), new S(-62, 28), new S(-61, 21), new S(-63, 11),
        new S(-39, 8), new S(-49, 16), new S(-61, 25), new S(-49, 11), new S(-71, 16), new S(-63, 31), new S(-65, 27), new S(-44, 4),
        new S(-42, -10), new S(-68, 2), new S(-87, 40), new S(-94, 32), new S(-54, 31), new S(-78, 45), new S(-71, 9), new S(-36, -11),
        new S(-24, -24), new S(-36, -12), new S(-50, 26), new S(-80, 35), new S(-48, 31), new S(-53, 32), new S(-21, -19), new S(-1, -35),
        new S(-46, -8), new S(-26, -17), new S(-10, 8), new S(-29, 14), new S(-16, 11), new S(-3, 8), new S(-10, -30), new S(12, -42),
        new S(23, -17), new S(22, -22), new S(9, -11), new S(11, -22), new S(10, -22), new S(14, -14), new S(42, -44), new S(43, -47),
        new S(39, -34), new S(53, -45), new S(20, -45), new S(13, -55), new S(5, -58), new S(36, -53), new S(51, -49), new S(46, -59)
    };
    public static readonly S OutpostBonus = new S(24, 27);
    public static readonly S OpenFileBonus = new S(27, -4);
    public static readonly S SemiFileBonus = new S(13, 22);
    public static readonly S[] PassedPawnBonus = {new S(0, 0), new S(4, 2), new S(8, 6), new S(20, 17), new S(27, 30), new S(38, 33), new S(61, 87), new S(0, 0)};
    public static readonly S IsolatedPawnPenaltyPerPawn = new S(6, 7);

    // King Safety
    public static readonly S PawnShelterMissingPenalty = new S(16, -5);
    public static readonly S PawnShelterWeakPenalty = new S(10, -2);
    public static readonly S KingFileOpenPenalty = new S(19, 3);
    public static readonly S KingFileSemiOpenPenalty = new S(12, -9);

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