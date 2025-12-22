namespace H.Core;

public static class Zobrist 
{
    const int seed = Constants.ZOBRIST_SEED;
    static Random rng = new(seed);

    public static readonly ulong [,] Pieces = new ulong[12, 64];
    public static readonly ulong[] Castling = new ulong[16];
    public static readonly ulong[] EP = new ulong[8];
    public static readonly ulong STM = NextUlong();

    static Zobrist()
    {
        for (Square square = 0; square < 64; square++)
        {
            for (int piece = 0; piece < 12; piece++)
            {
                Pieces[piece, square] = NextUlong();
            }
        }

        for (int i = 0; i < Castling.Length; i++)
        {
            Castling[i] = NextUlong();
        }

        for (int i = 0; i < EP.Length; i++)
        {
            EP[i] = NextUlong();
        }
    }

    // Called initially after loading a position
    public static ulong GetZobristKey(Board board)
    {
        ulong key = 0;

        for (Square square = 0; square < 64; square++)
        {
            if (board.At(square) != PieceHelper.NONE)
            {
                key ^= Pieces[PieceHelper.GetPieceIndex(board.At(square)), square];
            }
        }

        if (board.State.EnPassantSquare != SquareHelper.INVALID_SQUARE)
        {
            int enpFile = SquareHelper.GetFile(board.State.EnPassantSquare);
            key ^= EP[enpFile];
        }

        if (board.State.SideToMove)
        {
            key ^= STM;
        }
        
        key ^= Castling[board.State.CastlingRights];

        return key;
    }

    static ulong NextUlong()
    {
        byte[] bytes = new byte[8];
        rng.NextBytes(bytes);
        return BitConverter.ToUInt64(bytes);
    }
}