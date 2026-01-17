namespace H.Core;

public static class Constants
{
    public const int INF = 999_999_999;
    public const int MIN_THINKTIME = 100;

    public const int MAX_MOVES = 256;

    public const int ZOBRIST_SEED = 29426028;

    // Engine Configs
    public const int TT_SIZE_MB = 64;
    public const int MAX_DEPTH = 99;

    public const int AspirationWindowDepth = 8;
    public const int AspirationWindowBase = 20;
    public const int MaxAspirations = 3;

    public const int LMR_MinDepth = 3;
    public const int LMR_MinFullSearchMoves = 3;
    public const double LMR_Divisor = 3.49;
    public const double LMR_Base = 0.75;
    public static readonly int[][] LMR_Reductions = new int[MAX_DEPTH + 1][];

    public const int SEE_BadCaptureReduction = 2;

    public const int MaxKillerPly = 32;

    static Constants()
    {
        for (int searchDepth = 1; searchDepth < MAX_DEPTH + 1; ++searchDepth) {
            LMR_Reductions[searchDepth] = new int[MAX_MOVES];
            
            // movesSearchedCount > 0 or we wouldn't be applying LMR
            for (int movesSearchedCount = 1; movesSearchedCount < MAX_MOVES; ++movesSearchedCount)
            {
                LMR_Reductions[searchDepth][movesSearchedCount] = Convert.ToInt32(Math.Round(
                    LMR_Base + (Math.Log(movesSearchedCount) * Math.Log(searchDepth) / LMR_Divisor)
                ));
            }
        }
    }
}