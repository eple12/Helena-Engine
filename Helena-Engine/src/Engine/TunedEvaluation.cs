namespace H.Engine;

using S = TaperedScore;

public static partial class TunedEvaluation
{
    public static readonly S[] MaterialValues = {new S(144, 111), new S(424, 311), new S(456, 335), new S(650, 537), new S(1210, 911)};
    public static readonly S[] PawnPsqt = {
        new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0),
        new S(40, 97), new S(82, 72), new S(46, 93), new S(75, 67), new S(82, 81), new S(41, 83), new S(34, 102), new S(14, 116),
        new S(22, 55), new S(16, 55), new S(40, 50), new S(50, 51), new S(59, 51), new S(38, 46), new S(26, 48), new S(16, 52),
        new S(13, 29), new S(17, 26), new S(16, 28), new S(35, 27), new S(37, 25), new S(17, 25), new S(10, 29), new S(4, 33),
        new S(5, 22), new S(7, 19), new S(5, 19), new S(30, 14), new S(25, 20), new S(5, 19), new S(2, 22), new S(-4, 23),
        new S(9, 12), new S(3, 9), new S(-6, 11), new S(4, 11), new S(3, 9), new S(-6, 10), new S(-3, 11), new S(1, 14),
        new S(5, 9), new S(13, 5), new S(10, 7), new S(-22, 6), new S(-21, 6), new S(10, 10), new S(9, 9), new S(-3, 13),
        new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0), new S(0, 0)
    };
    public static readonly S[] KnightPsqt = {
        new S(-68, -75), new S(-90, -37), new S(-70, -20), new S(-36, -33), new S(20, -88), new S(-118, -15), new S(-46, -64), new S(-98, 10),
        new S(5, -34), new S(52, -26), new S(56, -3), new S(43, -18), new S(-8, -10), new S(5, 12), new S(17, -21), new S(-47, -55),
        new S(-3, -27), new S(8, 7), new S(36, 14), new S(39, 19), new S(69, 4), new S(9, 5), new S(2, 11), new S(-46, -26),
        new S(-19, -21), new S(8, 7), new S(30, 23), new S(27, 28), new S(25, 26), new S(20, 23), new S(7, 1), new S(-13, -34),
        new S(-30, -28), new S(6, 9), new S(20, 25), new S(26, 26), new S(25, 28), new S(20, 23), new S(7, 2), new S(-33, -33),
        new S(-38, -32), new S(6, 1), new S(5, 7), new S(18, 18), new S(15, 21), new S(6, 3), new S(7, -3), new S(-38, -26),
        new S(-27, -51), new S(-25, -13), new S(-5, 1), new S(1, 1), new S(1, 5), new S(1, -11), new S(-20, -30), new S(-49, -49),
        new S(-27, -41), new S(-55, -43), new S(-27, -32), new S(-29, -43), new S(-38, -34), new S(-29, -42), new S(-58, -49), new S(-61, 12)
    };
    public static readonly S[] BishopPsqt = {
        new S(-55, -30), new S(-54, -8), new S(20, -30), new S(-44, 3), new S(-28, 7), new S(-109, -3), new S(-22, -32), new S(-23, -39),
        new S(32, -10), new S(72, 3), new S(39, 1), new S(22, 16), new S(52, 3), new S(-5, 18), new S(52, 5), new S(-18, 12),
        new S(27, -16), new S(6, 10), new S(26, 16), new S(26, 16), new S(20, 11), new S(21, 16), new S(16, 14), new S(11, -6),
        new S(15, -15), new S(6, 15), new S(21, 16), new S(24, 16), new S(26, 18), new S(18, 11), new S(1, 14), new S(1, 0),
        new S(-1, -12), new S(5, 7), new S(7, 14), new S(19, 11), new S(17, 15), new S(7, 13), new S(6, -4), new S(-7, -20),
        new S(-13, -17), new S(7, 6), new S(10, 10), new S(4, 14), new S(6, 8), new S(12, 9), new S(10, 6), new S(-9, -15),
        new S(-4, -26), new S(2, -9), new S(-3, -9), new S(-6, -2), new S(-6, -7), new S(-2, -5), new S(2, -2), new S(-11, -29),
        new S(-24, -13), new S(-11, -8), new S(-24, -17), new S(-8, -12), new S(-14, -16), new S(-15, -21), new S(-16, -8), new S(-23, -29)
    };
    public static readonly S[] RookPsqt = {
        new S(15, 3), new S(19, -2), new S(29, -11), new S(21, -4), new S(-12, 3), new S(-21, 10), new S(-3, 8), new S(-39, 21),
        new S(37, 3), new S(50, 1), new S(56, 0), new S(36, 10), new S(30, 12), new S(31, 5), new S(-2, 10), new S(21, 7),
        new S(10, 2), new S(29, -0), new S(22, 13), new S(10, 13), new S(28, 11), new S(36, 0), new S(18, 0), new S(6, 8),
        new S(8, 13), new S(10, 15), new S(-4, 26), new S(8, 24), new S(11, 16), new S(-5, 18), new S(-25, 15), new S(-2, 16),
        new S(-8, 8), new S(1, 17), new S(2, 11), new S(-1, 13), new S(4, 8), new S(-2, 3), new S(-3, 2), new S(-4, 0),
        new S(-13, -4), new S(-4, 6), new S(-2, 0), new S(-7, -2), new S(-9, 2), new S(-9, -4), new S(-6, 4), new S(-19, -1),
        new S(-6, -16), new S(-7, -7), new S(-2, -4), new S(4, -10), new S(-8, -7), new S(-3, -14), new S(-6, -12), new S(-6, -10),
        new S(-10, -18), new S(-5, -9), new S(-7, -11), new S(-1, -10), new S(-1, -15), new S(-10, -24), new S(-4, -23), new S(-15, -18)
    };
    public static readonly S[] QueenPsqt = {
        new S(-34, -34), new S(-13, -32), new S(-7, -38), new S(29, -75), new S(-23, -45), new S(-49, -0), new S(2, -68), new S(-119, -9),
        new S(31, 1), new S(91, -26), new S(54, -19), new S(81, -42), new S(50, -41), new S(-17, 27), new S(64, -37), new S(20, -37),
        new S(5, 41), new S(16, 56), new S(40, 39), new S(47, 21), new S(6, -3), new S(5, 12), new S(14, -23), new S(-13, -1),
        new S(15, 51), new S(-2, 45), new S(24, 47), new S(21, 31), new S(37, 3), new S(29, -17), new S(-2, -11), new S(-8, -8),
        new S(2, 21), new S(4, 30), new S(3, 40), new S(10, 28), new S(7, 21), new S(6, 16), new S(-2, 7), new S(-11, -0),
        new S(-7, -12), new S(3, -1), new S(1, 13), new S(-2, 13), new S(-3, 19), new S(-2, 17), new S(4, -17), new S(-13, -6),
        new S(-12, -22), new S(3, -12), new S(1, -3), new S(-4, -1), new S(-8, -0), new S(-12, 8), new S(-3, -6), new S(-8, 2),
        new S(-14, -26), new S(-21, 4), new S(-16, -9), new S(-17, -6), new S(-12, -7), new S(-10, -21), new S(4, -61), new S(-21, -29)
    };
    public static readonly S[] KingPsqt = {
        new S(-55, 50), new S(-88, 29), new S(-60, -17), new S(-47, -34), new S(-43, -6), new S(-21, 10), new S(-82, 25), new S(-116, 40),
        new S(-80, 17), new S(-91, 19), new S(-59, 21), new S(-70, 1), new S(-36, -18), new S(-55, 32), new S(-65, 20), new S(-106, 15),
        new S(-37, 6), new S(-32, 10), new S(-87, 32), new S(3, 2), new S(-103, 22), new S(-73, 32), new S(-108, 33), new S(-42, 2),
        new S(-50, -2), new S(-97, 4), new S(-123, 42), new S(-130, 39), new S(-23, 24), new S(-97, 46), new S(-70, 7), new S(-57, -9),
        new S(6, -29), new S(-45, -9), new S(-49, 25), new S(-79, 34), new S(-47, 30), new S(-50, 31), new S(-10, -23), new S(20, -41),
        new S(-76, 3), new S(-21, -19), new S(-6, 7), new S(-25, 13), new S(-0, 7), new S(-1, 7), new S(-3, -32), new S(10, -43),
        new S(31, -22), new S(33, -26), new S(16, -13), new S(22, -24), new S(20, -27), new S(23, -18), new S(49, -48), new S(49, -51),
        new S(52, -38), new S(59, -46), new S(26, -49), new S(21, -59), new S(12, -64), new S(46, -58), new S(56, -53), new S(52, -63)
    };
    public static readonly S OutpostBonus = new S(25, 26);
    public static readonly S OpenFileBonus = new S(27, -4);
    public static readonly S SemiFileBonus = new S(13, 24);
    public static readonly S[] PassedPawnBonus = {new S(0, 0), new S(3, 3), new S(7, 5), new S(19, 17), new S(27, 30), new S(36, 32), new S(62, 88), new S(0, 0)};
    public static readonly S IsolatedPawnPenaltyPerPawn = new S(6, 7);

    // King Safety
    public static readonly S PawnShelterMissingPenalty = new S(16, -6);
    public static readonly S PawnShelterWeakPenalty = new S(9, -1);
    public static readonly S KingFileOpenPenalty = new S(19, 3);
    public static readonly S KingFileSemiOpenPenalty = new S(13, -9);

    // Mopup
    public static readonly S CloserToEnemyKing = new S(0, 50);
    public static readonly S EnemyKingCorner = new S(0, 50);
}