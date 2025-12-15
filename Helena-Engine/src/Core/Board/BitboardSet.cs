namespace H.Core;

public class BitboardSet
{
    // Never use set[type] directly. Correct usage: set[type - 1]
    // For safety and clearance, do this[type]
    Bitboard[] set;
    Bitboard all;

    // type - 1 because PAWN = 1
    public Bitboard this[PieceType type]
    {
        get
        {
            return set[type - 1];
        }
    }

    public void ToggleSquare(PieceType type, Square square)
    {
        set[type - 1].ToggleSquare(square);
        all.ToggleSquare(square);
    }
    public void ToggleSquare(PieceType type, Square s1, Square s2)
    {
        set[type - 1].ToggleSquare(s1, s2);
        all.ToggleSquare(s1, s2);
    }

    public Bitboard All => all;

    public BitboardSet()
    {
        set = new Bitboard[6];
        all = 0;
        Init();
    }

    public void Init()
    {
        for (int i = 0; i < 6; i++)
        {
            set[i] = 0;
        }

        all = 0;
    }
}