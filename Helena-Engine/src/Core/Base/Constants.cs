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

    public const int MaxKillerPly = 32;
}