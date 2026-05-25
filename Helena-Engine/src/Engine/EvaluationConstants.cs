namespace H.Engine;

using S = TaperedScore;

public static class EvaluationConstants
{
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

    // King Safety
    public static readonly S PawnShelterMissingPenalty = new S(15, 0);
    public static readonly S PawnShelterWeakPenalty = new S(10, 0);
    public static readonly S KingFileOpenPenalty = new S(20, 0);
    public static readonly S KingFileSemiOpenPenalty = new S(10, 0);
    // UNUSED
    // public static readonly int[] AttackerWeight = { 0, 0, 2, 2, 3, 5, 0 }; // None, Pawn, Knight, Bishop, Rook, Queen, King
    // public const int KingSafetyPenaltyTableLength = 100;
    // public static readonly S[] KingSafetyPenaltyTable = new S[KingSafetyPenaltyTableLength];

    // Absolute (non-tapered) material values used by SEE and MoveOrdering.
    public static readonly int[] AbsoluteMaterial = { 0, 100, 300, 300, 500, 900, 0 };
}